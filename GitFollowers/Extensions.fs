namespace GitFollowers

open System
open System.Threading.Tasks
open CoreFoundation
open FFImageLoading
open FFImageLoading.Svg.Platform
open Foundation
open UIKit

[<AutoOpen>]
module Extensions =
    
    type MainThreadBuilder() =
        member this.Zero() =
            printf "Zero"
            ()

        member this.Delay(f) = 
            DispatchQueue.MainQueue.DispatchAsync(fun _ ->  f())

    let mainThread = MainThreadBuilder()
    
    type MaybeBuilder() =
        member this.Bind(x, f) =
            match x with
            | None -> None
            | Some a -> f a

        member this.Return(x) =
            Some x

    let maybe = MaybeBuilder()
    
    open FSharp.Control.Tasks
    
    let private httpClientFactory = Http.createHttpClientFactory()
    let private cache = new NSCache()

    let fetchWitHeader urlString =
        let request =
            Http.createRequest urlString Get
            |> withHeader ("Accept", "application/json")
            |> withHeader ("User-Agent", "GitFollowers")

        request |> Http.execute httpClientFactory
        
    let fetch urlString =
        let request =
            Http.createRequest urlString Get
            |> withHeader ("Accept", "application/json")

        request |> Http.execute httpClientFactory
    
    let downloadDataFromUrl(url: string) :ValueTask<UIImage option> =
        vtask {
            let cacheKey = new NSString(url)
            let image = cache.ObjectForKey(cacheKey) :?> UIImage
            if image <> null then
                return Some image
            else
                let! result = Http.fetchDataFromUrl(httpClientFactory, url).AsTask()
                              |> Async.AwaitTask
                return
                    match result with
                    | Ok response ->
                        let data = NSData.FromArray(response)
                        let image = UIImage.LoadFromData(data)
                        cache.SetObjectforKey(image, cacheKey)
                        Some image
                    | Error _ -> None
        }
    type UIImageView with
    
        member ui.RoundedCorners()=
            ui.Layer.CornerRadius <- nfloat 50.
            ui.ClipsToBounds <- true
            ui.Layer.BorderWidth <- nfloat 3.
            ui.Layer.BorderColor = UIColor.White.CGColor
            |> ignore
        member ui.LoadImageFromUrl(url: string) =
            vtask {
                let! result =
                    downloadDataFromUrl(url).AsTask()
                    |> Async.AwaitTask
                match result with
                | Some image -> ui.Image <- image
                | None -> ui.Image <- UIImage.FromBundle(ImageNames.ghLogo)
            }

        member ui.LoadImageFromSvg(url: string) =
            vtask {
                    let result =
                        ImageService.Instance.LoadString(url)
                          .WithCustomDataResolver(SvgDataResolver(64, 0, true))
                          .AsUIImageAsync()
                          |> Async.AwaitTask
                    match! Async.Catch(result) with
                    | Choice1Of2 image ->
                        ui.Image.ImageWithRenderingMode(UIImageRenderingMode.AlwaysOriginal) |>ignore
                        ui.Image <- image
                    | Choice2Of2 _ ->
                        ui.Image <- UIImage.FromBundle(ImageNames.currencies)
            }
            
    type UIView with
        member uv.AddSubviewsX([<ParamArray>]views:UIView array) =
            views
            |> Array.map(fun view ->
                    view.TranslatesAutoresizingMaskIntoConstraints <- false
                    uv.AddSubview view)
            |> ignore
            
        member uv.ConstraintToParent(parentView: UIView, ?leading: float, ?top: float,
                                     ?trailing: float, ?bottom: float) =
            let leading = defaultArg leading (float 0)
            let top = defaultArg top (float 0)
            let trailing = defaultArg trailing (float 0)
            let bottom = defaultArg bottom (float 0)

            NSLayoutConstraint.ActivateConstraints(
                [| uv.TopAnchor.ConstraintEqualTo(parentView.TopAnchor, nfloat top)
                   uv.LeadingAnchor.ConstraintEqualTo(parentView.LeadingAnchor, nfloat leading)
                   uv.TrailingAnchor.ConstraintEqualTo(parentView.TrailingAnchor, nfloat -trailing)
                   uv.BottomAnchor.ConstraintEqualTo(parentView.BottomAnchor, nfloat -bottom) |]
            )

        member uv.ConstraintToParent(parentView: UIView, all: nfloat) =
            NSLayoutConstraint.ActivateConstraints(
                [| uv.TopAnchor.ConstraintEqualTo(parentView.TopAnchor, all)
                   uv.LeadingAnchor.ConstraintEqualTo(parentView.LeadingAnchor, all)
                   uv.TrailingAnchor.ConstraintEqualTo(parentView.TrailingAnchor, -all)
                   uv.BottomAnchor.ConstraintEqualTo(parentView.BottomAnchor, -all) |]
            )
 