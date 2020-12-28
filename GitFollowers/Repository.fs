namespace GitFollowers

module private Repository =
    open System
    open System.IO
    open SQLite

    let private getDbPath =
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

