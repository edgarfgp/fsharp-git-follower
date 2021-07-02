namespace GitFollowers.Persistence

open System
open System.IO
open GitFollowers.Entities
open SQLite
open FSharp.Control.Tasks

module GitHubRepository =
    let getDbPath =
            let docFolder =
                Environment.GetFolderPath(Environment.SpecialFolder.Personal)

            let libFolder =
                Path.Combine(docFolder, "..", "Library", "Databases")

            if not (Directory.Exists libFolder) then
                Directory.CreateDirectory(libFolder) |> ignore
            else
                ()

            Path.Combine(libFolder, "GitFollowers.db3")
            
    let private connection = SQLiteAsyncConnection(SQLiteConnectionString getDbPath)

    let connect =
        vtask {
            let result = connection.CreateTableAsync<Favorite>() |> Async.AwaitTask
            match! Async.Catch(result) with
            | Choice1Of2 result ->
                printfn $"GitFollowers.db3 was created with result: {result}"
            | Choice2Of2 error ->
                printfn $"Error while creating db: {error.Message}"
        }
    
    let insert(item: Favorite) =
        vtask {
            let result = connection.InsertAsync(item) |> Async.AwaitTask
            match! Async.Catch(result) with
            | Choice1Of2 result ->
                printfn $"{item} was inserted with result: {result}"
            | Choice2Of2 error ->
                printfn $"Error while inserting {item}: {error.Message}"
        }

//    let update(follower: FollowerObject) =
//        async {
//            let! database = connection
//            let! result = database.UpdateAsync(follower) |> Async.AwaitTask
//            return result
//        }
//
//    let delete(follower: FollowerObject) =
//        async {
//            let! database = connection
//            let! result = database.DeleteAsync(follower) |> Async.AwaitTask
//            return result
//        }
//
//    let readAll =
//        async {
//            let! database = connection
//            let! result = database.Table<FollowerObject>().ToListAsync() |> Async.AwaitTask
//            return result |> Seq.toList |> List.map FollowerObject.ConvertFrom
//        }