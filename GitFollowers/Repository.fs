namespace GitFollowers

open SQLite

type FollowerObject() =
    [<PrimaryKey; AutoIncrement>]
    member val Id = 0 with get, set

    member val Login = "" with get, set
    member val AvatarUrl = "" with get, set

module Repository =
    let connect dbPath =
        async {
            let db =
                SQLiteAsyncConnection(SQLiteConnectionString dbPath)

            do! db.CreateTableAsync<FollowerObject>()
                |> Async.AwaitTask
                |> Async.Ignore

            return db
        }

    let convertToObject (item: Follower) =
        let obj = FollowerObject()
        obj.Id <- item.id
        obj.Login <- item.login
        obj.AvatarUrl <- item.avatar_url
        obj


    let insertContact dbPath follower =
        async {
            let! database = connect dbPath
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
