namespace GitFollowers

open System
open System.Text.Json.Serialization

 [<JsonFSharpConverter>]
type User =
    {
      id: int
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

type UpdateResult =
    | AlreadyExists
    | FavouriteAdded
