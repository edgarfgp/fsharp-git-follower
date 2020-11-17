namespace GitFollowers

open System.Net.Http
open System.Text.Json
open FSharp.Control.Tasks
open FSharp.Data

type FollowersError =
    | NetworkError
    | Non200Response
    | ParseError of string

module private Network =
    let baseUrl = "https://api.github.com/users/"

    let fetch urlString =
        task {
            let! response =
                Http.AsyncRequestString(urlString, headers = [ "User-Agent", "GitFollowers" ])
                |> Async.Catch

            let result =
                match response with
                | Choice1Of2 data -> Ok data
                | Choice2Of2 error -> Error error

            return result
        }

    let fetchDataFromUrl (urlString: string) =
        task {
            try
                let httpClient = new HttpClient()
                let! response = httpClient.GetByteArrayAsync(urlString)
                return Ok response
            with :? HttpRequestException as ex -> return ex.Message |> Error
        }

    let decode<'T> (json: string) =
        try
            let deserialized =
                JsonSerializer.Deserialize<'T>(json, createJsonOption)

            Ok deserialized
        with ex -> ex.Message |> Error



type IGitHubService =
    abstract GetFollowers: string * int -> Async<Result<Follower list, FollowersError>>
    abstract GetUserInfo: string -> Async<Result<User, FollowersError>>
    abstract DownloadDataFromUrl: string -> Async<Result<byte [], string>>

type GitHubService() =

    interface IGitHubService with
        member __.GetFollowers(searchTerm: string, page: int) =
            let urlString =
                sprintf "https://api.github.com/users/%s/followers?per_page=100&page=%d" searchTerm page

            async {
                let! result = Network.fetch urlString |> Async.AwaitTask

                let data =
                    match result with
                    | Ok response ->
                        match Network.decode response with
                        | Error error -> Error(ParseError error)
                        | Ok data -> Ok data
                    | Error _ -> Error NetworkError

                return data
            }

        member __.GetUserInfo(userName: string) =
            let urlString =
                sprintf "https://api.github.com/users/%s" userName

            async {
                let! result = Network.fetch urlString |> Async.AwaitTask

                let data =
                    match result with
                    | Ok response ->
                        match Network.decode response with
                        | Error error -> Error(ParseError error)
                        | Ok data -> Ok data
                    | Error _ -> Error NetworkError

                return data
            }

        member __.DownloadDataFromUrl(url: string) =
            async {
                let! result = Network.fetchDataFromUrl url |> Async.AwaitTask

                let data =
                    match result with
                    | Ok data -> Ok data
                    | Error error -> Error error

                return data
            }
