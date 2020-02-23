namespace GitFollowers.ViewControllers

open System
open CoreGraphics
open GitFollowers
open GitFollowers.Controllers
open GitFollowers.Models
open GitFollowers.Views.Cells
open UIKit

type FollowerListViewController(userName: string) as self =
    inherit UICollectionViewController(new UICollectionViewFlowLayout())

    let getFollowers username  =
        match NetworkService.getFollowers username with
        | Ok value -> value
        | Error _ -> []

    let followers = getFollowers userName

    let numberOfFollowers = nint followers.Length

    let rec filter f list =
        match list with
        | x::xs when f x -> x::(filter f xs)
        | _::xs -> filter f xs
        | [] -> []

    let createThreeColumnFlowLayout(view: UIView) =
        let width = view.Bounds.Width
        let padding  = nfloat 12.
        let minimumItemSpacing = nfloat 10.
        let availableWidth = width - (padding * nfloat 2.) - (minimumItemSpacing * nfloat 2.)
        let itemWidth = availableWidth / nfloat 3.
        let flowLayout = new  UICollectionViewFlowLayout()
        flowLayout.SectionInset <- UIEdgeInsets(padding, padding, padding, padding)
        flowLayout.ItemSize <- CGSize(itemWidth,  itemWidth + nfloat 40.)
        flowLayout

    let configureCollectionView() =
        self.CollectionView <- new UICollectionView(self.View.Bounds, createThreeColumnFlowLayout(self.View))
        self.CollectionView.BackgroundColor <- UIColor.SystemBackgroundColor
        self.CollectionView.RegisterClassForCell(typeof<FollowerCell>, FollowerCell.CellId)

    let configureSearchController() =
        self.NavigationItem.SearchController <-
            { new UISearchController() with
                member x.ObscuresBackgroundDuringPresentation = false }

        self.NavigationItem.SearchController.SearchResultsUpdater <-
            { new UISearchResultsUpdating() with
                member x.UpdateSearchResultsForSearchController(searchController) =
                    let text = searchController.SearchBar.Text
                    let result = filter (fun c -> c.login.ToLower().Contains(text.ToLower())) followers
                    printfn "%A" result.Length }

    override v.ViewDidLoad() =
        base.ViewDidLoad()
        configureCollectionView()
        configureSearchController()

    override v.GetItemsCount(_, _) =
        numberOfFollowers

    override v.GetCell(collectionView, indexPath) =
        let cell = collectionView.DequeueReusableCell(FollowerCell.CellId, indexPath) :?> FollowerCell
        let follower = followers.[int indexPath.Item]
        cell.Follower <- follower
        upcast cell

    override v.ItemSelected(_, indexPath) =
        let index = int indexPath.Item
        let follower = followers.[index]
        let userInfoController = new UserInfoController(follower.login)
        let navController = new UINavigationController(rootViewController = userInfoController)
        self.PresentViewController(navController, true, null)

    override v.ViewWillAppear(_) =
        base.ViewWillAppear(true)
        self.NavigationController.SetNavigationBarHidden(hidden = false, animated = true)
        self.NavigationController.NavigationBar.PrefersLargeTitles <- true
