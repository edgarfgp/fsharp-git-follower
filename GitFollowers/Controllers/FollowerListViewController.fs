namespace GitFollowers

open System
open CoreFoundation
open FSharp.Control.Reactive
open Foundation
open GitFollowers
open UIKit

type FollowerListViewController(username) as self =
    inherit UICollectionViewController(new UICollectionViewFlowLayout())

    let mutable followers = []

    let loadingView = LoadingView.Instance

    let emptyView = FGEmptyView.Instance

    let githubService = GitHubService() :> IGitHubService

    let persistenceService =
        UserDefaultsService() :> IUserDefaultsService

    let sqliteService = SQLiteService() :> ISQLiteService

    let mutable page: int = 1

    let addToFavorites userName =
        async {
            let! userInfo =
                githubService.GetUserInfo userName
                |> Async.AwaitTask

            match userInfo with
            | Ok user ->
                let favorite =
                    { id = 0
                      login = user.login
                      avatar_url = user.avatar_url }

                let defaults = persistenceService.SaveFavorite favorite
                //let! _ = sqliteService.InsertFavorite favorite
                match defaults with
                | Added -> presentFGAlertOnMainThread "Favorites" "Favorite Added" self
                | FirstTimeAdded _ -> presentFGAlertOnMainThread "Favorites" "You have added your first favorite" self
                | AlreadyAdded -> presentFGAlertOnMainThread "Favorites" "This user is already in your favorites " self
            | Error _ ->
                presentFGAlertOnMainThread "Error" "We can not get the user info now. Please try again later." self
        }
        |> Async.Start

    let performDiDRequestFollowers username (collectionView: UICollectionView) =
        async {
            let! result =
                githubService.GetFollowers(username, page)
                |> Async.AwaitTask

            match result with
            | Ok result ->
                followers <- result

                DispatchQueue.MainQueue.DispatchAsync(fun _ ->
                    loadingView.Dismiss()
                    self.Title <- username

                    addRightNavigationItem self.NavigationItem UIBarButtonSystemItem.Add (fun _ ->
                        addToFavorites username)

                    collectionView.ReloadData())
            | Error _ ->
                loadingView.Dismiss()
                presentFGAlertOnMainThread "Error" "Error while processing request. Please try again later." self
        }
        |> Async.Start


    let performSearch (searchController: UISearchController) (self: UICollectionViewController) =
        searchController.SearchBar.TextChanged
        |> Observable.delay (TimeSpan.FromMilliseconds(350.))
        |> Observable.subscribe (fun filter ->
            let filteredResult =
                followers
                |> List.distinct
                |> List.filter (fun c ->
                    c
                        .login
                        .ToLower()
                        .Contains(filter.SearchText.ToLower()))

            followers <- filteredResult

            DispatchQueue.MainQueue.DispatchAsync(fun _ -> self.CollectionView.ReloadData()))

    override self.ViewDidLoad() =
        base.ViewDidLoad()

        addRightNavigationItem self.NavigationItem UIBarButtonSystemItem.Add (fun _ -> addToFavorites username)

        self.Title <- username

        self.NavigationController.SetNavigationBarHidden(hidden = false, animated = true)
        self.NavigationController.NavigationBar.PrefersLargeTitles <- true
        self.CollectionView <- new UICollectionView(self.View.Bounds, CreateThreeColumnFlowLayout(self.CollectionView))
        self.CollectionView.RegisterClassForCell(typeof<FollowerCell>, FollowerCell.CellId)
        self.CollectionView.BackgroundColor <- UIColor.SystemBackgroundColor

        loadingView.Show(self.View)

        async {
            let! followersResult =
                githubService.GetFollowers(username, page)
                |> Async.AwaitTask

            match followersResult with
            | Ok result ->
                followers <- result
                self.ConfigureCollectionView result
            | Error _ ->
                DispatchQueue.MainQueue.DispatchAsync(fun _ ->
                    loadingView.Dismiss()
                    self.ShowAlertAndGoBack())
        }
        |> Async.Start

    member self.FollowerCollectionViewDelegate =
        { new UICollectionViewDelegate() with

            member this.ItemSelected(collectionView, indexPath) =
                let index = int indexPath.Item
                let follower = followers.[index]
                loadingView.Show(self.CollectionView)

                async {
                    let! result =
                        githubService.GetUserInfo follower.login
                        |> Async.AwaitTask

                    match result with
                    | Ok value ->
                        DispatchQueue.MainQueue.DispatchAsync(fun _ ->
                            loadingView.Dismiss()
                            let userInfoController = new UserInfoController(value)

                            userInfoController.DidRequestFollowers.Add(fun (_, username) ->
                                loadingView.Show(self.View)
                                performDiDRequestFollowers username collectionView)

                            let navController =
                                new UINavigationController(rootViewController = userInfoController)

                            self.PresentViewController(navController, true, null))
                    | Error _ ->
                        DispatchQueue.MainQueue.DispatchAsync(fun _ ->
                            loadingView.Dismiss()

                            presentFGAlertOnMainThread
                                "Error"
                                "Error while processing request. Please try again later."
                                self)
                }
                |> Async.Start

            member this.DraggingEnded(scrollView: UIScrollView, _) =
                let offsetY = scrollView.ContentOffset.Y
                let contentHeight = scrollView.ContentSize.Height
                let height = scrollView.Frame.Size.Height

                if offsetY > contentHeight - height then
                    page <- page + 1
                    loadingView.Show(self.View)

                    async {
                        let! result =
                            githubService.GetFollowers(username, page)
                            |> Async.AwaitTask

                        match result with
                        | Ok result ->
                            DispatchQueue.MainQueue.DispatchAsync(fun _ ->
                                loadingView.Dismiss()

                                if result.IsEmpty |> not then
                                    followers <- result
                                    self.CollectionView.ReloadData()

                                    self.CollectionView.ScrollToItem
                                        (NSIndexPath.Create([| 0; 0 |]), UICollectionViewScrollPosition.Top, true)
                                else
                                    printfn "No more followers")
                        | Error _ -> DispatchQueue.MainQueue.DispatchAsync(fun _ -> loadingView.Dismiss())

                    }
                    |> Async.Start }

    member self.ShowAlertAndGoBack() =
        let alertVC =
            new FGAlertVC("Error", "Error while processing your request. Please try again later", "Ok")

        alertVC.ModalPresentationStyle <- UIModalPresentationStyle.OverFullScreen
        alertVC.ModalTransitionStyle <- UIModalTransitionStyle.CrossDissolve
        self.PresentViewController(alertVC, true, null)

        alertVC.ActionButtonClicked(fun _ ->
            alertVC.DismissViewController(true, null)

            self.NavigationController.PopToRootViewController(true)
            |> ignore)

    member self.ConfigureCollectionView result =
        DispatchQueue.MainQueue.DispatchAsync(fun _ ->
            if followers.Length > 0 then
                loadingView.Dismiss()

                self.CollectionView.DataSource <-
                    { new UICollectionViewDataSource() with
                        member this.GetCell(collectionView, indexPath) =
                            let cell =
                                collectionView.DequeueReusableCell(FollowerCell.CellId, indexPath) :?> FollowerCell

                            let follower = followers.[int indexPath.Item]
                            cell.SetUp(follower, GitHubService())
                            upcast cell

                        member this.GetItemsCount(_, _) = nint followers.Length }

                self.NavigationItem.SearchController <-
                    { new UISearchController() with
                        member this.ObscuresBackgroundDuringPresentation = false }

                self.NavigationItem.SearchController.SearchResultsUpdater <-
                    { new UISearchResultsUpdating() with
                        member this.UpdateSearchResultsForSearchController(searchController) =
                            match searchController.SearchBar.Text with
                            | text when String.IsNullOrWhiteSpace(text) |> not ->
                                performSearch searchController self |> ignore
                            | _ -> followers <- result }

                self.CollectionView.Delegate <- self.FollowerCollectionViewDelegate
            else
                loadingView.Dismiss()
                emptyView.Show self.View "No Followers")