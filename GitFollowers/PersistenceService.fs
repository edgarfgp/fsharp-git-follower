namespace GitFollowers

open Foundation
open FSharp.Json
open GitFollowers.Models
open GitFollowers.Helpers

type UpdateResult =
    | AlreadyExists
    | FavouriteAdded

type UserDefaults private() =
    let favorites = "favorites"
    let defaults = NSUserDefaults.StandardUserDefaults
    static let instance = UserDefaults()
    static member Instance = instance

    member this.SaveFavorite(favs: Follower list) =
        favs
        |> Json.serialize
        |> fun c -> defaults.SetString(c, "favorites")

    member this.Update(follower : Follower) =
        let favorites = this.RetrieveFavorites()
        match favorites with
        | Ok favs ->
            let hasFollowers = favs |> List.tryFind(fun c -> c.login = follower.login)
            match hasFollowers  with
            | Some follower  -> Ok AlreadyExists
            | None ->
                favs
                |> List.append [follower]
                |> this.SaveFavorite
                Ok FavouriteAdded
        | Error _  ->
            [follower]
            |> this.SaveFavorite
            Ok FavouriteAdded

    member this.RetrieveFavorites() : Result<Follower list, string> =
        let favoritesResult = defaults.StringForKey(favorites) |> Option.OfString
        match favoritesResult with
        | Some favs ->
            Ok (Json.deserialize<Follower list> favs)
        | _ -> Error "Error trying to deserialize the Follower list"
