namespace GitFollowers

open Foundation
open FSharp.Json
open GitFollowers.Models
open GitFollowers.Helpers

type UserDefaults private () as self =
    let favorites = "favorites"
    let defaults = NSUserDefaults.StandardUserDefaults
    static let instance = UserDefaults()
    static member Instance = instance

    member __.SaveFavorite(followers: Follower list) =
        followers
        |> Json.serialize
        |> fun c -> defaults.SetString(c, favorites)

    member __.Update(follower: Follower) =
        let followers = self.RetrieveFavorites()
        match followers with
        | Ok newFollower ->
            let hasFollowers =
                newFollower
                |> List.tryFind (fun c -> c.login = follower.login)

            match hasFollowers with
            | Some _ -> Ok AlreadyExists
            | None ->
                newFollower
                |> List.append [ follower ]
                |> self.SaveFavorite
                Ok FavouriteAdded
        | Error _ ->
            [ follower ] |> self.SaveFavorite
            Ok FavouriteAdded

    member __.RetrieveFavorites(): Result<Follower list, string> =
        let favoritesResult =
            defaults.StringForKey(favorites)
            |> Option.OfString

        match favoritesResult with
        | Some followers -> Ok(Json.deserialize<Follower list> followers)
        | _ -> Error "Error trying to deserialize the Follower list"
