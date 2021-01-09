namespace GitFollowers

open SQLite

type FollowerObject() =
    [<PrimaryKey; AutoIncrement>]
    member val Id = 0 with get, set
    member val Login = "" with get, set
    member val AvatarUrl = "" with get, set

module private Repository =
    open System
    open System.IO

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

