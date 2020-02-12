namespace GitFollowers.Models

type Follower =
    { login: string
      avatar_url: string }
    static member CreateFollower(?login, ?avatarUrl) =
        let initMember x = Option.fold (fun state param -> param) <| x
        { login = initMember "Edgar" login
          avatar_url = initMember "avatar-placeholder" avatarUrl }
