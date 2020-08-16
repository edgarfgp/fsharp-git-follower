namespace GitFollowers.Helpers

module ImageNames =
    let location = "mappin.and.ellipse"
    let avatarPlaceHolder = "avatar-placeholder"

module Option =
    let OfString (str: string) =
        if str |> System.String.IsNullOrWhiteSpace |> not then
            Some str
        else None