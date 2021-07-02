namespace GitFollowers

open System.Text.Json
open System.Text.Json.Serialization
open GitFollowers.DTOs

[<RequireQualifiedAccess>]
module Json =
    let createJsonOption: JsonSerializerOptions =
        let options = JsonSerializerOptions()
        options.Converters.Add(JsonFSharpConverter())
        options

    let deserialize<'T> (json: string) =
        try
            JsonSerializer.Deserialize<'T>(json, createJsonOption)
            |> Result.Ok
        with
        | ex -> Result.Error ex

    let parseExchange (json: string) : Result<Exchange, exn> =
        
        try
            let document = JsonDocument.Parse(json)
            let property =
                document.RootElement.EnumerateObject()
                |> Seq.toArray
                |> Seq.map(fun value -> value.Value)
                |> Seq.toList
            Result.Ok { first = property.[0].GetDouble() ; second = property.[1].GetDouble()}
        with
        | ex -> Result.Error ex

    let serialize<'T> (json: 'T) =
        try
            JsonSerializer.Serialize<'T>(json)
            |> Result.Ok
        with
        | ex -> Result.Error ex

