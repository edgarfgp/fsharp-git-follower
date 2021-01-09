namespace GitFollowers

open System.Threading.Tasks
open FSharp.Control.Tasks
open Foundation
open UIKit

module GitHubService =
    let private httpClientFactory = Http.createHttpClientFactory()
    let private cache = new NSCache()

    let private fetch urlString =
        let request =
            Http.createRequest urlString Get
            |> withHeader ("Accept", "application/json")
            |> withHeader ("User-Agent", "GitFollowers")
            |> withQueryParam ("print", "Url")

        request |> Http.execute httpClientFactory

    let getFollowers(searchTerm: string, page: int): ValueTask<Result<Follower list, GitHubError>> =
        let urlString =
            sprintf "https://api.github.com/users/%s/followers?per_page=100&page=%d" searchTerm page
        vtask {
            let! response = fetch urlString
            match response.StatusCode with
            | 200 ->
                return
                    response.Body
                     |> JSON.decode
                     |> Result.mapError ParseError
            | _ ->
                return Error NetworkError
        }

    let getUserInfo(userName: string): ValueTask<Result<User, GitHubError>> =
        let urlString =
            sprintf "https://api.github.com/users/%s" userName

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

    let downloadDataFromUrl(url: string) :ValueTask<Result<UIImage, string>> =
        vtask {
            let cacheKey = new NSString(url)
            let image = cache.ObjectForKey(cacheKey) :?> UIImage
            if image <> null then
                return Ok image
            else
                let! result = Http.fetchDataFromUrl(httpClientFactory, url).AsTask() |> Async.AwaitTask
                return
                    match result with
                    | Ok response ->
                        let data = NSData.FromArray(response)
                        let image = UIImage.LoadFromData(data)
                        cache.SetObjectforKey(image, cacheKey)
                        Ok image
                    | Error error -> Error error
        }