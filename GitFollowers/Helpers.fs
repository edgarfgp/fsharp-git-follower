namespace GitFollowers

open System.Text.Json
open System.Text.Json.Serialization
open CoreFoundation
open System

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
        if str |> String.IsNullOrWhiteSpace |> not
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
        
    let encode<'T> (json: 'T) =
        try
            JsonSerializer.Serialize<'T>(json)
            |> Ok

        with ex -> ex.Message |> Error
 
module List =
    let rec removeItem predicate list =
        match list with
        | h::t when predicate h -> t
        | h::t -> h::removeItem predicate t
        | _ -> []

[<AutoOpen>]
module Dispatcher =      
    let invokeOnMainThread action =
        DispatchQueue.MainQueue.DispatchAsync(fun _ -> action)
