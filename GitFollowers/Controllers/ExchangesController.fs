namespace GitFollowers.Controllers


open FSharp.Control.Reactive
open GitFollowers
open GitFollowers.DTOs
open GitFollowers.Entities
open GitFollowers.Persistence
open GitFollowers.Services
open Xamarin.Essentials

module ExchangesController =

    let requestExchangesSubject = Subject<Selection>.broadcast

    let connectionChecker = Connectivity.ConnectivityChanged

    let isConnected =
        Connectivity.NetworkAccess = NetworkAccess.Internet
        || Connectivity.NetworkAccess = NetworkAccess.Local

    let loadCountriesInfo =
        ExchangeService.getSupportedCountriesInfo
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

    let loadExchangesFromRepo =
        async {
            let! exchanges =
                ExchangeRepository.Instance.GetAllExchanges.AsTask()
                |> Async.AwaitTask

            return exchanges |> Option.get
        }

    let saveCurrenciesToRepository currency =
        let data = Currency.fromDomain currency

        ExchangeRepository
            .Instance
            .InsertCurrency(data)
            .AsTask()
        |> Async.AwaitTask
        |> ignore

    let loadCurrenciesFromRepo =
        async {
            let! result =
                ExchangeRepository.Instance.GetAllCurrencies.AsTask()
                |> Async.AwaitTask

            return result |> Option.get
        }

    let insertOrUpdateExchangeToRepo exchange =
        ExchangeRepository
            .Instance
            .InsertExchange(exchange)
            .AsTask()
        |> Async.AwaitTask
        |> ignore

    let getExchangeFor selection =
        async {
            let! exchangeResult =
                (ExchangeService.getExchangeFor selection.first.code selection.second.code)
                    .AsTask()
                |> Async.AwaitTask

            match exchangeResult with
            | Ok exchange -> return Some exchange
            | Error _ -> return None
        }


    let updateExchangeData (selection: Selection) (exchange: DTOs.Exchange) (exchangesData: ResizeArray<Selection>) =
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

        if result.IsSome then
            let index =
                exchangesData
                |> Seq.findIndex
                    (fun sel ->
                        sel.first.code = selectionData.first.code
                        && sel.second.code = selectionData.second.code)

            exchangesData.[index] <- selectionData

            Exchange.fromDomain (
                exchange,
                selectionData.first.code,
                selectionData.first.name,
                selectionData.second.code,
                selectionData.second.name
            )
            |> insertOrUpdateExchangeToRepo
        else
            exchangesData.Add selectionData

            Exchange.fromDomain (
                exchange,
                selectionData.first.code,
                selectionData.first.name,
                selectionData.second.code,
                selectionData.second.name
            )
            |> insertOrUpdateExchangeToRepo
