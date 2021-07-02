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
    let currencies = "coloncurrencysign.circle"

    let addImage = "icon_add"

module URlConstants =

    [<Literal>]
    let githubBaseUrl = "https://api.github.com/users/"

    [<Literal>]
    let exchangesBaseUrl = "<Enter the URL here>"

    [<Literal>]
    let countriesBaseUrl =
        "https://restcountries.eu/rest/v2/currency/"

module Countries =
    let currencies =
        [| "aud"
           "bgn"
           "brl"
           "cad"
           "chf"
           "cny"
           "czk"
           "dkk"
           "eur"
           "gbp"
           "hkd"
           "hrk"
           "huf"
           "idr"
           "ils"
           "inr"
           "isk"
           "jpy"
           "krw"
           "mxn"
           "myr"
           "nok"
           "nzd"
           "php"
           "pln"
           "ron"
           "rub"
           "sek"
           "sgd"
           "thb"
           "usd"
           "zar" |]
