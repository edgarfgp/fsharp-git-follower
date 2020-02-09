namespace GitFollowers.ViewControllers

open System
open CoreGraphics
open GitFollowers.Views.Cells
open GitFollowers.Models
open UIKit

type GitHubData = FSharp.Data.JsonProvider<"https://api.github.com/users/edgarfgp/followers?per_page=100&page=1">

type FollowerListViewController() =
    inherit UICollectionViewController(new UICollectionViewFlowLayout())

    member self.GetFollowers() =
        GitHubData.GetSamples()
        |> Array.toList
        |> List.map (fun c ->
            { Login = c.Login
              AvatarUrl = c.AvatarUrl })

    member self.CreateThreeColumnFlowLayout(view: UIView) : UICollectionViewFlowLayout =
        let width = view.Bounds.Width
        let padding  = nfloat 12.
        let minimumItemSpacing = nfloat 10.
        let availableWidth = width - (padding * nfloat 2.) - (minimumItemSpacing * nfloat 2.)
        let itemWidth = availableWidth / nfloat 3.
        let flowLayout = new  UICollectionViewFlowLayout()
        flowLayout.SectionInset <- new  UIEdgeInsets(padding, padding, padding, padding)
        flowLayout.ItemSize <- CGSize(itemWidth,  itemWidth + nfloat 40.)
        flowLayout

    override self.ViewDidLoad() =
        base.ViewDidLoad()

        self.CollectionView <- new UICollectionView(self.View.Bounds, self.CreateThreeColumnFlowLayout(self.View))
        self.CollectionView.BackgroundColor <- UIColor.SystemBackgroundColor
        self.NavigationItem.SearchController <-
            { new UISearchController() with
                member x.ObscuresBackgroundDuringPresentation = false }

        self.CollectionView.RegisterClassForCell(typeof<FollowerCell>, FollowerCell.CellId)

    override self.GetItemsCount(_, _) =
        //let numberOfFollowers = self.GetFollowers().Length
        nint 19 //numberOfFollowers


    override self.GetCell(collectionView, indexPath) =
        let cell = collectionView.DequeueReusableCell(FollowerCell.CellId, indexPath) :?> FollowerCell
        //let follower = self.GetFollowers().[int indexPath.Item]
        //cell.Follower <- follower
        upcast cell

    override self.ViewWillAppear(_) =
        base.ViewWillAppear(true)
        base.NavigationController.SetNavigationBarHidden(hidden = false, animated = true)
        base.NavigationController.NavigationBar.PrefersLargeTitles <- true
