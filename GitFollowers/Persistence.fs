namespace GitFollowers

open System.Text.Json
open Foundation

type FavoriteResult =
    | Added
    | AlreadyAdded
    | FirstTimeAdded

module Persistence =
    let defaults = NSUserDefaults.StandardUserDefaults
    let favoritesKey = "favorites"
    
    let (|Saved|NotSaved|)(follower : Follower) =
        let storedFavorites =
            defaults.StringForKey(favoritesKey)
            |> Option.OfString
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
                let favoritesToSave = follower::favorites
                let favoriteString = JsonSerializer.Serialize favoritesToSave
                defaults.SetString(favoriteString, favoritesKey)
                Saved Added
        | None ->
            let favoritesToSave = [] |> List.append[follower]
            let favoriteString = JsonSerializer.Serialize favoritesToSave
            defaults.SetString(favoriteString, favoritesKey)
            Saved FirstTimeAdded
            
    let removeFavorite (follower: Follower) =
        let storedFavorites =
            defaults.StringForKey(favoritesKey)
            |> Option.OfString
        match storedFavorites with
        | Some favoritesString ->
            let favorites =
                JsonSerializer.Deserialize<Follower list>(favoritesString, JSON.createJsonOption)
            let updatedFavorites = favorites |> List.removeItem(fun f -> f.login = follower.login)
            let favoriteString = JsonSerializer.Serialize updatedFavorites
            defaults.SetString(favoriteString, favoritesKey)
        | None -> failwith ""
        
        

