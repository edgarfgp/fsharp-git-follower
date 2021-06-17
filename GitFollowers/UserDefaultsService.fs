namespace GitFollowers

open GitFollowers
open Persistence

module private UserDefaultsService =
    let saveFavorite follower =
             match follower with
             | NotSaved reason -> reason
             | Saved status -> status

    let getFavorites =
            let storedFavorites =
                defaults.StringForKey(favoritesKey)
                |> Option.OfString
            match storedFavorites with
            | Some favoritesResult ->
                let result=  JSON.decode<Follower list>(favoritesResult)
                match result with
                | Ok favorites -> Some favorites
                | _ -> None
            | None -> None

    let removeFavorite follower =
            match follower with
            | Removed reason -> reason
            | NotRemoved reason -> reason