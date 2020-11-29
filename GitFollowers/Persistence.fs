namespace GitFollowers

open System.Text.Json
open Foundation

type PersistenceAddActionType =
    | Added
    | AlreadyAdded
    | FirstTimeAdded
   
type PersistenceRemoveActionType =
    | RemovedOk
    | RemovingError

module Persistence =
    let defaults = NSUserDefaults.StandardUserDefaults
    let favoritesKey = "favorites"
    
    let (|Saved|NotSaved|)(follower : Follower) =
        let storedFavorites =
            defaults.StringForKey(favoritesKey)
            |> Option.OfString
        match storedFavorites with
        | Some favoritesString ->
            let favorites = JsonSerializer.Deserialize<Follower list>(favoritesString, JSON.createJsonOption)
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
            
    let (|Removed|NotRemoved|) (follower: Follower) =
        let storedFavorites =
            defaults.StringForKey(favoritesKey)
            |> Option.OfString
        match storedFavorites with
        | Some favoritesString ->
            let favorites = JSON.decode(favoritesString)
            match favorites with
            | Ok favorites -> 
                let updatedFavorites = favorites |> List.removeItem(fun f -> f.login = follower.login)
                match updatedFavorites with
                | favorites when favorites.Length >= 0 ->
                    let encodedFavorites = JSON.encode updatedFavorites
                    match encodedFavorites with
                    | Ok result ->
                        defaults.SetString(result, favoritesKey)
                        Removed RemovedOk
                    | _ -> NotRemoved RemovingError
                | _ -> NotRemoved RemovingError
            | _ -> NotRemoved RemovingError
            
        | None -> NotRemoved RemovingError
        
        

