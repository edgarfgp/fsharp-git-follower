namespace GitFollowers


open System.Threading.Tasks
open FSharp.Control.Tasks
open System.Net.Http
open Microsoft.Extensions.DependencyInjection

type IGitHubService =
    abstract GetFollowers: string * int -> Task<Result<Follower list, string>>
    abstract GetUserInfo: string -> Task<Result<User, string>>
    abstract DownloadDataFromUrl: string -> Task<Result<byte [], string>>

type GitHubService() =

    let createHttpClientFactory () =
        let services = ServiceCollection()
        services.AddHttpClient() |> ignore

        let serviceProvider = services.BuildServiceProvider()

        serviceProvider.GetRequiredService<IHttpClientFactory>()

    let httpClientFactory = createHttpClientFactory ()

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
                return Ok response
            with :? HttpRequestException as ex -> return ex.Message |> Error
        }

    interface IGitHubService with
        member __.GetFollowers(searchTerm: string, page: int) =
            let urlString =
                sprintf "https://api.github.com/users/%s/followers?per_page=100&page=%d" searchTerm page

            task {
                let! response = fetch urlString
                let result = decode response.Body
                return result
            }

        member __.GetUserInfo(userName: string) =
            let urlString =
                sprintf "https://api.github.com/users/%s" userName

            task {
                let! response = fetch urlString
                let result = decode response.Body
                return result
            }

        member __.DownloadDataFromUrl(url: string) =
            task {
                let! result = fetchDataFromUrl url |> Async.AwaitTask

                let data =
                    match result with
                    | Ok data -> Ok data
                    | Error error -> Error error

                return data
            }
