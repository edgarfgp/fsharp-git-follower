namespace GitFollowers.Helpers

module Option =

    let OfString (str: string) =
        if str |> System.String.IsNullOrWhiteSpace |> not then
            Some str
        else None




