namespace GitFollowers

open System.Net.Http
open System.Text.Json
open System.Text.Json.Serialization
open Microsoft.Extensions.DependencyInjection

module ImageNames =
    let location = "mappin.and.ellipse"
    let avatarPlaceHolder = "avatar-placeholder"
    let ghLogo = "gh-logo.png"
    let folder = "folder"
    let textAlignLeft = "text.alignleft"
    let heart = "heart"
    let person2 = "person.2"
    let emptyStateLogo = "empty-state-logo"

module Option =
    let OfString (str: string) =
        if str |> System.String.IsNullOrWhiteSpace |> not
        then Some str
        else None

module JSON =

    let createJsonOption: JsonSerializerOptions =
        let options = JsonSerializerOptions()
        options.Converters.Add(JsonFSharpConverter())
        options

    let decode<'T> (json: string) =
        try
            JsonSerializer.Deserialize<'T>(json, createJsonOption)
            |> Ok

        with ex -> ex.Message |> Error
 
module List =
    let rec removeItem predicate list =
        match list with
        | h::t when predicate h -> t
        | h::t -> h::removeItem predicate t
        | _ -> []
