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

type PersistenceService private () =
    static let favorites = "favorites"
    let defaults = NSUserDefaults.StandardUserDefaults
    static let instance = PersistenceService()
    static member Instance = instance

    member this.Update(follower : Follower) : Result<UpdateResult, string> =
            match this.RetrieveFavorites() with
            | Ok favourites ->
                    let hasFollowers = favourites |> List.tryFind(fun c -> c.login = follower.login)
                    match hasFollowers  with
                    | Some _  -> Ok AlreadyExists
                    | None ->
                       let newFavorites = favourites |> List.append [follower]
                       match this.Save(newFavorites) with
                       | Ok _ ->
                            Ok FavouriteAdded
                       | Error error -> Ok (UpdateError error)
            | Error error ->
                Ok (UpdateError error)

    member this.RetrieveFavorites() : Result<Follower list, string> =
        try
            match defaults.StringForKey(favorites) with
            | result when result <> null &&  result <> ""->
                let favorites = Json.deserialize<Follower list> result
                Ok favorites
            | _ -> Ok []
        with
            | :? JsonDeserializationError as ex -> ex.Message |> Error

    member this.Save(followers : Follower list) =
        try
            let result = Json.serialize followers
            defaults.SetString(result, "favorites")
            Ok true
        with
            | :? JsonDeserializationError as ex -> ex.Message |> Error