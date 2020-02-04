namespace GitFollowers.ViewControllers

open CoreGraphics
open System
open UIKit

type Follower = {
    login : string
    avatarUrl : string
}

type GitHubData = FSharp.Data.JsonProvider<"https://api.github.com/users/edgarfgp/followers?per_page=100&page=1">

type FollowerListViewController () =
    inherit UIViewController()

    let mutable collectionView : UICollectionView = null

    member self.CreateThreeColumnFlowLayout(view: UIView) =
        let width = view.Bounds.Width
        let padding = nfloat 12.
        let minimumItemSpacing = nfloat 10.
        let availableWidth = width - (padding * nfloat 2.) - (minimumItemSpacing * nfloat 2.)
        let itemwidth = availableWidth / nfloat 3.

        let flowLayout = new UICollectionViewFlowLayout()
        flowLayout.SectionInset <- UIEdgeInsets(padding, padding, padding, padding)
        flowLayout.ItemSize <- CGSize(itemwidth, (itemwidth + nfloat 40.))
        flowLayout

    member self.GetData() =
        let result =
            GitHubData.GetSamples()
                |> Array.toList
                |> List.map( fun c -> { login = c.Login ; avatarUrl = c.AvatarUrl })
        result

    override self.ViewDidLoad() =
        base.ViewDidLoad()
        self.View.BackgroundColor <- UIColor.SystemBackgroundColor
        collectionView <- new UICollectionView(self.View.Bounds, self.CreateThreeColumnFlowLayout(self.View))
        self.View.AddSubview(collectionView)

    override self.ViewWillAppear(_) =
        base.ViewWillAppear(true)
        base.NavigationController.SetNavigationBarHidden(hidden = false, animated = true)
        base.NavigationController.NavigationBar.PrefersLargeTitles <- true





