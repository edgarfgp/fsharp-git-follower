namespace GitFollowers.Controllers

open System
open System.Diagnostics
open FSharp.Control.Reactive
open GitFollowers.DTOs
open GitFollowers.Services
open GitFollowers.Views

module ExchangeLoader =

    let didRequestFollowers = Subject<Selection>.broadcast

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
