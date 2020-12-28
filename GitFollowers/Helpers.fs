namespace GitFollowers

open System.Text.Json
open System.Text.Json.Serialization
open System
open CoreFoundation
open UIKit

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
        Option.ofObj str

module private JSON =

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
        


//type Anchor =
//    | Leading of float
//    | Top of float
//    | Trailing of float
//    | Bottom of float
//    | Height of float
//    | Width of float

type Anchor = Anchor of float option * float option* float option* float option

module UIView =
    let constraintToEdges (parent: UIView [])(view: UIView) (anchor: Anchor)=
        view.TranslatesAutoresizingMaskIntoConstraints <- false
        match anchor with
        | Anchor(leading, top, trailing, height) ->
                match leading.IsSome, top.IsSome, trailing.IsSome, height.IsSome with
                | (true, true, true, true) -> 
                    view.TopAnchor.ConstraintEqualTo(parent.[0].BottomAnchor, constant =  (nfloat top.Value)).Active <- true
                    view.LeadingAnchor.ConstraintEqualTo(parent.[0].LeadingAnchor, constant = nfloat leading.Value).Active <- true
                    view.TrailingAnchor.ConstraintEqualTo(parent.[0].TrailingAnchor, constant = nfloat -trailing.Value).Active  <- true
                    view.HeightAnchor.ConstraintEqualTo(constant = nfloat height.Value).Active <- true
                    ()
                | _ -> failwith ""