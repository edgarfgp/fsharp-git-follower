namespace GitFollowers

open System.Text.Json
open System.Text.Json.Serialization

[<AutoOpen>]
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

    let serialize<'T> (json: 'T) =
        try
            JsonSerializer.Serialize<'T>(json)
            |> Result.Ok
        with
        | ex -> Result.Error ex

