namespace GitFollowers

open Foundation
open GitFollowers

type PersistenceAddActionType =
    | Added
    | AlreadyAdded
    | FirstTimeAdded

type PersistenceRemoveActionType =
    | RemovedOk
    | RemovingError

module private Persistence =
    let defaults = NSUserDefaults.StandardUserDefaults
    let favoritesKey = "favorites"
    
    let rec removeItem predicate list =
        match list with
        | h::t when predicate h -> t
        | h::t -> h::removeItem predicate t
        | _ -> []

    let (|Saved|NotSaved|) (follower: Follower) =
        let storedFavorites =
            defaults.StringForKey(favoritesKey)
            |> OfString

        match storedFavorites with
        | Some favoritesString ->
            let decodedFavorites =
                deserialize<Follower list> favoritesString

            match decodedFavorites with
            | Ok favorites ->
                let favoriteLookup =
                    favorites
                    |> List.tryFind (fun f -> f.login = follower.login)
                    |> Option.isSome

                if favoriteLookup then
                    NotSaved AlreadyAdded
                else
                    let favoritesToSave = follower :: favorites
                    let encodedFavorites = serialize favoritesToSave

                    match encodedFavorites with
                    | Ok favoriteString ->
                        defaults.SetString(favoriteString, favoritesKey)
                        Saved Added
                    | _ -> failwith "Error while encoding favorites"
            | _ -> failwith "Error while decoding favorites"
        | None ->
            let favoritesToSave = [] |> List.append [ follower ]
            let encodedFavorites = serialize favoritesToSave

            match encodedFavorites with
            | Ok favoriteString ->
                defaults.SetString(favoriteString, favoritesKey)
                Saved FirstTimeAdded
            | _ -> failwith "Error while encoding favorites"

    let (|Removed|NotRemoved|) (follower: Follower) =
        let storedFavorites =
            defaults.StringForKey(favoritesKey)
            |> OfString

        match storedFavorites with
        | Some favoritesString ->
            let favorites = deserialize favoritesString

            match favorites with
            | Ok favorites ->
                let updatedFavorites =
                    favorites
                    |> removeItem (fun f -> f.login = follower.login)

                match updatedFavorites with
                | favorites when favorites.Length >= 0 ->
                    let encodedFavorites = serialize updatedFavorites

                    match encodedFavorites with
                    | Ok result ->
                        defaults.SetString(result, favoritesKey)
                        Removed RemovedOk
                    | _ -> NotRemoved RemovingError
                | _ -> NotRemoved RemovingError
            | _ -> NotRemoved RemovingError

        | None -> NotRemoved RemovingError
