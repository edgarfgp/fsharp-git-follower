namespace GitFollowers

open Foundation
open System.Text.Json

type FavoriteStatus =
    | Saved
    | AlreadySaved
    | NoFavorites

type FavoritesResult =
    | Present of Follower list
    | NotPresent

type UserDefaultsService private () =
    let favoritesKey = "favorites"
    let defaults = NSUserDefaults.StandardUserDefaults
    static let instance = UserDefaultsService()
    static member Instance = instance

    member __.Save follower =
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
                AlreadySaved
            else
                let favoritesToSave =
                    favorites
                    |> List.append[follower]

                let favoriteString = JsonSerializer.Serialize favoritesToSave
                defaults.SetString(favoriteString, favoritesKey)
                Saved
        | None ->
            let favoritesToSave = [] |> List.append[follower]
            let favoriteString = JsonSerializer.Serialize favoritesToSave
            defaults.SetString(favoriteString, favoritesKey)
            NoFavorites

    member __.GetFavorites() =
        let favoritesString =
            defaults.StringForKey(favoritesKey)
            |> Option.OfString

        match favoritesString with
        | Some favoritesResult ->
            let favorites=  (JsonSerializer.Deserialize<Follower list>(favoritesResult, JSON.createJsonOption))
            Present favorites
        | _ -> NotPresent
