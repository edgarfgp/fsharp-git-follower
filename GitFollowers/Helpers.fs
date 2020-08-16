namespace GitFollowers.Helpers

module SFImages =
    let location = "mappin.and.ellipse"
module Option =
    let OfString (str: string) =
        if str |> System.String.IsNullOrWhiteSpace |> not then
            Some str
        else None