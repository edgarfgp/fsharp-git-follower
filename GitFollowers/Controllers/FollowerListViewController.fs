namespace GitFollowers

open System
open System.Threading
open CoreFoundation
open FSharp.Control.Reactive
open Foundation
open GitFollowers
open UIKit

[<AutoOpen>]
module FollowerListViewController =

    let mainThread = SynchronizationContext.Current

    let mutable page: int = 1
    
    let addToFavorites(viewController: UIViewController, service: IGitHubService, userDefaults : IUserDefaultsService,  userName: string) =
        async {
            do! Async.SwitchToThreadPool()
            let! userInfo = service.GetUserInfo userName |> Async.AwaitTask
            match userInfo with
            | Ok user ->
                let defaults = userDefaults.SaveFavorite { id = 0 ; login = user.login ; avatar_url = user.avatar_url }
                match defaults with
                | Added ->
                    do! Async.SwitchToContext mainThread
                    presentFGAlertOnMainThread ("Favorites", "Favorite Added", viewController)
                | FirstTimeAdded _ ->
                    do! Async.SwitchToContext mainThread
                    presentFGAlertOnMainThread ("Favorites", "You have added your first favorite", viewController)
                | AlreadyAdded ->
                    do! Async.SwitchToContext mainThread
                    presentFGAlertOnMainThread ("Favorites", "This user is already in your favorites ", viewController)
            | Error _ ->
                do! Async.SwitchToContext mainThread
                presentFGAlertOnMainThread ("Error", "We can not get the user info now. Please try again later.", viewController)
        }
        |> Async.Start


type FollowerSearchController() as self =
    inherit UISearchController()

    let didSearchFollower = Event<_>()

    do
        self.ObscuresBackgroundDuringPresentation <- false
        self.SearchBar.Placeholder <- "Enter a valid  user"

        self.SearchBar.TextChanged
        |> Observable.delay (TimeSpan.FromMilliseconds(350.))
        |> Observable.subscribe (fun args ->
            let filter = args.SearchText
            if String.IsNullOrEmpty(filter) |> not then didSearchFollower.Trigger(self, filter))
        |> ignore

    [<CLIEvent>]
    member this.DidSearchFollower = didSearchFollower.Publish

type FollowerDataSource(followers: Follower list) =
    inherit UICollectionViewDataSource()

    override __.GetCell(collectionView: UICollectionView, indexPath: NSIndexPath) =
        let cell =
            collectionView.DequeueReusableCell(FollowerCell.CellId, indexPath) :?> FollowerCell

        let follower = followers.[int indexPath.Row]
        cell.SetUp(follower, GitHubService())
        upcast cell

    override __.GetItemsCount(_, _) = nint followers.Length

type FollowersCollectionViewDelegate(followers: Follower list, service: IGitHubService, userDefaults: IUserDefaultsService, viewController: UICollectionViewController, username: string) =
    inherit UICollectionViewDelegate()

    let loadingView = LoadingView.Instance
    
    override __.ItemSelected(collectionView, indexPath) =
        let index = int indexPath.Item
        let follower = followers.[index]
        loadingView.Show(viewController.View)

        async {
            do! Async.SwitchToThreadPool()

            let! result =
                service.GetUserInfo follower.login
                |> Async.AwaitTask

            match result with
            | Ok value ->
                DispatchQueue.MainQueue.DispatchAsync(fun _ ->
                    loadingView.Dismiss()
                    let userInfoController = new UserInfoController(value)

                    userInfoController.DidRequestFollowers.Add(fun (_, username) ->
                        loadingView.Show(viewController.View)
                        async {
                            do! Async.SwitchToThreadPool()

                            let! result =
                                service.GetFollowers(username, 0)
                                |> Async.AwaitTask

                            match result with
                            | Ok followers ->
                                DispatchQueue.MainQueue.DispatchAsync(fun _ ->
                                    loadingView.Dismiss()
                                    viewController.Title <- username
                                    collectionView.DataSource <-
                                        new FollowerDataSource(followers)
                                        
                                    addRightNavigationItem(viewController.NavigationItem, UIBarButtonSystemItem.Add,
                                        (fun _ -> addToFavorites(viewController, service, userDefaults, username)))

                                    collectionView.ReloadData())
                            | Error _ ->
                                loadingView.Dismiss()
                                presentFGAlertOnMainThread("Error", "Error while processing request. Please try again later.", viewController)
                        }
                        |> Async.Start)

                    let navController =
                        new UINavigationController(rootViewController = userInfoController)

                    viewController.PresentViewController(navController, true, null))
            | Error _ ->
                DispatchQueue.MainQueue.DispatchAsync(fun _ ->
                    loadingView.Dismiss()

                    presentFGAlertOnMainThread
                        ("Error", "Error while processing request. Please try again later.", viewController))
        }
        |> Async.Start

    override __.DraggingEnded(scrollView: UIScrollView, _) =
        let offsetY = scrollView.ContentOffset.Y
        let contentHeight = scrollView.ContentSize.Height
        let height = scrollView.Frame.Size.Height

        if offsetY > contentHeight - height then
            page <- page + 1
            loadingView.Show(viewController.View)

            async {
                do! Async.SwitchToThreadPool()

                let! result =
                    service.GetFollowers(username, page)
                    |> Async.AwaitTask

                match result with
                | Ok followers ->
                    DispatchQueue.MainQueue.DispatchAsync(fun _ ->
                        loadingView.Dismiss()
                        viewController.Title <- username
                        viewController.CollectionView.DataSource <- new FollowerDataSource(followers)
                        viewController.CollectionView.ReloadData())
                | Error _ ->
                    DispatchQueue.MainQueue.DispatchAsync(fun _ ->
                        loadingView.Dismiss()
                        failwith "")

            }
            |> Async.Start

type FollowerListViewController(service: IGitHubService, userDefaults: IUserDefaultsService, username: string) as self =
    inherit UICollectionViewController(new UICollectionViewFlowLayout())

    let loadingView = LoadingView.Instance

    override __.ViewDidLoad() =
        base.ViewDidLoad()

        addRightNavigationItem
            (self.NavigationItem,
             UIBarButtonSystemItem.Add,
             (fun _ -> addToFavorites (self, service, userDefaults, username)))

        self.Title <- username

        self.NavigationController.SetNavigationBarHidden(hidden = false, animated = true)
        self.NavigationController.NavigationBar.PrefersLargeTitles <- true
        self.CollectionView <- new UICollectionView(self.View.Bounds, CreateThreeColumnFlowLayout(self.CollectionView))
        self.CollectionView.RegisterClassForCell(typeof<FollowerCell>, FollowerCell.CellId)
        self.CollectionView.BackgroundColor <- UIColor.SystemBackgroundColor
        loadingView.Show(self.View)

        async {
            do! Async.SwitchToThreadPool()

            let! result =
                service.GetFollowers(username, page)
                |> Async.AwaitTask

            match result with
            | Ok followers ->
                if followers.Length > 0 then
                    DispatchQueue.MainQueue.DispatchAsync(fun _ ->
                        loadingView.Dismiss()
                        let searchController = new FollowerSearchController()
                        self.CollectionView.DataSource <- new FollowerDataSource(followers)
                        self.NavigationItem.SearchController <- searchController

                        searchController.DidSearchFollower.Add(fun (_, userName) ->
                            let filteredFollowers =
                                followers
                                |> List.distinct
                                |> List.filter (fun c -> c.login.ToLower().Contains(userName.ToLower()))

                            printfn "%A" filteredFollowers

                            DispatchQueue.MainQueue.DispatchAsync(fun _ ->
                                self.CollectionView.DataSource <- new FollowerDataSource(filteredFollowers)
                                self.CollectionView.ReloadData()))                            
                        self.CollectionView.Delegate <-
                            new FollowersCollectionViewDelegate(followers, GitHubService(), userDefaults, self, username)
                    )
                else
                    do! Async.SwitchToContext mainThread

                    DispatchQueue.MainQueue.DispatchAsync(fun _ ->
                        loadingView.Dismiss()
                        showEmptyView ("No Favorites", self))
            | Error _ ->
                do! Async.SwitchToContext mainThread

                DispatchQueue.MainQueue.DispatchAsync(fun _ ->
                    loadingView.Dismiss()
                    self.ShowAlertAndGoBack())
        }
        |> Async.Start

    member __.ShowAlertAndGoBack() =
        let alertVC =
            new FGAlertVC("Error", "Error while processing your request. Please try again later", "Ok")

        alertVC.ModalPresentationStyle <- UIModalPresentationStyle.OverFullScreen
        alertVC.ModalTransitionStyle <- UIModalTransitionStyle.CrossDissolve
        self.PresentViewController(alertVC, true, null)

        alertVC.ActionButtonClicked(fun _ ->
            alertVC.DismissViewController(true, null)

            self.NavigationController.PopToRootViewController(true)
            |> ignore)