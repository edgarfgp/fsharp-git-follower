namespace GitFollowers.Persistence

open System
open System.IO
open System.Threading.Tasks
open GitFollowers.DTOs
open GitFollowers.Entities
open SQLite
open FSharp.Control.Tasks

[<RequireQualifiedAccess>]
type ExchangesLoadedEvent =
    | Loaded of Selection array
    | NotLoaded of exn

type IRepository =
    abstract member InsertCurrency : item: Currency  -> ValueTask<unit>
    abstract member InsertExchange : exchange: Exchange -> ValueTask<unit>
    abstract member GetAllCurrencies :  ValueTask<CurrencyData[] option>
    abstract member GetAllExchanges : ValueTask<ExchangesLoadedEvent>

type ExchangeRepository() =

    let (|Success|Failure|) = 
        function
        | Choice1Of2 a -> Success a
        | Choice2Of2 e -> Failure e
        
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
        
    let connection = SQLiteAsyncConnection(SQLiteConnectionString getDbPath)

    interface IRepository with
        member _.InsertCurrency(item: Currency) =
            vtask {
                do! connection.CreateTableAsync<Currency>() |> Async.AwaitTask |> Async.Ignore
                let result = connection.InsertOrReplaceAsync(item) |> Async.AwaitTask
                match! Async.Catch(result) with
                | Success result ->
                    printfn $"{item} was inserted with result: {result}"
                | Failure error ->
                    printfn $"Error while inserting {item}: {error.Message}"
        }
        
        member _.InsertExchange(exchange: Exchange) =
            vtask {
                do! connection.CreateTableAsync<Exchange>() |> Async.AwaitTask |> Async.Ignore
                let result = connection.InsertOrReplaceAsync(exchange) |> Async.AwaitTask
                match! Async.Catch(result) with
                | Success _ -> ()
                | Failure error ->
                    printfn $"Error while inserting {exchange}: {error.Message}"
            }

        member _.GetAllCurrencies =
            vtask {
                do! connection.CreateTableAsync<Currency>() |> Async.AwaitTask |> Async.Ignore
                let result = connection.Table<Currency>().ToListAsync() |> Async.AwaitTask
                match! Async.Catch(result) with
                | Success result ->
                    printfn $"GetAllCurrencies with result: {result}"
                    return result |> Seq.map Currency.toDomain |> Seq.toArray |> Some
                | Failure error ->
                    printfn $"Error while getAllCurrencies db: {error.Message}"
                    return None
            }
        
        member _.GetAllExchanges =
            vtask {
                do! connection.CreateTableAsync<Exchange>() |> Async.AwaitTask |> Async.Ignore
                let result = connection.Table<Exchange>().ToListAsync() |> Async.AwaitTask
                match! Async.Catch(result) with
                | Success result ->
                    let exchanges = result |> Seq.map Exchange.toDomain |> Seq.toArray
                    return ExchangesLoadedEvent.Loaded exchanges
                | Failure error ->
                    return ExchangesLoadedEvent.NotLoaded error
            }