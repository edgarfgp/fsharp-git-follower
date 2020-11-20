namespace GitFollowers

open System
open System.Threading
open CoreFoundation
open GitFollowers
open UIKit

[<AutoOpen>]
module FollowerListViewController =
    let loadingView = LoadingView.Instance
    let userDefaults = UserDefaults.Instance
    let mainThread = SynchronizationContext.Current
    let mutable page: int = 1

type FollowerListViewController(service: IGitHubService, userName: string) as self =
    inherit UICollectionViewController(new UICollectionViewFlowLayout())

    let performUserAndNavigation follower =
        let mainThread = SynchronizationContext.Current

        async {
            do! Async.SwitchToThreadPool()

            let! result =
                service.GetUserInfo follower.login
                |> Async.AwaitTask

            match result with
            | Ok value ->
                do! Async.SwitchToContext mainThread

                DispatchQueue.MainQueue.DispatchAsync(fun _ ->
                    loadingView.Dismiss()
                    let userInfoController = new UserInfoController(value)

                    userInfoController.DidRequestFollowers.Add(fun (_, userName) ->
                        self.GetFollowers(userName, page)
                        self.Title <- userName
                        self.CollectionView.ReloadData())

                    let navController =
                        new UINavigationController(rootViewController = userInfoController)

                    self.PresentViewController(navController, true, null))
            | Error _ ->
                do! Async.SwitchToContext mainThread

                DispatchQueue.MainQueue.DispatchAsync(fun _ ->
                    loadingView.Dismiss()

                    presentFGAlertOnMainThread
                        ("Error", "Error while processing request. Please try again later.", self))
        }
        |> Async.Start

    let rec ConfigureCollectionView (followers: Follower list) =
        self.CollectionView <- new UICollectionView(self.View.Bounds, CreateThreeColumnFlowLayout(self.CollectionView))
        self.CollectionView.RegisterClassForCell(typeof<FollowerCell>, FollowerCell.CellId)
        self.NavigationController.NavigationBar.PrefersLargeTitles <- true
        self.CollectionView.TranslatesAutoresizingMaskIntoConstraints <- false
        self.CollectionView.BackgroundColor <- UIColor.SystemBackgroundColor

        self.CollectionView.Delegate <-
            { new UICollectionViewDelegate() with
                member __.ItemSelected(_, indexPath) =
                    let index = int indexPath.Item
                    let follower = followers.[index]
                    loadingView.Show(self.View)
                    performUserAndNavigation follower

                member __.DraggingEnded(scrollView, willDecelerate) =
                    let offsetY = scrollView.ContentOffset.Y
                    let contentHeight = scrollView.ContentSize.Height
                    let height = scrollView.Frame.Size.Height

                    if offsetY > contentHeight - height then
                        page <- page + 1
                        loadingView.Show(self.View)

                        async {
                            do! Async.SwitchToThreadPool()

                            let! result =
                                service.GetFollowers(userName, page)
                                |> Async.AwaitTask

                            match result with
                            | Ok followers ->
                                if followers.Length > 0 then
                                    do! Async.SwitchToContext mainThread
                                    loadingView.Dismiss()
                                    ConfigureCollectionView(followers)

                                do! Async.SwitchToContext mainThread
                                loadingView.Dismiss()
                            | Error _ ->
                                do! Async.SwitchToContext mainThread
                                loadingView.Dismiss()
                                DispatchQueue.MainQueue.DispatchAsync(fun _ -> self.ShowAlertAndGoBack())
                        }
                        |> Async.Start }

        self.CollectionView.DataSource <-
            { new UICollectionViewDataSource() with
                member __.GetCell(collectionView, indexPath) =
                    let cell =
                        collectionView.DequeueReusableCell(FollowerCell.CellId, indexPath) :?> FollowerCell

                    let follower = followers.[int indexPath.Item]
                    cell.SetUp(follower, GitHubService())
                    upcast cell

                member __.GetItemsCount(_, _) = nint followers.Length }

        self.NavigationItem.SearchController <-
            { new UISearchController() with
                member __.ObscuresBackgroundDuringPresentation = false }

        self.NavigationItem.SearchController.SearchResultsUpdater <-
            { new UISearchResultsUpdating() with
                member __.UpdateSearchResultsForSearchController(searchController) = () }

    override __.ViewDidLoad() =
        base.ViewDidLoad()

        self.NavigationController.SetNavigationBarHidden(hidden = false, animated = true)

        addRightNavigationItem
            (self.NavigationItem, UIBarButtonSystemItem.Add, (fun _ -> self.AddToFavorites(userName)))

        self.Title <- userName
        self.GetFollowers(userName, page)

    member __.ShowAlertAndGoBack() =
        let alertVC =
            new FGAlertVC("Error", "Error while processing your request. Please try again later", "Ok")

        self.CollectionView.BackgroundColor <- UIColor.SystemBackgroundColor
        alertVC.ModalPresentationStyle <- UIModalPresentationStyle.OverFullScreen
        alertVC.ModalTransitionStyle <- UIModalTransitionStyle.CrossDissolve
        self.PresentViewController(alertVC, true, null)

        alertVC.ActionButtonClicked(fun _ ->
            alertVC.DismissViewController(true, null)

            self.NavigationController.PopToRootViewController(true)
            |> ignore)

    member __.GetFollowers(userName: string, page: int) =
        loadingView.Show(self.View)

        async {
            do! Async.SwitchToThreadPool()

            let! result =
                service.GetFollowers(userName, page)
                |> Async.AwaitTask

            match result with
            | Ok followers ->
                if followers.Length > 0 then
                    do! Async.SwitchToContext mainThread
                    loadingView.Dismiss()
                    ConfigureCollectionView(followers)
                else
                    do! Async.SwitchToContext mainThread
                    loadingView.Dismiss()
                    showEmptyView ("This user has no followers. Go follow him", self)
            | Error _ ->
                do! Async.SwitchToContext mainThread
                loadingView.Dismiss()
                DispatchQueue.MainQueue.DispatchAsync(fun _ -> self.ShowAlertAndGoBack())
        }
        |> Async.Start

    member __.AddToFavorites(userName: string) =
        async {
            do! Async.SwitchToThreadPool()
            let! result = service.GetUserInfo userName |> Async.AwaitTask

            match result with
            | Ok value ->
                let follower =
                    { id = 0
                      login = value.login
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
                presentFGAlertOnMainThread ("Error", "Error while processing request. Please try again later.", self)
        }
        |> Async.Start