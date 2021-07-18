namespace GitFollowers.Services

open FSharp.Control.Tasks
open GitFollowers
open GitFollowers.DTOs

type GitHubError =
    | Non200Response
    | ParseError of string

module GitHubService =

    let getFollowers(searchTerm: string, page: int) =
        let urlString =
            $"{URlConstants.githubBaseUrl}%s{searchTerm}/followers?per_page=100&page=%d{page}"
        vtask {
            let! response = fetchWitHeader urlString
            return
                match response.StatusCode with
                | 200 ->
                        response.Body
                         |> Json.deserialize<Follower array>
                         |> Result.mapError ParseError
 
                | _ ->
                    Error Non200Response
        }

    let getUserInfo(userName: string) =
        let urlString =
            $"https://api.github.com/users/%s{userName}"

        vtask {
            let! response = fetchWitHeader urlString
            return
                match response.StatusCode with
                | 200 ->
                    response.Body
                        |> Json.deserialize<User>
                        |> Result.mapError ParseError
                | _ ->
                    Error Non200Response
        }