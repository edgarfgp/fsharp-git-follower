namespace GitFollowers

open GitFollowers
open Repository

type ISQLiteService =
    abstract InsertFavorite: Follower -> Async<Follower>

type SQLiteService() =
    interface ISQLiteService with

        member self.InsertFavorite(follower: Follower) =
            async {
                let! database = connect
                let obj = self.convertToObject follower

                do! database.InsertAsync(obj)
                    |> Async.AwaitTask
                    |> Async.Ignore

                let! rowIdObj =
                    database.ExecuteScalarAsync("select last_insert_rowid()", [||])
                    |> Async.AwaitTask

                let rowId = rowIdObj |> int

                return { follower with id = rowId }
            }

    member private  self.convertToObject (item: Follower) =
        let follower = FollowerObject()
        follower.Id <- item.id
        follower.Login <- item.login
        follower.AvatarUrl <- item.avatar_url
        follower