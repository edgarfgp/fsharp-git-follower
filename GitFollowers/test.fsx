

open GitFollowers.Services

#r "nuget: Ply"
#r "nuget: System.Text.Json"

open System.Text.Json
open System.Net.Http
open FSharp.Control.Tasks
open System.IO


let data = "{\"AUDUSD\":0.7227,\"USDAUD\":1.4266}"
let result = JsonDocument.Parse(data)

let property =
    result.RootElement.EnumerateObject()
    |> Seq.toArray
    |> Seq.map(fun value -> value.Value)
    |> Seq.toList

printfn $"{property.[0]}"


task {
    /// note the ***use*** instead of ***let***
    use client = new HttpClient()
    let! response = 
        client.GetStringAsync("https://restcountries.eu/rest/v2/alpha/col")
    do! File.WriteAllTextAsync("./response.json", response)
    // after the client goes out of scope
    // it will get disposed automatically thanks to the ***use*** keyword
}
|> Async.AwaitTask
// we run synchronously
// to allow the fsi to finish the pending tasks
|> Async.RunSynchronously



    
    