namespace GitFollowers.Models

type Follower =
    { Login: string
      AvatarUrl: string }
    static member CreateFollower(?login, ?avatarUrl) =
        let initMember x = Option.fold (fun state param -> param) <| x
        { Login = initMember "Edgar" login
          AvatarUrl = initMember "avatar-placeholder" avatarUrl }
