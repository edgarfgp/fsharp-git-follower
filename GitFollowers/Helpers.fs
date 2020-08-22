namespace GitFollowers.Helpers

open System
open System.Net.Http
open System.Threading
open CoreFoundation
open CoreGraphics
open Foundation
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
        if str |> System.String.IsNullOrWhiteSpace |> not
        then Some str
        else None

module UIImageView =

    let mainThread = SynchronizationContext.Current

    let downloadImageFromUrl (url: string, image: UIImageView) =
        async {
            let httpClient = new HttpClient()

            let! response =
                httpClient.GetByteArrayAsync(url)
                |> Async.AwaitTask

            let newImage =
                UIImage.LoadFromData(NSData.FromArray(response))

            DispatchQueue.MainQueue.DispatchAsync(fun _ -> image.Image <- newImage)
        }
        |> Async.Start


[<AutoOpen>]
module UICollectionView =

    let CreateThreeColumnFlowLayout (view: UIView) =
        let width = view.Bounds.Width
        let padding = nfloat 12.
        let minimumItemSpacing = nfloat 10.

        let availableWidth =
            width
            - (padding * nfloat 2.)
            - (minimumItemSpacing * nfloat 2.)

        let itemWidth = availableWidth / nfloat 3.
        let flowLayout = new UICollectionViewFlowLayout()
        flowLayout.SectionInset <- UIEdgeInsets(padding, padding, padding, padding)
        flowLayout.ItemSize <- CGSize(itemWidth, itemWidth + nfloat 40.)
        flowLayout
