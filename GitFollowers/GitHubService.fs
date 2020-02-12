namespace GitFollowers

open System
open System.Net.Http
open FSharp.Json
open GitFollowers.Models

module GitHubService =

    let getFollowers searchTerm =

        let urlString = sprintf "https://api.github.com/users/%s/followers?per_page=100&page=1" searchTerm
        async {
            let httpClient = new HttpClient()
            try
                use! response = httpClient.GetAsync(Uri(urlString), HttpCompletionOption.ResponseHeadersRead)
                                |> Async.AwaitTask
                response.EnsureSuccessStatusCode |> ignore

                let! followers = response.Content.ReadAsStringAsync()|> Async.AwaitTask

                let deserialized = Json.deserialize<Follower list> followers

                return Ok (deserialized |> List.map(fun c -> { login = c.login ; avatar_url = c.avatar_url }))
            with
            | :? HttpRequestException as ex -> return ex.Message |> Error
        }|> Async.RunSynchronously