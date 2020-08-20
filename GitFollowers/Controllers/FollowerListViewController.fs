namespace GitFollowers

open System
open System.Threading
open CoreFoundation
open CoreGraphics
open GitFollowers
open GitFollowers.Controllers
open GitFollowers.Models
open GitFollowers.Views
open UIKit

[<AutoOpen>]
module FollowerListController =

    type FollowerListViewController(service: IGitHubService, userName: string) as self =
        inherit UIViewController()

        let loadingView = LoadingView.Instance
        let userDefaults = UserDefaults.Instance

        let mutable page: int = 1

        let collectionView =
            lazy (new UICollectionView(self.View.Bounds, self.CreateThreeColumnFlowLayout(self.View)))

        override __.ViewDidLoad() =
            base.ViewDidLoad()

            self.View.BackgroundColor <- UIColor.SystemBackgroundColor
            self.Title <- userName
            self.GetFollowers(userName, page)

            collectionView.Value.RegisterClassForCell(typeof<FollowerCell>, FollowerCell.CellId)

            self.NavigationController.SetNavigationBarHidden(hidden = false, animated = true)
            self.NavigationController.NavigationBar.PrefersLargeTitles <- true
            self.NavigationItem.RightBarButtonItem <- new UIBarButtonItem(systemItem = UIBarButtonSystemItem.Add)
            self.NavigationItem.RightBarButtonItem.Clicked
            |> Event.add (fun _ -> self.GetUserInfo(userName))

        member private __.ConfigureCollectionView(followers: Follower list) =
            collectionView.Value.TranslatesAutoresizingMaskIntoConstraints <- false
            self.View.AddSubview collectionView.Value
            collectionView.Value.BackgroundColor <- UIColor.SystemBackgroundColor

            collectionView.Value.Delegate <-
                { new UICollectionViewDelegate() with
                    member __.ItemSelected(_, indexPath) =
                        let index = int indexPath.Item
                        let follower = followers.[index]

                        let userInfoController =
                            new UserInfoController(GitHubService(), follower.login)

                        userInfoController.DidRequestFollowers.Add(fun (_, userName) ->
                            self.GetFollowers(userName, page)
                            self.Title <- userName
                            collectionView.Value.ReloadData())

                        let navController =
                            new UINavigationController(rootViewController = userInfoController)

                        self.PresentViewController(navController, true, null)

                    member __.DraggingEnded(scrollView, willDecelerate) =
                        let offsetY = scrollView.ContentOffset.Y
                        let contentHeight = scrollView.ContentSize.Height
                        let height = scrollView.Frame.Size.Height
                        if offsetY > contentHeight - height then
                            page <- page + 1
                            self.GetFollowers(userName, page) }

            collectionView.Value.DataSource <-
                { new UICollectionViewDataSource() with
                    member __.GetCell(collectionView, indexPath) =
                        let cell =
                            collectionView.DequeueReusableCell(FollowerCell.CellId, indexPath) :?> FollowerCell

                        let follower = followers.[int indexPath.Item]
                        cell.SetUp follower
                        upcast cell

                    member __.GetItemsCount(_, _) = nint followers.Length }

            self.NavigationItem.SearchController <-
                { new UISearchController() with
                    member __.ObscuresBackgroundDuringPresentation = false }

            self.NavigationItem.SearchController.SearchResultsUpdater <-
                { new UISearchResultsUpdating() with
                    member __.UpdateSearchResultsForSearchController(searchController) = () }

        member private __.CreateThreeColumnFlowLayout(view: UIView) =
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

        member __.GetFollowers(userName: string, page: int) =
            loadingView.Show(self.View)
            let mainThread = SynchronizationContext.Current
            async {
                do! Async.SwitchToThreadPool()
                let! result = service.GetFollowers(userName, page)

                match result with
                | Ok followers ->
                    if followers.Length > 0 then
                        do! Async.SwitchToContext mainThread
                        loadingView.Dismiss()
                        self.ConfigureCollectionView(followers)
                    else
                        do! Async.SwitchToContext mainThread
                        loadingView.Dismiss()
                        showEmptyView ("This user has no followers. Go follow him", self)
                | Error _ ->
                    do! Async.SwitchToContext mainThread
                    loadingView.Dismiss()
                    DispatchQueue.MainQueue.DispatchAsync(fun _ ->
                        let alertVC =
                            new FGAlertVC("Error", "Error while processing your request. Please try again later", "Ok")

                        alertVC.ModalPresentationStyle <- UIModalPresentationStyle.OverFullScreen
                        alertVC.ModalTransitionStyle <- UIModalTransitionStyle.CrossDissolve
                        self.PresentViewController(alertVC, true, null)
                        alertVC.ActionButtonClicked(fun _ ->
                            alertVC.DismissViewController(true, null)
                            self.NavigationController.PopToRootViewController(true)
                            |> ignore))
            }
            |> Async.Start

        member __.GetUserInfo(userName: string) =
            let mainThread = SynchronizationContext.Current
            async {
                do! Async.SwitchToThreadPool()
                let! result = service.GetUserInfo userName

                match result with
                | Ok value ->
                    let follower =
                        { login = value.login
                          avatar_url = value.avatar_url }

                    let defaults = userDefaults.Update follower
                    match defaults with
                    | Ok status ->
                        match status with
                        | AlreadyExists ->
                            do! Async.SwitchToContext mainThread
                            presentFGAlertOnMainThread ("Favorite", "This user is already in your favorites ", self)
                        | FavouriteAdded ->
                            do! Async.SwitchToContext mainThread
                            presentFGAlertOnMainThread ("Favorite", "Favorite Added", self)
                    | Error _ ->
                        do! Async.SwitchToContext mainThread
                        presentFGAlertOnMainThread ("Error", "Error while trying to save the favorite", self)
                | Error _ ->
                    do! Async.SwitchToContext mainThread
                    presentFGAlertOnMainThread
                        ("Error", "Error while processing request. Please try again later.", self)
            }
            |> Async.Start
