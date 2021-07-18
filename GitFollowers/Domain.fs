namespace GitFollowers

open System
open System.Text.Json.Serialization
open Foundation
open SQLite

module DTOs =

    [<JsonFSharpConverter>]
    type User =
        { id: int
          login: string
          avatar_url: string
          name: string option
          location: string option
          bio: string option
          public_repos: int
          public_gists: int
          html_url: string
          following: int
          followers: int
          created_at: DateTime }

    [<JsonFSharpConverter>]
    type Exchange = { first: float; second: float }

    [<JsonFSharpConverter>]
    type Country = {
        flag: string
        currencies: Currency array
    }

    and Currency =
        { code: string option
          name: string option
          symbol: string option }

    [<JsonFSharpConverter>]
    type Follower =
        { id: int
          login: string
          avatar_url: string }

    type CurrencyData =
        { code: string
          name: string
          value: double
          symbol: string }

    type Selection =
        { first: CurrencyData
          second: CurrencyData }
module Entities =

    open DTOs

    type Follower() =
        inherit NSObject()
        member val Id = 0 with get, set
        member val Login = "" with get, set
        member val AvatarUrl = "" with get, set

        static member fromDomain(entity: Follower) : DTOs.Follower =
            { id = entity.Id
              login = entity.Login
              avatar_url = entity.AvatarUrl }

        static member toDomain(dto: DTOs.Follower) : Follower =
            new Follower(Id = dto.id, Login = dto.login, AvatarUrl = dto.avatar_url)

    type Favorite() =
        [<PrimaryKey; AutoIncrement>]
        member val Id = 0 with get, set

        member val Login = "" with get, set
        member val AvatarUrl = "" with get, set

        static member fromDomain(follower: DTOs.Follower) =
            Favorite(Id = follower.id, Login = follower.login, AvatarUrl = follower.avatar_url)

        static member toDomain(favorite: Favorite) =
            { id = favorite.Id
              login = favorite.Login
              avatar_url = favorite.AvatarUrl }

    type Exchange() =
        [<PrimaryKey>]
        member val firstCode = "" with get, set

        member val firstName = "" with get, set
        member val firstValue = 0. with get, set
        member val secondCode = "" with get, set
        member val secondName = "" with get, set
        member val secondValue = 0. with get, set

        static member fromDomain
            (
                exchange: DTOs.Exchange,
                firstCode: string,
                firstName: string,
                secondCode: string,
                secondName: string
            ) =
            Exchange(
                firstCode = firstCode,
                firstName = firstName,
                firstValue = exchange.first,
                secondCode = secondCode,
                secondName = secondName,
                secondValue = exchange.second
            )

        static member toDomain(exchange: Exchange) =
            { first =
                  { code = exchange.firstCode
                    name = exchange.firstName
                    value = exchange.firstValue
                    symbol = "" }
              second =
                  { code = exchange.secondCode
                    name = exchange.secondName
                    value = exchange.secondValue
                    symbol = "" } }

    type Currency() =
        [<PrimaryKey>]
        member val code = "" with get, set

        member val name = "" with get, set
        member val value = 0. with get, set
        member val symbol = "" with get, set

        static member fromDomain(currency: CurrencyData) =
            Currency(code = currency.code, name = currency.name, value = currency.value, symbol = currency.symbol)

        static member toDomain(currency: Currency) =
            { code = currency.code
              name = currency.name
              value = currency.value
              symbol = currency.symbol }
