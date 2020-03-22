namespace GitFollowers

open Foundation

type PersistenceActionType =
    | Removing
    | Adding

type PersistenceService private () =
    let defaults = NSUserDefaults.StandardUserDefaults
    static let instance = PersistenceService()
    static member Instance = instance

    member this.RetrieveFavorites() =
        defaults.StringForKey("favorites")


    member this.Save(favorites: string) =
        defaults.SetString(favorites, "favorites")