namespace GitFollowers.Models

open System

type User =
    { name: string option
      location: string option
      bio: string option
      public_repos: int
      public_gists: int
      html_url: string
      following: int
      followers: int
      created_at: DateTime }
    static member CreateUser(?name, ?location, ?bio, ?publicRepos, ?publicGists, ?htmlUrl, ?following, ?followers,
                             ?createdAt) =
        let initMember x = Option.fold (fun state param -> param) <| x
        { name = initMember (Some "") name
          location = initMember (Some "") location
          bio = initMember (Some "") bio
          public_repos = initMember 0 publicRepos
          public_gists = initMember 0 publicGists
          html_url = initMember "" htmlUrl
          following = initMember 0 following
          followers = initMember 0 followers
          created_at = initMember DateTime.Now createdAt }


type Follower =
    { login: string
      avatar_url: string }
    static member CreateFollower(?login, ?avatarUrl) =
        let initMember x = Option.fold (fun state param -> param) <| x
        { login = initMember "Edgar" login
          avatar_url = initMember "avatar-placeholder" avatarUrl }
