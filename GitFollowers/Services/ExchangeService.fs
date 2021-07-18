namespace GitFollowers.Services

open FSharp.Control.Reactive
open FSharp.Control.Tasks
open GitFollowers
open GitFollowers.DTOs

type ExchangesError =
    | BadRequest
    | Non200Response
    | ParseError of string

module ExchangeService =

    let private getCountryInfoFor (currency: string) =
        let urlString =
            $"{URlConstants.countriesBaseUrl}%s{currency}"

        vtask {
            let! response = fetch urlString

            return
                match response.StatusCode with
                | 200 ->
                    response.Body
                    |> Json.deserialize<Country array>
                    |> Result.mapError ParseError
                | _ -> Error Non200Response
        }

    let getSupportedCountriesInfo currency =
        vtask {
            let! countryInfo =
                (getCountryInfoFor currency).AsTask()
                |> Async.AwaitTask

            match countryInfo with
            | Ok response ->
                let currencies =
                    response
                    |> Seq.map (fun c -> c.currencies)
                    |> Seq.tryHead
                    |> Option.get

                return Some currencies
            | Error error ->
                printfn $"{error}"
                return None
        }

    let getExchangeFor (first: string) (second: string) =
        let urlString =
            $"{URlConstants.exchangesBaseUrl}%s{first}%s{second}&pairs=%s{second}%s{first}"

        vtask {
            let! response = fetch urlString

            return
                match response.StatusCode with
                | 200 ->
                    response.Body
                    |> Json.parseExchange
                    |> Result.mapError ParseError
                | 400 -> Error BadRequest
                | _ -> Error Non200Response
        }
