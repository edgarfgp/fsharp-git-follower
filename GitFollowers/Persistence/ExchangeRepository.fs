namespace GitFollowers.Persistence

open System
open System.IO
open GitFollowers.Entities
open SQLite
open FSharp.Control.Tasks

module ExchangeRepository =

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

    let private connection =
        SQLiteAsyncConnection(SQLiteConnectionString getDbPath)

    let connectExchange =
        vtask {
            let result =
                connection.CreateTableAsync<Exchange>()
                |> Async.AwaitTask

            match! Async.Catch(result) with
            | Choice1Of2 result -> printfn $"GitFollowers.db3 was created with result: {result}"
            | Choice2Of2 error -> printfn $"Error while creating db: {error.Message}"
        }

    let connectCurrencies =
        vtask {
            let result =
                connection.CreateTableAsync<Currency>()
                |> Async.AwaitTask

            match! Async.Catch(result) with
            | Choice1Of2 result -> printfn $"GitFollowers.db3 was created with result: {result}"
            | Choice2Of2 error -> printfn $"Error while creating db: {error.Message}"
        }
 
    let insertCurrency(item: Currency) =
        vtask {
            let result = connection.InsertOrReplaceAsync(item) |> Async.AwaitTask
            match! Async.Catch(result) with
            | Choice1Of2 result ->
                printfn $"{item} was inserted with result: {result}"
            | Choice2Of2 error ->
                printfn $"Error while inserting {item}: {error.Message}"
        }
        
    let getAllCurrencies =
        vtask {
            let! result = connection.Table<Currency>().ToListAsync() |> Async.AwaitTask
            return result |> Seq.toArray
        }
