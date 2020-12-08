namespace GitFollowers

open System
open System.Text.Json.Serialization
open Foundation
open SQLite

type FollowerData(id : int, login: string, avatarUrl: string) = inherit NSObject()
    with 
        member __.Id = id 
        member __.Login = login 
        member __.AvatarUrl = avatarUrl

[<JsonFSharpConverter>]
type User =
    { id: int
      login: string
      avatar_url: string
      name: string option
      location: string option
      bio: string option
      public_repos: int
      public_gists: int
      html_url: string
      following: int
      followers: int
      created_at: DateTime }

[<JsonFSharpConverter>]
type Follower =
    { id: int
      login: string
      avatar_url: string }
    member this.ConvertToFollowerData = new FollowerData(this.id, this.login,  this.avatar_url)

type FollowerObject() =
    [<PrimaryKey; AutoIncrement>]
    member val Id = 0 with get, set

    member val Login = "" with get, set
    member val AvatarUrl = "" with get, set
    
type Section() = inherit NSObject()
