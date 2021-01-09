namespace GitFollowers

open GitFollowers
open Repository
open FSharp.Control.Tasks

module SQLiteService =

    let private  convertToObject (item: Follower) =
        let follower = FollowerObject()
        follower.Id <- item.id
        follower.Login <- item.login
        follower.AvatarUrl <- item.avatar_url
        follower

    let insertFavorite(follower: Follower) =
            vtask {
                let! database = connect
                let obj = convertToObject follower

                do! database.InsertAsync(obj)
                    |> Async.AwaitTask
                    |> Async.Ignore

                let! rowIdObj =
                    database.ExecuteScalarAsync("select last_insert_rowid()", [||])
                    |> Async.AwaitTask

                let rowId = rowIdObj |> int

                return { follower with id = rowId }
            }