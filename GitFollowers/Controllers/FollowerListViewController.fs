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
    let loadingView = lazy (LoadingView.Instance)

    let userDefaults = UserDefaultsService.Instance

    let mainThread = SynchronizationContext.Current
    let mutable page: int = 1

type FollowerSearchController() as self =
    inherit UISearchController()

    let didSearchFollower = Event<_>()

    do
        self.ObscuresBackgroundDuringPresentation <- false
        self.SearchBar.Placeholder <- "Enter a valid  user"
        self.SearchBar.TextChanged
            |> Observable.delay(TimeSpan.FromMilliseconds(350.))
            |> Observable.subscribe(
                fun args ->
                    let filter = args.SearchText
                    if String.IsNullOrEmpty(filter) |> not then
                        didSearchFollower.Trigger(self, filter)
                )
            |> ignore

    [<CLIEvent>]
    member this.DidSearchFollower = didSearchFollower.Publish

type FollowerDataSource(followers: Follower list) =
    inherit UICollectionViewDataSource()

    override __.GetCell(collectionView: UICollectionView, indexPath: NSIndexPath) =
        let cell =
            collectionView.DequeueReusableCell(FollowerCell.CellId, indexPath) :?> FollowerCell

        let follower = followers.[int indexPath.Item]
        cell.SetUp(follower, GitHubService())
        upcast cell

    override __.GetItemsCount(_, _) = nint followers.Length

type DelegateData =
    { followers: Follower list
      username: string
      service: IGitHubService
      viewController: UICollectionViewController }

type FollowersCollectionViewDelegate(delegateData: DelegateData) =
    inherit UICollectionViewDelegate()

    override __.ItemSelected(_, indexPath) =
        let index = int indexPath.Item
        let follower = delegateData.followers.[index]
        loadingView.Value.Show(delegateData.viewController.View)

        async {
            do! Async.SwitchToThreadPool()

            let! result =
                delegateData.service.GetUserInfo follower.login
                |> Async.AwaitTask

            match result with
            | Ok value ->
                DispatchQueue.MainQueue.DispatchAsync(fun _ ->
                    loadingView.Value.Dismiss()
                    let userInfoController = new UserInfoController(value)

                    userInfoController.DidRequestFollowers.Add(fun (_, userName) ->
                        loadingView.Value.Show(delegateData.viewController.View)

                        async {
                            do! Async.SwitchToThreadPool()

                            let! result =
                                delegateData.service.GetFollowers(userName, 0)
                                |> Async.AwaitTask

                            match result with
                            | Ok followers ->
                                DispatchQueue.MainQueue.DispatchAsync(fun _ ->
                                    loadingView.Value.Dismiss()
                                    delegateData.viewController.Title <- userName

                                    delegateData.viewController.CollectionView.DataSource <-
                                        new FollowerDataSource(followers)

                                    delegateData.viewController.CollectionView.ReloadData())
                            | Error _ ->
                                loadingView.Value.Dismiss()
                                failwith ""
                        }
                        |> Async.Start)

                    let navController =
                        new UINavigationController(rootViewController = userInfoController)

                    delegateData.viewController.PresentViewController(navController, true, null))
            | Error _ ->
                DispatchQueue.MainQueue.DispatchAsync(fun _ ->
                    loadingView.Value.Dismiss()

                    presentFGAlertOnMainThread
                        ("Error", "Error while processing request. Please try again later.", delegateData.viewController))
        }
        |> Async.Start

    override __.DraggingEnded(scrollView: UIScrollView, _) =
        let offsetY = scrollView.ContentOffset.Y
        let contentHeight = scrollView.ContentSize.Height
        let height = scrollView.Frame.Size.Height

        if offsetY > contentHeight - height then
            page <- page + 1
            loadingView.Value.Show(delegateData.viewController.View)

            async {
                do! Async.SwitchToThreadPool()

                let! result =
                    delegateData.service.GetFollowers(delegateData.username, page)
                    |> Async.AwaitTask

                match result with
                | Ok followers ->
                    DispatchQueue.MainQueue.DispatchAsync(fun _ ->
                        loadingView.Value.Dismiss()
                        delegateData.viewController.Title <- delegateData.username
                        delegateData.viewController.CollectionView.DataSource <- new FollowerDataSource(followers)
                        delegateData.viewController.CollectionView.ReloadData())
                | Error _ ->
                    DispatchQueue.MainQueue.DispatchAsync(fun _ ->
                        loadingView.Value.Dismiss()
                        failwith "")

            }
            |> Async.Start

type FollowerListViewController(service: IGitHubService, userName: string) as self =
    inherit UICollectionViewController(new UICollectionViewFlowLayout())

    let collectionView =
        lazy (new UICollectionView(self.View.Bounds, CreateThreeColumnFlowLayout(self.CollectionView)))

    override __.ViewDidLoad() =
        base.ViewDidLoad()

        addRightNavigationItem
            (self.NavigationItem, UIBarButtonSystemItem.Add, (fun _ -> self.AddToFavorites(userName)))

        self.Title <- userName

        self.NavigationController.SetNavigationBarHidden(hidden = false, animated = true)
        self.NavigationController.NavigationBar.PrefersLargeTitles <- true
        self.CollectionView.TranslatesAutoresizingMaskIntoConstraints <- false

        self.CollectionView <- collectionView.Value
        self.CollectionView.BackgroundColor <- UIColor.SystemBackgroundColor
        self.CollectionView.RegisterClassForCell(typeof<FollowerCell>, FollowerCell.CellId)
        self.CollectionView.BackgroundColor <- UIColor.SystemBackgroundColor
        loadingView.Value.Show(self.View)

        async {
            do! Async.SwitchToThreadPool()

            let! result =
                service.GetFollowers(userName, page)
                |> Async.AwaitTask

            match result with
            | Ok followers ->
                if followers.Length > 0 then
                    DispatchQueue.MainQueue.DispatchAsync(fun _ ->
                        loadingView.Value.Dismiss()
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
                            new FollowersCollectionViewDelegate(
                                { followers = followers
                                  username = userName
                                  service = service
                                  viewController = self }))
                else
                    do! Async.SwitchToContext mainThread
                    DispatchQueue.MainQueue.DispatchAsync(fun _ ->
                        loadingView.Value.Dismiss()
                        showEmptyView("No Favorites", self)
                    )
            | Error _ ->
                do! Async.SwitchToContext mainThread
                DispatchQueue.MainQueue.DispatchAsync(fun _ ->
                    loadingView.Value.Dismiss()
                    self.ShowAlertAndGoBack()
                )
        }
        |> Async.Start

    member __.ShowAlertAndGoBack() =
        let alertVC = new FGAlertVC("Error", "Error while processing your request. Please try again later", "Ok")
        alertVC.ModalPresentationStyle <- UIModalPresentationStyle.OverFullScreen
        alertVC.ModalTransitionStyle <- UIModalTransitionStyle.CrossDissolve
        self.PresentViewController(alertVC, true, null)
        alertVC.ActionButtonClicked(fun _ ->
            alertVC.DismissViewController(true, null)
            self.NavigationController.PopToRootViewController(true)
            |> ignore)

    member __.AddToFavorites(userName: string) =
        async {
            do! Async.SwitchToThreadPool()
            let! userInfo = service.GetUserInfo userName |> Async.AwaitTask
            match userInfo with
            | Ok user ->
                let defaults = userDefaults.Save { id = 0 ; login = user.login ; avatar_url = user.avatar_url }
                match defaults with
                | Saved ->
                    do! Async.SwitchToContext mainThread
                    presentFGAlertOnMainThread ("Favorites", "Favorite Added", self)
                | NoFavorites _ ->
                    do! Async.SwitchToContext mainThread
                    presentFGAlertOnMainThread ("Favorites", "You have added your first favorite", self)
                | AlreadySaved ->
                    do! Async.SwitchToContext mainThread
                    presentFGAlertOnMainThread ("Favorites", "This user is already in your favorites ", self)
            | Error _ ->
                do! Async.SwitchToContext mainThread
                presentFGAlertOnMainThread ("Error", "We can not get the user info now. Please try again later.", self)
        }
        |> Async.Start