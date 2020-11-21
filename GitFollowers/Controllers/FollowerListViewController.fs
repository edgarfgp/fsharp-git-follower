namespace GitFollowers

open System
open System.Threading
open CoreFoundation
open Foundation
open GitFollowers
open UIKit

[<AutoOpen>]
module FollowerListViewController =
    let loadingView = lazy LoadingView.Instance

    let userDefaults = UserDefaults.Instance

    let mainThread = SynchronizationContext.Current
    let mutable page: int = 1

    let alertVC =
            lazy new FGAlertVC("Error", "Error while processing your request. Please try again later", "Ok")

type FollowerSearchController() as self =
        inherit UISearchController()
        do
            self.ObscuresBackgroundDuringPresentation <- false
            self.SearchBar.Placeholder <- "Enter a valid  user"
            self.SearchResultsUpdater <-
            { new UISearchResultsUpdating() with
                member __.UpdateSearchResultsForSearchController(searchController) =
                    printfn "%A" searchController.SearchBar.Text
                    () }
            
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
    {
        followers: Follower list
        username: string
        service : IGitHubService
        viewController : UICollectionViewController
    }
        
type FollowersCollectionViewDelegate(delegateData : DelegateData) =
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
                                        delegateData.viewController.CollectionView.DataSource <- new FollowerDataSource(followers)
                                        delegateData.viewController.CollectionView.ReloadData()
                                    )
                                | Error _ ->
                                    loadingView.Value.Dismiss()
                                    failwith ""
                        }|> Async.Start
                    )

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
    
    override __.DraggingEnded(scrollView : UIScrollView , _) =
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
                        delegateData.viewController.CollectionView.ReloadData()
                    )
                | Error _ ->
                    DispatchQueue.MainQueue.DispatchAsync(fun _ ->
                        loadingView.Value.Dismiss()
                        failwith ""
                    )
                    
            }|> Async.Start

type FollowerListViewController(service: IGitHubService, userName: string) as self =
    inherit UICollectionViewController(new UICollectionViewFlowLayout())

    let collectionView =
        lazy new UICollectionView(self.View.Bounds, CreateThreeColumnFlowLayout(self.CollectionView))
        
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

        self.NavigationItem.SearchController <- new FollowerSearchController()
        
        async {
                do! Async.SwitchToThreadPool()
                let! result =
                    service.GetFollowers(userName, page)
                    |> Async.AwaitTask
                match result with
                | Ok followers ->
                    DispatchQueue.MainQueue.DispatchAsync(fun _ ->
                        self.CollectionView.DataSource <- new FollowerDataSource(followers)
                        self.CollectionView.Delegate <- new FollowersCollectionViewDelegate(
                            { followers = followers
                              username = userName
                              service = service
                              viewController = self })
                    )
                | Error ex ->
                    printf "%A" ex
        }
        |> Async.Start

    member __.ShowAlertAndGoBack() =
        alertVC.Value.ModalPresentationStyle <- UIModalPresentationStyle.OverFullScreen
        alertVC.Value.ModalTransitionStyle <- UIModalTransitionStyle.CrossDissolve
        self.PresentViewController(alertVC.Value, true, null)

        alertVC.Value.ActionButtonClicked(fun _ ->
            alertVC.Value.DismissViewController(true, null)

            self.NavigationController.PopToRootViewController(true)
            |> ignore)

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