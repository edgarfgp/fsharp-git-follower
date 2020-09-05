namespace GitFollowers

open System
open System.IO
open SQLite

type FollowerObject() =
    [<PrimaryKey; AutoIncrement>]
    member val Id = 0 with get, set
    member val Login = "" with get, set
    member val AvatarUrl = "" with get, set

module Repository =

    let getDbPath =
        let docFolder =
            Environment.GetFolderPath(Environment.SpecialFolder.Personal)

        let libFolder =
            Path.Combine(docFolder, "..", "Library", "Databases")

        if not (Directory.Exists libFolder)
        then Directory.CreateDirectory(libFolder) |> ignore
        else ()

        Path.Combine(libFolder, "GitFollowers.db3")

    let connect =
        async {
            let db =
                SQLiteAsyncConnection(SQLiteConnectionString getDbPath)

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

    let insertFollower (follower: Follower) =
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
