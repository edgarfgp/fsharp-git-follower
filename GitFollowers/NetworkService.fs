namespace GitFollowers

open System
open System.Net.Http
open FSharp.Json
open GitFollowers.Models

module NetworkService =
    let private baseUrl = "https://api.github.com/users/"
    let getFollowers searchTerm =

        let urlString = sprintf "%s%s/followers?per_page=100&page=1" baseUrl searchTerm
        async {
            let httpClient = new HttpClient()
            try
                use! response = httpClient.GetAsync(Uri(urlString), HttpCompletionOption.ResponseHeadersRead)
                                |> Async.AwaitTask
                response.EnsureSuccessStatusCode |> ignore

                let! followers = response.Content.ReadAsStringAsync() |> Async.AwaitTask

                let deserialized = Json.deserialize<Follower list> followers

                return Ok deserialized
            with
            | :? HttpRequestException as ex -> return ex.Message |> Error
            | :? JsonDeserializationError as ex -> return ex.Message |> Error
        }

    let getUserInfo userName =
        let urlString = sprintf "%s%s" baseUrl userName
        async {
            let httpClient = new HttpClient()
            try
                use! response = httpClient.GetAsync(Uri(urlString), HttpCompletionOption.ResponseHeadersRead)
                                |> Async.AwaitTask
                response.EnsureSuccessStatusCode |> ignore

                let! user = response.Content.ReadAsStringAsync() |> Async.AwaitTask
                let deserialized = Json.deserialize<User> user

                return Ok deserialized
            with
            | :? HttpRequestException as ex -> return ex.Message |> Error
            | :? JsonDeserializationError as ex -> return ex.Message |> Error

        }


module NetworkServiceVersion2 =
    open FSharp.Data
    let private baseUrl = "https://api.github.com/users/"
    let getFollowers searchTerm =
        let urlString = sprintf "%s%s/followers?per_page=100&page=1" baseUrl searchTerm
        async {
            let! response = Async.Catch(Http.AsyncRequestString(urlString))
            let result = match response with
                         | Choice1Of2 result ->
                            Ok (Json.deserialize<Follower list> result)
                         | Choice2Of2 error -> Error error.Message

            return result
        }

    let getUserInfo userName =
        let urlString = sprintf "%s%s" baseUrl userName
        async {
            let! response = Async.Catch(Http.AsyncRequestString(urlString))
            match response with
            | Choice1Of2 user ->
                return Ok (Json.deserialize<User> user)
            | Choice2Of2 error -> return Error error
        }