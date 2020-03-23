namespace GitFollowers


open Foundation
open FSharp.Json
open GitFollowers.Models

type PersistenceActionType =
    | Removing
    | Adding

type UpdateResult =
    | AlreadyExists
    | FavouriteAdded
    | UpdateError of string

module PersistenceService =
    let private favorites = "favorites"
    let private defaults = NSUserDefaults.StandardUserDefaults

    let Save(followers : Follower list) =
        try
            let result = Json.serialize followers
            defaults.SetString(result, "favorites")
            Ok true
        with
            | :? JsonDeserializationError as ex -> ex.Message |> Error

    let RetrieveFavorites() : Result<Follower list, string> =
        try
            match defaults.StringForKey(favorites) with
            | result when result <> null &&  result <> ""->
                let favorites = Json.deserialize<Follower list> result
                Ok favorites
            | _ -> Ok []
        with
            | :? JsonDeserializationError as ex -> ex.Message |> Error

    let Update(follower : Follower) : Result<UpdateResult, string> =
            match RetrieveFavorites() with
            | Ok favourites ->
                    let hasFollowers = favourites |> List.tryFind(fun c -> c.login = follower.login)
                    match hasFollowers  with
                    | Some _  -> Ok AlreadyExists
                    | None ->
                       let newFavorites = favourites |> List.append [follower]
                       match Save(newFavorites) with
                       | Ok _ ->
                            Ok FavouriteAdded
                       | Error error -> Ok (UpdateError error)
            | Error error ->
                Ok (UpdateError error)