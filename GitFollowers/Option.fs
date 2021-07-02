namespace GitFollowers

[<RequireQualifiedAccess>]
module Option =
    let ofString (str: string) =
        Option.ofObj str

    let ofOption error =
        function
            Some s -> Ok s | None -> Error error

