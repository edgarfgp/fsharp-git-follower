namespace GitFollowers.Persistence

open Foundation
open GitFollowers
open GitFollowers.DTOs

type AddActionResult =
    | Added
    | AlreadyAdded
    | FirstTimeAdded
    | NotAdded

type RemoveActionResult =
    | Removed
    | NotRemoved

type FavoritesUserDefaults() =
    let defaults = NSUserDefaults.StandardUserDefaults
    static let instance = FavoritesUserDefaults()

    member _.Remove(favorite: Follower) =
        let storedFavorites =
            defaults.StringForKey(Persistent.favoritesKey)
            |> Option.ofNullableString

        match storedFavorites with
        | Some favorites ->
            let result =
                Json.deserialize<Follower array> favorites

            let updatedFavorites =
                result
                |> Result.get
                |> Seq.except (seq { favorite })
                |> Seq.distinctBy (fun f -> f.login)
                |> Seq.toArray

            let encodedFavorites = Json.serialize updatedFavorites
            defaults.SetString(favorites, Persistent.favoritesKey)
            Removed
        | None -> failwithf $"{favorite} is not stored."

    member _.Save(favorite: Follower) =
        let storedFavorites =
            defaults.StringForKey(Persistent.favoritesKey)
            |> Option.ofNullableString

        match storedFavorites with
        | Some favoritesString ->
            let decodedFavorites =
                Json.deserialize<Follower array> favoritesString

            let favoriteLookup =
                decodedFavorites
                |> Result.get
                |> Seq.tryFind (fun f -> f.login = favorite.login)
                

            if favoriteLookup.IsSome then
                AlreadyAdded
            else
                let encodedFavorites =
                    decodedFavorites
                    |> Result.get
                    |> Seq.append [| favorite |]
                    |> Json.serialize
                    |> Result.get

                defaults.SetString(encodedFavorites, Persistent.favoritesKey)
                Added
        | None ->
            let favoritesToSave = ResizeArray()
            favoritesToSave.Add favorite
            let encodedFavorites =
                Json.serialize favoritesToSave
                |> Result.get

            defaults.SetString(encodedFavorites, Persistent.favoritesKey)
            FirstTimeAdded

    member _.GetAll =
        let storedFavorites =
            defaults.StringForKey(Persistent.favoritesKey)
            |> Option.ofNullableString

        match storedFavorites with
        | Some favorites ->
            Json.deserialize<Follower array> favorites |> Result.get
        | None -> Array.empty

    static member Instance = instance
