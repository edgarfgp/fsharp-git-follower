namespace GitFollowers.ViewControllers

open System
open CoreGraphics
open GitFollowers
open GitFollowers.Controllers
open GitFollowers.Models
open GitFollowers.Views
open GitFollowers.Views.Cells
open UIKit
open Extensions

type FollowerListViewController(userName : string) as self =
    inherit UIViewController()

    override __.ViewDidLoad() =
        base.ViewDidLoad()

        self.View.BackgroundColor <- UIColor.SystemBackgroundColor
        let loadingView = showLoadingView(self.View)
        match (NetworkService.getFollowers userName) |> Async.RunSynchronously with
        | Ok followers  ->
            match followers.Length with
            | x when x > 0 ->
                loadingView.Dismiss()
                self.Title <- userName
                self.ConfigureCollectionView(followers)
            |  _ ->
                loadingView.Dismiss()
                showEmptyView("This user has no followers. Go follow him", self)
        | Error error ->
            loadingView.Dismiss()
            presentFGAlertOnMainThread ("Error", error, self)

    override __.ViewWillAppear(_) =
        base.ViewWillAppear(true)
        self.NavigationController.SetNavigationBarHidden(hidden = false, animated = true)
        self.NavigationController.NavigationBar.PrefersLargeTitles <- true
        self.NavigationItem.RightBarButtonItem <- new UIBarButtonItem(systemItem= UIBarButtonSystemItem.Add)
        self.NavigationItem.RightBarButtonItem.Clicked
        |> Event.add(fun _ -> self.AddFavoriteTapped())

    member __.ConfigureCollectionView(followers : Follower list) =

        let collectionView = new UICollectionView(self.View.Bounds, self.CreateThreeColumnFlowLayout(self.View))
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

        self.NavigationItem.SearchController <-
            { new UISearchController() with
                member __.ObscuresBackgroundDuringPresentation = false }

        self.NavigationItem.SearchController.SearchResultsUpdater <-
            { new UISearchResultsUpdating() with
                member __.UpdateSearchResultsForSearchController(searchController) =
                   // Implement search logic
                   ()}

        collectionView.RegisterClassForCell(typeof<FollowerCell>, FollowerCell.CellId)

    member __.CreateThreeColumnFlowLayout(view: UIView) =
            let width = view.Bounds.Width
            let padding  = nfloat 12.
            let minimumItemSpacing = nfloat 10.
            let availableWidth = width - (padding * nfloat 2.) - (minimumItemSpacing * nfloat 2.)
            let itemWidth = availableWidth / nfloat 3.
            let flowLayout = new  UICollectionViewFlowLayout()
            flowLayout.SectionInset <- UIEdgeInsets(padding, padding, padding, padding)
            flowLayout.ItemSize <- CGSize(itemWidth,  itemWidth + nfloat 40.)
            flowLayout

    member __.AddFavoriteTapped() =
         Xamarin.Essentials.Preferences.Set(userName, userName)