namespace GitFollowers

[<RequireQualifiedAccess>]
module Option =
    let ofNullableString (str: string) =
        Option.ofObj str

    let fromNullableString(str: string option) =
        if str.IsSome then str.Value else null

    let ofOption error =
        function
            Some s -> Ok s | None -> Error error

