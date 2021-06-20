namespace GitFollowers

open System.Runtime.CompilerServices
open CoreFoundation
open UIKit

[<AutoOpen>]
module Extensions =
    type MainThreadBuilder() =
        member this.Zero() =
            printf "Zero"
            ()

        member this.Delay(f) = 
            DispatchQueue.MainQueue.DispatchAsync(fun _ ->  f())

    let mainThread =new  MainThreadBuilder()
    
    open FSharp.Control.Tasks
    type UIImageView with
        member ui.LoadImageFromUrl(url: string) =
            vtask {
                let! result =
                    GitHubService.downloadDataFromUrl(url).AsTask()
                    |> Async.AwaitTask
                match result with
                | Some image -> ui.Image <- image
                | None -> ui.Image <- UIImage.FromBundle(ImageNames.ghLogo)
            }
 