namespace GitFollowers

open GitFollowers
open Repository

type ISQLService =
    abstract InsertFavorite: Follower -> Async<Follower>

type SQLService() =
    interface ISQLService with

        member __.InsertFavorite(follower: Follower) =
            async {
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