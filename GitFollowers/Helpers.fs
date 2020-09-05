namespace GitFollowers

module ImageNames =
    let location = "mappin.and.ellipse"
    let avatarPlaceHolder = "avatar-placeholder"
    let ghLogo = "gh-logo.png"
    let folder = "folder"
    let textAlignLeft = "text.alignleft"
    let heart = "heart"
    let person2 = "person.2"
    let emptyStateLogo = "empty-state-logo"

[<AutoOpen>]
module Option =
    let OfString (str: string) =
        if str |> System.String.IsNullOrWhiteSpace |> not
        then Some str
        else None
