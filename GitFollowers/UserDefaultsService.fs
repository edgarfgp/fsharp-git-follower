namespace GitFollowers

open GitFollowers
open Persistence

type IUserDefaultsService =
    abstract GetFavorites: unit -> Follower list option
    abstract SaveFavorite: Follower -> PersistenceAddActionType
    abstract RemoveFavorite : Follower -> PersistenceRemoveActionType

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
                let result=  JSON.decode<Follower list>(favoritesResult)
                match result with
                | Ok favorites -> Some favorites
                | _ -> None
            | None -> None
            
        member __.RemoveFavorite(follower) =
            match follower with
            | Removed reason -> reason
            | NotRemoved reason -> reason