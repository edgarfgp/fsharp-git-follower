namespace GitFollowers.Controllers

open GitFollowers.DTOs
open GitFollowers.Persistence
open GitFollowers.Services
open FSharp.Control.Tasks

module FollowerListController =
    let addToFavorites username =
        vtask {
            let! userInfo =
                GitHubService.getUserInfo(username).AsTask()
                |> Async.AwaitTask
            return
                match userInfo with
                | Ok result ->
                    let follower : Follower =
                        { id = 0
                          login = result.login
                          avatar_url = result.avatar_url }
                    FavoritesUserDefaults.Instance.Save follower
                | Error error ->
                    printfn $"{error}"
                    NotAdded
        }

    let getFollowers(username: string, page: int) =
        vtask {
            let! result =
                GitHubService
                    .getFollowers(username, page)
                    .AsTask()
                |> Async.AwaitTask
            return
                match result with
                | Ok followers when followers |> Seq.isEmpty -> FollowersReceivedEvent.Empty
                | Ok followers  -> FollowersReceivedEvent.Ok followers
                | Error error ->
                    printfn $"{error}"
                    FollowersReceivedEvent.Error error
        }

