namespace GitFollowers

open System.Threading.Tasks
open FSharp.Control.Tasks
open System.Net.Http
open Foundation
open UIKit

type IGitHubService =
    abstract GetFollowers: string * int -> ValueTask<Result<Follower list, GitHubError>>
    abstract GetUserInfo: string -> Task<Result<User, GitHubError>>
    abstract DownloadDataFromUrl: string -> Task<Result<UIImage, string>>

type GitHubService() =

    let httpClientFactory = Http.createHttpClientFactory ()
    
    let mutable cache = new NSCache()

    let fetch urlString =
        let request =
            Http.createRequest urlString Get
            |> withHeader ("Accept", "application/json")
            |> withHeader ("User-Agent", "GitFollowers")
            |> withQueryParam ("print", "Url")

        request |> Http.execute httpClientFactory

    let fetchDataFromUrl (urlString: string) =
        task {
            try
                let httpClient = new HttpClient()
                let! response = httpClient.GetByteArrayAsync(urlString)
                let image = UIImage.LoadFromData(NSData.FromArray(response))
                return Ok image
            with :? HttpRequestException as ex -> return ex.Message |> Error
        }

    interface IGitHubService with
        member __.GetFollowers(searchTerm: string, page: int) =
            let urlString =
                sprintf "https://api.github.com/users/%s/followers?per_page=100&page=%d" searchTerm page

            vtask {
                let! response = fetch urlString
                match response.StatusCode with
                | 200 ->
                    return response.Body
                 |> JSON.decode
                 |> Result.mapError ParseError
                | _ ->
                    return Error NetworkError
            }

        member __.GetUserInfo(userName: string) =
            let urlString =
                sprintf "https://api.github.com/users/%s" userName

            task {
                let! response = fetch urlString
                match response.StatusCode with
                | 200 ->
                    return response.Body
                        |> JSON.decode
                        |> Result.mapError ParseError
                | _ ->
                    return Error NetworkError
            }

        member __.DownloadDataFromUrl(url: string) =
            task {
                let cacheKey = new NSString(url)
                let image = cache.ObjectForKey(cacheKey) :?> UIImage
                if image <> null then
                    return Ok image
                else
                    let! result = fetchDataFromUrl url |> Async.AwaitTask

                    let data =
                        match result with
                        | Ok data ->
                            cache.SetObjectforKey(data, cacheKey)
                            Ok data
                        | Error error -> Error error

                    return data
            }