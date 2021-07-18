namespace GitFollowers.Controllers

open System
open System.Threading.Tasks
open FSharp.Control.Tasks
open GitFollowers.DTOs
open GitFollowers.Services

module SearchController =

    let getUserInfo username =
        vtask {
            let! userResult = GitHubService.getUserInfo(username).AsTask() |> Async.AwaitTask
            return
                match userResult with
                | Ok user -> UserReceivedEvent.Ok user
                | Error _ -> UserReceivedEvent.NotFound
        }

    let sanitizeUsername (text: string) =
        text
        |> String.filter Char.IsLetterOrDigit
        |> String.filter (fun char -> (Char.IsWhiteSpace(char) |> not))
        |> String.IsNullOrWhiteSpace
        |> not
