namespace GitFollowers.Controllers

open System
open System.Reactive.Linq
open FSharp.Control.Reactive
open GitFollowers
open GitFollowers.Controllers
open GitFollowers.DTOs
open GitFollowers.Entities
open GitFollowers.Persistence
open GitFollowers.Services
open Xamarin.Essentials
open FSharp.Control.Tasks

type ExchangesController(repository: IRepository) =
    static let didRequestNewExchangeRate = Subject<Selection>.broadcast

    let requestExchangeUpdate = Subject<Unit>.broadcast

    let loadExchanges = Subject<unit>.broadcast

    let loadCurrencies = Subject<unit>.broadcast

    let exchangesData = ResizeArray<Selection>()
    
    let isConnected =
        Connectivity.NetworkAccess = NetworkAccess.Internet
        || Connectivity.NetworkAccess = NetworkAccess.Local

    let currenciesData = ResizeArray<CurrencyData>()

    let insertOrUpdateExchangeToRepo exchange =
        repository.InsertExchange(exchange).AsTask()
        |> Async.AwaitTask
        |> ignore

    let updateExchangeData selection (exchange: DTOs.Exchange) =
        let selectionData =
            { first =
                  { selection.first with
                        value = exchange.first }
              second =
                  { selection.second with
                        value = exchange.second } }

        let result =
            exchangesData
            |> Seq.tryFind
                (fun selection ->
                    selection.first.code = selectionData.first.code
                    && selection.second.code = selectionData.second.code)

        let exchangeToSave =
            Exchange.fromDomain (
                exchange,
                selectionData.first.code,
                selectionData.first.name,
                selectionData.second.code,
                selectionData.second.name
            )

        if result.IsSome then
            let index =
                exchangesData
                |> Seq.findIndex
                    (fun sel ->
                        sel.first.code = selectionData.first.code
                        && sel.second.code = selectionData.second.code)

            exchangesData.[index] <- selectionData
        else
            exchangesData.Add selectionData

        insertOrUpdateExchangeToRepo exchangeToSave

    let saveCurrenciesToRepository currency =
        let data = Currency.fromDomain currency
        repository.InsertCurrency(data).AsTask()
        |> Async.AwaitTask
        |> ignore

    member x.ConnectionChecker: IObservable<InternetConnectionEvent> =
        Connectivity.ConnectivityChanged
        |> Observable.bind
            (fun state ->
                match state.NetworkAccess with
                | NetworkAccess.Local -> Observable.Return(InternetConnectionEvent.Connected)
                | NetworkAccess.Internet -> Observable.Return(InternetConnectionEvent.Connected)
                | _ -> Observable.Return(InternetConnectionEvent.NotConnected))

    member x.LoadCountriesInfo =
        Countries.currencies
        |> Observable.toObservable
        |> Observable.flatmapAsync
            (fun currency ->
                async {
                    return!
                        ExchangeService
                            .getSupportedCountriesInfo(currency)
                            .AsTask()
                        |> Async.AwaitTask
                })
        |> Observable.choose id
        |> Observable.flatmapSeq
            (fun currencies ->
                currencies
                |> Seq.distinctBy (fun c -> c.code)
                |> Seq.distinctBy (fun c -> c.name)
                |> Seq.distinctBy (fun c -> c.symbol)
                |> Seq.sortDescending)
        |> Observable.map
            (fun currency ->
                { code = currency.code.Value
                  name = currency.name.Value
                  value = 0.
                  symbol = currency.symbol.Value })
        |> Observable.subscribe(saveCurrenciesToRepository)

    member x.LoadCurrencies =
        vtask {
            let! result =
                repository.GetAllCurrencies.AsTask()
                |> Async.AwaitTask

            result |> Option.get |> currenciesData.AddRange
            loadCurrencies.OnNext()
        }

    member x.LoadExchanges =
        vtask {
            let! result =
                repository.GetAllExchanges.AsTask()
                |> Async.AwaitTask

            match result with
            | ExchangesLoadedEvent.Loaded exchanges ->
                exchanges |> exchangesData.AddRange
                return ExchangesState.Loaded
            | ExchangesLoadedEvent.NotLoaded error ->
                printfn $"{error.Message}"
                return ExchangesState.NotLoaded
        }

    member x.PublishSelection selection =
        didRequestNewExchangeRate.OnNext(selection)

    member x.HandleRequestExchangeUpdate = requestExchangeUpdate

    member x.HandleLoadExchanges = loadExchanges

    member x.HandleLoadCurrenciesSubject = loadCurrencies

    member x.DidRequestExchangesSubject =
        didRequestNewExchangeRate
        |> Observable.subscribe
            (fun selection ->
                async {
                    let! result =
                        (ExchangeService.getExchangeFor selection.first.code selection.second.code)
                            .AsTask()
                        |> Async.AwaitTask

                    match result with
                    | Ok exchange ->
                        updateExchangeData selection exchange
                        requestExchangeUpdate.OnNext()
                    | Error error -> printfn $" ${error}No exchange found for ======> {selection}"
                }
                |> Async.Start)

    member x.IsConnected = isConnected

    member x.ExchangesData = exchangesData

    member x.CurrenciesData = currenciesData

    member x.FetchExchangeForCurrencies =
        exchangesData
        |> Observable.toObservable
        |> Observable.flatmapAsync
            (fun selection ->
                async {
                    let! result =
                        (ExchangeService.getExchangeFor selection.first.code selection.second.code)
                            .AsTask()
                        |> Async.AwaitTask

                    match result with
                    | Ok exchange ->
                        updateExchangeData selection exchange
                        printfn $"Updated with value {exchange}"
                        return ExchangesUpdate.Updated
                    | Error error ->
                        printfn $" No Updated with Error : {error}"
                        return ExchangesUpdate.NotUpdated
                })
