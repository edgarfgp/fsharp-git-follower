namespace GitFollowers

open Foundation
open FSharp.Json
open GitFollowers.Models

type UpdateResult =
    | AlreadyExists of Follower
    | FavouriteAdded

type UserDefaults private() =
    let favorites = "favorites"
    let defaults = NSUserDefaults.StandardUserDefaults
    static let instance = UserDefaults()
    static member Instance = instance

    member this.Update(follower : Follower) =
        match this.RetrieveFavorites()  with
        | Some favourites ->
            let hasFollowers = favourites |> List.tryFind(fun c -> c.login = follower.login)
            match hasFollowers  with
            | Some follower  -> Ok (AlreadyExists follower)
            | None ->
                favourites
                |> List.append [follower]
                |> Json.serialize
                |> fun c -> defaults.SetString(c, "favorites")
                Ok FavouriteAdded
        | None  -> Error "Error getting favourites"

    member this.RetrieveFavorites() : Follower list option =
        match defaults.StringForKey(favorites) with
        | result when result <> null &&  result <> "" ->
            Some (Json.deserialize<Follower list> result)
        | _ -> None