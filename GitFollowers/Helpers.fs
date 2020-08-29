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

    let downloadImageFromUrl (url: string, image: UIImageView) =
        async {
            try
                let httpClient = new HttpClient()

                let! response =
                    httpClient.GetByteArrayAsync(url)
                    |> Async.AwaitTask

                DispatchQueue.MainQueue.DispatchAsync(fun _ ->
                    image.Image <- UIImage.LoadFromData(NSData.FromArray(response)))

            with :? HttpRequestException as ex ->
                DispatchQueue.MainQueue.DispatchAsync(fun _ -> image.Image <- UIImage.FromBundle(ImageNames.ghLogo))
                printfn "%O" ex.Message
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
        
[<AutoOpen>]
module UIViewController =
    
    let addRightNavigationItem(navigationItem : UINavigationItem, systemItem: UIBarButtonSystemItem,  action) =
        navigationItem.RightBarButtonItem <- new UIBarButtonItem(systemItem = systemItem)
        navigationItem.RightBarButtonItem.Clicked
            |> Event.add (action)
