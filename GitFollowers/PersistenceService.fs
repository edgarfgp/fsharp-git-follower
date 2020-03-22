namespace GitFollowers

open Foundation

type PersistenceActionType =
    | Removing
    | Adding

type PersistenceService private () =

    let favorites = "favorites"
    let defaults = NSUserDefaults.StandardUserDefaults
    static let instance = PersistenceService()
    static member Instance = instance


