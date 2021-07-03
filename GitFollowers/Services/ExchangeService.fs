namespace GitFollowers.Services

open System
open FSharp.Control.Reactive
open FSharp.Control.Tasks
open GitFollowers
open GitFollowers.DTOs

type ExchangesResult =
    | NetworkError
    | InternalError
    | WrongPairCurrency
    | DeserializationError of exn

module ExchangeService =
    let private getCountryInfoFor (currency: string) =
        let urlString =
            $"{URlConstants.countriesBaseUrl}%s{currency}"

        vtask {
            let! response = fetch urlString

            match response.StatusCode with
            | 200 ->
                return
                    response.Body
                    |> Json.deserialize<Country array>
                    |> Result.mapError DeserializationError
            | 500 -> return Error InternalError
            | error ->
                Console.WriteLine error
                return Error NetworkError
        }

    let getSupportedCountriesInfo =
        Countries.currencies
        |> Observable.toObservable
        |> Observable.flatmapAsync
            (fun currency ->
                async {
                    let! countryInfo =
                        (getCountryInfoFor currency).AsTask()
                        |> Async.AwaitTask

                    match countryInfo with
                    | Ok response ->
                        let currencies =
                            response
                            |> Array.map (fun c -> c.currencies)
                            |> Array.tryHead

                        return Some currencies
                    | Error _ -> return None
                })
        |> Observable.choose id

    let getExchanges (first: string) (second: string) =
        let urlString =
            $"{URlConstants.exchangesBaseUrl}%s{first}%s{second}&pairs=%s{second}%s{first}"

        vtask {
            let! response = fetch urlString

            match response.StatusCode with
            | 200 ->
                return
                    response.Body
                    |> Json.parseExchange
                    |> Result.mapError DeserializationError
            | 500 -> return Error InternalError
            | 400 -> return Error WrongPairCurrency
            | _ -> return Error NetworkError
        }
