namespace GitFollowers

open System.Text.Json
open System.Text.Json.Serialization
open GitFollowers.DTOs

[<RequireQualifiedAccess>]
module Json =
    let createJsonOption : JsonSerializerOptions =
        let options = JsonSerializerOptions()
        options.Converters.Add(JsonFSharpConverter())
        options

    let deserialize<'T> (json: string) =
        try
            JsonSerializer.Deserialize<'T>(json, createJsonOption)
            |> Ok
        with
        | ex -> Error ex.Message

    let parseExchange (json: string) =
        try
            let document = JsonDocument.Parse(json)

            let properties =
                document.RootElement.EnumerateObject()
                |> Seq.toArray
                |> Seq.map (fun value -> value.Value)
                |> Seq.toArray

            let first =
                (properties |> Array.tryHead |> Option.get)
                    .GetDouble()

            let second =
                (properties |> Array.tryLast |> Option.get)
                    .GetDouble()

            let exchange: Exchange = { first = first; second = second }
            Ok exchange
        with ex -> Error ex.Message

    let serialize<'T> (json: 'T) =
        try
            JsonSerializer.Serialize<'T>(json)
            |> Ok
        with
        | ex -> Error ex.Message

