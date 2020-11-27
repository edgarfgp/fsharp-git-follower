namespace GitFollowers

module private Repository =
    open System
    open System.IO
    open SQLite

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


