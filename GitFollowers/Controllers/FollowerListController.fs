namespace GitFollowers.ViewControllers

open CoreFoundation
open System
open CoreGraphics
open GitFollowers
open GitFollowers.Controllers
open GitFollowers.Models
open GitFollowers.Views.Cells
open GitFollowers.Views.ViewControllers
open GitFollowers.Views.Views
open UIKit

type FollowerListViewController(followers : Follower list) as self =
    inherit UIViewController()

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

    let showEmptyView() =
        let emptyView = new FGEmptyView("This user has not followers.")
        emptyView.Frame <- self.View.Bounds
        emptyView.TranslatesAutoresizingMaskIntoConstraints <- false
        self.View.AddSubview emptyView

        NSLayoutConstraint.ActivateConstraints([|
            emptyView.TopAnchor.ConstraintEqualTo(self.View.TopAnchor)
            emptyView.LeadingAnchor.ConstraintEqualTo(self.View.LeadingAnchor)
            emptyView.TrailingAnchor.ConstraintEqualTo(self.View.TrailingAnchor)
            emptyView.BottomAnchor.ConstraintEqualTo(self.View.BottomAnchor)
                    |])

    let configureCollectionView(followers : Follower list) =
        let collectionView = new UICollectionView(self.View.Bounds, createThreeColumnFlowLayout(self.View))
        collectionView.TranslatesAutoresizingMaskIntoConstraints <- false
        self.View.AddSubview collectionView
        collectionView.BackgroundColor <- UIColor.SystemBackgroundColor
        collectionView.Delegate <- {
            new UICollectionViewDelegate() with
                member __.ItemSelected(_, indexPath) =
                    let index = int indexPath.Item
                    let follower = followers.[index]
                    let userInfoController = new UserInfoController(follower.login)
                    let navController = new UINavigationController(rootViewController = userInfoController)
                    self.PresentViewController(navController, true, null) }
        collectionView.DataSource <- {
            new UICollectionViewDataSource() with
                member __.GetCell(collectionView, indexPath) =
                    let cell = collectionView.DequeueReusableCell(FollowerCell.CellId, indexPath) :?> FollowerCell
                    let follower = followers.[int indexPath.Item]
                    cell.Follower <- follower
                    upcast cell
                member __.GetItemsCount(_, _) =
                    nint followers.Length }

        collectionView.RegisterClassForCell(typeof<FollowerCell>, FollowerCell.CellId)

    let configureSearchController(followers : Follower list) =
        self.NavigationItem.SearchController <-
            { new UISearchController() with
                member __.ObscuresBackgroundDuringPresentation = false }

        self.NavigationItem.SearchController.SearchResultsUpdater <-
            { new UISearchResultsUpdating() with
                member __.UpdateSearchResultsForSearchController(searchController) =
                    let text = searchController.SearchBar.Text
                    let result = filter (fun c -> c.login.ToLower().Contains(text.ToLower())) followers
                    printfn "%A" result.Length }

    override __.ViewDidLoad() =
        base.ViewDidLoad()
        self.View.BackgroundColor <- UIColor.SystemBackgroundColor
        match followers.Length with
        |  x when x > 0  ->
            configureCollectionView(followers)
            configureSearchController(followers)
        |  _ ->
            showEmptyView()


    override __.ViewWillAppear(_) =
        base.ViewWillAppear(true)
        self.NavigationController.SetNavigationBarHidden(hidden = false, animated = true)
        self.NavigationController.NavigationBar.PrefersLargeTitles <- true
