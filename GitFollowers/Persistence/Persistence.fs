namespace GitFollowers.Persistence

open Foundation
open GitFollowers
open GitFollowers.DTOs

type AddActionResult =
    | Added
    | AlreadyAdded
    | FirstTimeAdded

type RemoveActionResult =
    | RemovedOk
    | RemovingError

type FavoritesUserDefaults() =
    let favoritesKey = "favorites"
    let defaults = NSUserDefaults.StandardUserDefaults
    static let instance = FavoritesUserDefaults()
    
    let rec removeItem predicate list =
        match list with
        | h::t when predicate h -> t
        | h::t -> h::removeItem predicate t
        | _ -> []

    member _.Remove(favorite : Follower) =
        let storedFavorites =
            defaults.StringForKey(favoritesKey)
            |> Option.ofString

        match storedFavorites with
        | Some favoritesString ->
            let favorites = Json.deserialize<Follower array> favoritesString
            match favorites with
            | Ok favorites ->
                let updatedFavorites = favorites |> Array.except(seq { favorite })
                match updatedFavorites with
                | favorites when favorites.Length >= 0 ->
                    let encodedFavorites = Json.serialize updatedFavorites

                    match encodedFavorites with
                    | Ok result ->
                        defaults.SetString(result, favoritesKey)
                        RemovedOk
                    | _ -> RemovingError
                | _ -> RemovingError
            | _ -> RemovingError

        | None -> RemovingError
        
    member _.Save(favorite: Follower) =
        let storedFavorites =
            defaults.StringForKey(favoritesKey)
            |> Option.ofString

        match storedFavorites with
        | Some favoritesString ->
            let decodedFavorites =
                Json.deserialize<Follower array> favoritesString

            match decodedFavorites with
            | Ok favorites ->
                let favoriteLookup =
                    favorites
                    |> Array.tryFind (fun f -> f.login = favorite.login)
                    |> Option.isSome

                if favoriteLookup then
                    AlreadyAdded
                else
                    let favoritesToSave = [| favorite |]
                    let result = favorites |> Array.append favoritesToSave
                    let encodedFavorites = Json.serialize result

                    match encodedFavorites with
                    | Ok favoriteString ->
                        defaults.SetString(favoriteString, favoritesKey)
                        Added
                    | _ -> failwith "Error while encoding favorites"
            | _ -> failwith "Error while decoding favorites"
        | None ->
            let favoritesToSave = [] |> List.append [ favorite ]
            let encodedFavorites = Json.serialize favoritesToSave

            match encodedFavorites with
            | Ok favoriteString ->
                defaults.SetString(favoriteString, favoritesKey)
                FirstTimeAdded
            | _ -> failwith "Error while encoding favorites"
            
    member _.GetAll =
        let storedFavorites =
            defaults.StringForKey(favoritesKey) |> Option.ofString

        match storedFavorites with
        | Some favoritesResult ->
            let result =
                Json.deserialize<Follower array> favoritesResult
            match result with
            | Ok favorites -> Some favorites
            | _ -> None
        | None -> None

    static member Instance = instance
