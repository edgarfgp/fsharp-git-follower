namespace GitFollowers.Controllers

open GitFollowers.DTOs
open GitFollowers.Services

[<RequireQualifiedAccess>]
type UserReceivedEvent =
    | Ok of User
    | NotFound

[<RequireQualifiedAccess>]
type FollowersReceivedEvent =
    | Ok of Follower array
    | Empty
    | Error of GitHubError

[<RequireQualifiedAccess>]
type ExchangesReceivedEvent =
    | Ok of Exchange
    | Error of ExchangesError
    
[<RequireQualifiedAccess>]
type ExchangesState =
    | Loaded
    | NotLoaded
    
[<RequireQualifiedAccess>]
type CountriesState =
    | Loaded
    | NotLoaded
    
[<RequireQualifiedAccess>]
type ExchangesUpdate =
    | Updated
    | NotUpdated

[<RequireQualifiedAccess>]
type InternetConnectionEvent =
    | Connected
    | NotConnected
