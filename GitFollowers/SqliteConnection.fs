namespace GitFollowers

open SQLite

module SqliteConnection =
    open System
    open System.IO

    let private getDbPath =
        let docFolder =
            Environment.GetFolderPath(Environment.SpecialFolder.Personal)

        let libFolder =
            Path.Combine(docFolder, "..", "Library", "Databases")

        if not (Directory.Exists libFolder) then
            Directory.CreateDirectory(libFolder) |> ignore
        else
            ()

        Path.Combine(libFolder, "GitFollowers.db3")

    let connection =
        SQLiteAsyncConnection(SQLiteConnectionString getDbPath)

