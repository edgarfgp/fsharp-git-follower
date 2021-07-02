namespace GitFollowers.Services

open System.Threading.Tasks
open FSharp.Control.Tasks
open GitFollowers
open GitFollowers.DTOs

type GitHubResult =
    | NetworkError
    | DeserializationError of exn

module GitHubService =
    let getFollowers(searchTerm: string, page: int) =
        let urlString =
            $"{URlConstants.githubBaseUrl}%s{searchTerm}/followers?per_page=100&page=%d{page}"
        vtask {
            let! response = fetchWitHeader urlString
            match response.StatusCode with
            | 200 ->
                return
                    response.Body
                     |> Json.deserialize<Follower array>
                     |> Result.mapError DeserializationError
            | _ ->
                return Error NetworkError
        }

    let getUserInfo(userName: string): ValueTask<Result<User, GitHubResult>> =
        let urlString =
            $"https://api.github.com/users/%s{userName}"

        vtask {
            let! response = fetchWitHeader urlString
            match response.StatusCode with
            | 200 ->
                return response.Body
                    |> Json.deserialize
                    |> Result.mapError DeserializationError
            | _ ->
                return Error NetworkError
        }