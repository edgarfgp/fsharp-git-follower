namespace GitFollowers

open GitFollowers
open Persistence
open System.Text.Json

type IUserDefaultsService =
    abstract GetFavorites: unit -> Follower list option
    abstract SaveFavorite: Follower -> FavoriteResult
    
    abstract RemoveFavorite : Follower -> unit
    
type UserDefaultsService () =
    interface IUserDefaultsService with
        member __.SaveFavorite follower =
             match follower with
             | NotSaved reason -> reason
             | Saved status -> status

        member __.GetFavorites() =
            let storedFavorites =
                defaults.StringForKey(favoritesKey)
                |> Option.OfString
            match storedFavorites with
            | Some favoritesResult ->
                let favorites=  (JsonSerializer.Deserialize<Follower list>(favoritesResult, JSON.createJsonOption))
                Some favorites
            | None -> None
            
        member __.RemoveFavorite(follower) =
            removeFavorite(follower)