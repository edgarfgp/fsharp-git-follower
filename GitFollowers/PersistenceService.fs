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
        defaults.ArrayForKey("favorites") |> ignore
        defaults.Synchronize()

    member this.Save(favorites: Models.Follower list) =
        defaults.SetValueForKey(NSArray.FromObjects(favorites), new NSString("favorites")) |> ignore
