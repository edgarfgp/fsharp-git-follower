namespace GitFollowers

open System
open System.Net.Http
open System.Text.Json
open FSharp.Control.Tasks
open System.Threading.Tasks

module private Network =
    let baseUrl = "https://api.github.com/users/"

    let fetch<'T> urlString = task {
        try
            let httpClient = new HttpClient()
            use! response = httpClient.GetAsync(Uri(urlString), HttpCompletionOption.ResponseHeadersRead)
            response.EnsureSuccessStatusCode |> ignore
            let! followers = response.Content.ReadAsStringAsync()
            let deserialized = JsonSerializer.Deserialize<'T>(followers, createJsonOption)
            return Ok deserialized
        with
        | :? HttpRequestException as ex -> return ex.Message |> Error
        | :? JsonException as ex -> return ex.Message |> Error
    }

    let fetchDataFromUrl(url: string) = task {
        try
            let httpClient = new HttpClient()
            let! response =
                httpClient.GetByteArrayAsync(url)
            return Ok response
        with
        | :? HttpRequestException as ex -> return ex.Message |> Error
    }

type IGitHubService =
    abstract GetFollowers: string * int -> Task<Result<Follower list, string>>
    abstract GetUserInfo: string -> Task<Result<User, string>>
    abstract DownloadDataFromUrl : string -> Task<Result<byte[], string>>

type GitHubService() =
   
    interface IGitHubService with
        member __.GetFollowers(searchTerm: string, page: int) =
            let urlString = sprintf "https://api.github.com/users/%s/followers?per_page=100&page=%d" searchTerm page
            Network.fetch urlString

        member __.GetUserInfo(userName: string) =
            let urlString = sprintf "https://api.github.com/users/%s" userName
            Network.fetch urlString
 
        member __.DownloadDataFromUrl (url: string) =
            Network.fetchDataFromUrl url