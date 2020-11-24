namespace GitFollowers

open Foundation
open System.Text.Json

type FavoriteStatus =
    | Added
    | AlreadyAdded
    | FirstTimeAdded

[<AutoOpen>]
module UserDefaultsService =
    
    let defaults = NSUserDefaults.StandardUserDefaults
    
    let favoritesKey = "favorites"

    let storedFavorites =
            defaults.StringForKey(favoritesKey)
            |> Option.OfString
    
    let (|Saved|NotSaved|)(follower : Follower) =
        match storedFavorites with
        | Some favoritesString ->
            let favorites =
                JsonSerializer.Deserialize<Follower list>(favoritesString, JSON.createJsonOption)
            let favoriteLookup =
                favorites
                |> List.tryFind (fun f -> f.login = follower.login)
                |> Option.isSome
            if favoriteLookup then
                NotSaved AlreadyAdded
            else
                let favoritesToSave =
                    favorites
                    |> List.append[follower]
                let favoriteString = JsonSerializer.Serialize favoritesToSave
                defaults.SetString(favoriteString, favoritesKey)
                Saved Added
        | None ->
            let favoritesToSave = [] |> List.append[follower]
            let favoriteString = JsonSerializer.Serialize favoritesToSave
            defaults.SetString(favoriteString, favoritesKey)
            Saved FirstTimeAdded

type UserDefaultsService private () =
    static let instance = UserDefaultsService()
    static member Instance = instance

    member __.Save follower =
         match follower with
         | NotSaved reason -> reason
         | Saved status -> status

    member __.GetFavorites() =
        match storedFavorites with
        | Some favoritesResult ->
            let favorites=  (JsonSerializer.Deserialize<Follower list>(favoritesResult, JSON.createJsonOption))
            Some favorites
        | None -> None
