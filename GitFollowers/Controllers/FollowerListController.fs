namespace GitFollowers.ViewControllers

open System
open CoreGraphics
open GitFollowers
open GitFollowers.Views.Cells
open UIKit

type FollowerListViewController(userName: string) =
    inherit UICollectionViewController(new UICollectionViewFlowLayout())

    let followers  =
        match GitHubService.getFollowers userName with
        | Ok value -> value
        | Error _ -> []

    let numberOfFollowers = nint followers.Length

    let createThreeColumnFlowLayout(view: UIView) : UICollectionViewFlowLayout =
        let width = view.Bounds.Width
        let padding  = nfloat 12.
        let minimumItemSpacing = nfloat 10.
        let availableWidth = width - (padding * nfloat 2.) - (minimumItemSpacing * nfloat 2.)
        let itemWidth = availableWidth / nfloat 3.
        let flowLayout = new  UICollectionViewFlowLayout()
        flowLayout.SectionInset <- UIEdgeInsets(padding, padding, padding, padding)
        flowLayout.ItemSize <- CGSize(itemWidth,  itemWidth + nfloat 40.)
        flowLayout

    override self.ViewDidLoad() =
        base.ViewDidLoad()

        self.CollectionView <- new UICollectionView(self.View.Bounds, createThreeColumnFlowLayout(self.View))
        self.CollectionView.BackgroundColor <- UIColor.SystemBackgroundColor
        self.NavigationItem.SearchController <-
            { new UISearchController() with
                member x.ObscuresBackgroundDuringPresentation = false }

        self.CollectionView.RegisterClassForCell(typeof<FollowerCell>, FollowerCell.CellId)

    override self.GetItemsCount(_, _) =
        numberOfFollowers

    override self.GetCell(collectionView, indexPath) =
        let cell = collectionView.DequeueReusableCell(FollowerCell.CellId, indexPath) :?> FollowerCell
        let follower = followers.[int indexPath.Item]
        cell.Follower <- follower
        upcast cell

    override self.ViewWillAppear(_) =
        base.ViewWillAppear(true)
        base.NavigationController.SetNavigationBarHidden(hidden = false, animated = true)
        base.NavigationController.NavigationBar.PrefersLargeTitles <- true
