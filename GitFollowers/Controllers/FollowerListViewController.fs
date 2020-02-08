namespace GitFollowers.ViewControllers

open System
open CoreGraphics
open GitFollowers.Controls
open GitFollowers.Controls.Cells
open Models
open UIKit

type GitHubData = FSharp.Data.JsonProvider<"https://api.github.com/users/edgarfgp/followers?per_page=100&page=1">
type FollowerListViewController () =
    inherit UICollectionViewController(new UICollectionViewFlowLayout())

    member self.GetFollowers() =
        GitHubData.GetSamples()
        |> Array.toList
        |> List.map( fun c -> { Login = c.Login ; AvatarUrl = c.AvatarUrl })
        |> List.toArray

    override self.ViewDidLoad() =
        base.ViewDidLoad()

        self.CollectionView.BackgroundColor <- UIColor.SystemBackgroundColor
        self.NavigationItem.SearchController <- { new UISearchController() with
                                                    member x.ObscuresBackgroundDuringPresentation = false


                                                    }
        self.CollectionView.Delegate <-
            { new UICollectionViewDelegateFlowLayout() with
                override x.GetSizeForItem(collectionView, layout, indexPath) =
                    CGSize(self.View.Frame.Width,  nfloat 300.) }

        self.CollectionView.RegisterClassForCell(typeof<UICollectionViewCell>, "FollowerCell")

    override self.GetItemsCount(_, _) =
        nint 5

    override self.GetCell(collectionView, indexPath) =
        let cell = collectionView.DequeueReusableCell("FollowerCell", indexPath) :?> UICollectionViewCell
        cell.BackgroundColor <- UIColor.SystemPinkColor
        cell

    override self.ViewWillAppear(_) =
        base.ViewWillAppear(true)
        base.NavigationController.SetNavigationBarHidden(hidden = false, animated = true)
        base.NavigationController.NavigationBar.PrefersLargeTitles <- true





