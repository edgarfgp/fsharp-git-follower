namespace GitFollowers

open System
open System.Net.Http
open System.Text.Json
open System.Text.Json.Serialization

type IGitHubService =
    abstract GetFollowers: string * int -> Async<Result<Follower list, string>>
    abstract GetUserInfo: string -> Async<Result<User, string>>
    abstract DownloadDataFromUrl : string -> Async<Result<byte[], string>>

type GitHubService() =
    let baseUrl = "https://api.github.com/users/" 

    interface IGitHubService with
        member __.GetFollowers(searchTerm: string, page: int) =
            let urlString =
                sprintf "%s%s/followers?per_page=100&page=%d" baseUrl searchTerm page
            async {
                try
                    let httpClient = new HttpClient()
                    use! response =
                        httpClient.GetAsync(Uri(urlString), HttpCompletionOption.ResponseHeadersRead)
                        |> Async.AwaitTask
                    response.EnsureSuccessStatusCode |> ignore
                    let! followers =
                        response.Content.ReadAsStringAsync()
                        |> Async.AwaitTask
                    let deserialized =
                        JsonSerializer.Deserialize<Follower list>(followers, createJsonOption)

                    return Ok deserialized
                with
                | :? HttpRequestException as ex -> return ex.Message |> Error
                | :? JsonException as ex -> return ex.Message |> Error
            }

        member __.GetUserInfo(userName: string) =
            let urlString = sprintf "%s%s" baseUrl userName
            async {
                let httpClient = new HttpClient()
                try
                    use! response =
                        httpClient.GetAsync(Uri(urlString), HttpCompletionOption.ResponseHeadersRead)
                        |> Async.AwaitTask
                    response.EnsureSuccessStatusCode |> ignore
                    let! user =
                        response.Content.ReadAsStringAsync()
                        |> Async.AwaitTask
                    let deserialized = JsonSerializer.Deserialize<User>(user, createJsonOption)
                    return Ok deserialized
                with
                | :? HttpRequestException as ex -> return ex.Message |> Error
                | :? JsonException as ex -> return ex.Message |> Error
            }
 
        member __.DownloadDataFromUrl (url: string) =
            async {
                try
                    let httpClient = new HttpClient()
                    let! response =
                        httpClient.GetByteArrayAsync(url)
                        |> Async.AwaitTask
                    return Ok response
                with
                | :? HttpRequestException as ex -> return ex.Message |> Error
            }