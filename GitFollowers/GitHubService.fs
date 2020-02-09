namespace GitFollowers

open CoreFoundation

open System
open System.Net.Http
open FSharp.Data
open Foundation
open GitFollowers.Models
open UIKit

module GitHubService =

    type GitHubData = JsonProvider<"https://api.github.com/users/edgarfgp/followers?per_page=100&page=1">

    let getFollowers url =
        async {
            let httpClient = new HttpClient()
            try
                use! response = httpClient.GetAsync(Uri(url), HttpCompletionOption.ResponseHeadersRead) |> Async.AwaitTask
                response.EnsureSuccessStatusCode |> ignore

                let! followers = response.Content.ReadAsStringAsync() |> Async.AwaitTask

                let result =
                    GitHubData.Parse(followers)
                    |> Array.toList
                    |> List.map (fun c ->
                                { Login = c.Login
                                  AvatarUrl = c.AvatarUrl })

                return Ok result
            with
            | :? HttpRequestException as ex -> return ex.Message |> Error
        }|> Async.RunSynchronously

    let loadImage(urlString: string)=
        async {
            let httpClient = new HttpClient()
            try
                use! response = httpClient.GetAsync(Uri(urlString), HttpCompletionOption.ResponseHeadersRead) |> Async.AwaitTask
                response.EnsureSuccessStatusCode |> ignore

                let! followers = response.Content.ReadAsStringAsync() |> Async.AwaitTask
                return Ok (new UIImage(followers))
            with
            | :? HttpRequestException as ex -> return ex.Message |> Error
        }|> Async.RunSynchronously