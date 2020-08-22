namespace GitFollowers.Helpers

open System
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
        let request = new NSUrlRequest(new NSUrl(url))

        let response =
            NSUrlSessionResponse(fun data _ _ ->
                if data <> null then
                    let newImage = UIImage.LoadFromData(data)
                    DispatchQueue.MainQueue.DispatchAsync(fun _ -> image.Image <- newImage))

        let dataTask =
            NSUrlSession.SharedSession.CreateDataTask(request, response)

        dataTask.Resume()
        
[<AutoOpen>]
module UICollectionView =
    
     let CreateThreeColumnFlowLayout(view: UIView) =
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
