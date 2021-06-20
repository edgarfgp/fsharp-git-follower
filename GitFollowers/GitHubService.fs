namespace GitFollowers

open System.Net
open System.Threading.Tasks
open FSharp.Control.Tasks
open Foundation
open GitFollowers
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

    let getFollowers(searchTerm: string, page: int) =
        let urlString =
            $"https://api.github.com/users/%s{searchTerm}/followers?per_page=100&page=%d{page}"
        vtask {
            let! response = fetch urlString
            match response.StatusCode with
            | 200 ->
                return
                    response.Body
                     |> deserialize
                     |> Result.mapError DeserializationError
            | _ ->
                return Error NetworkError
        }

    let getUserInfo(userName: string): ValueTask<Result<User, GitHubResult>> =
        let urlString =
            $"https://api.github.com/users/%s{userName}"

        vtask {
            let! response = fetch urlString
            match response.StatusCode with
            | 200 ->
                return response.Body
                    |> deserialize
                    |> Result.mapError DeserializationError
            | _ ->
                return Error NetworkError
        }

    let downloadDataFromUrl(url: string) :ValueTask<UIImage option> =
        vtask {
            let cacheKey = new NSString(url)
            let image = cache.ObjectForKey(cacheKey) :?> UIImage
            if image <> null then
                return Some image
            else
                let! result = Http.fetchDataFromUrl(httpClientFactory, url).AsTask() |> Async.AwaitTask
                return
                    match result with
                    | Ok response ->
                        let data = NSData.FromArray(response)
                        let image = UIImage.LoadFromData(data)
                        cache.SetObjectforKey(image, cacheKey)
                        Some image
                    | Error _ -> None
        }