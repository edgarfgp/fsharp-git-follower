namespace GitFollowers

[<RequireQualifiedAccess>]
module Result =
    let get =
        function
        | Ok a -> a
        | Error e -> invalidArg "result" $"The result value was Error '%A{e}'"

