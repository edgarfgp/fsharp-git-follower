namespace GitFollowers

open System
open CoreFoundation
open FSharp.Control.Reactive
open GitFollowers
open UIKit

type FollowerListViewController(service: IGitHubService, userDefaults: IUserDefaultsService, username) as self =
    inherit UICollectionViewController(new UICollectionViewFlowLayout())

    let mutable followers = []
    
    let loadingView = LoadingView.Instance

    override __.ViewDidLoad() =
        base.ViewDidLoad()

        addRightNavigationItem
            self.NavigationItem
            UIBarButtonSystemItem.Add
            (fun _ -> addToFavorites self service userDefaults username)

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
            | Ok result ->
                followers <- result
                if followers.Length > 0 then
                    DispatchQueue.MainQueue.DispatchAsync(fun _ ->
                        loadingView.Dismiss()
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
                                member __.UpdateSearchResultsForSearchController(searchController) =
                                    match searchController.SearchBar.Text with
                                    | text when String.IsNullOrWhiteSpace(text) |> not -> 
                                        searchController.SearchBar.TextChanged
                                        |> Observable.delay (TimeSpan.FromMilliseconds(350.))
                                        |> Observable.subscribe (fun filter ->
                                            let filteredFollowers =
                                                followers
                                                |> List.distinct
                                                |> List.filter (fun c -> c.login.ToLower().Contains(filter.SearchText.ToLower()))

                                            followers <- filteredFollowers
                                            DispatchQueue.MainQueue.DispatchAsync(fun _ -> self.CollectionView.ReloadData()))
                                        |> ignore
                                    | _ -> followers <- result }

                        self.CollectionView.Delegate <- __.FollowerCollectionViewDelegate
                    )
                else
                    do! Async.SwitchToContext mainThread

                    DispatchQueue.MainQueue.DispatchAsync(fun _ ->
                        loadingView.Dismiss()
                        showEmptyView "No Followers" self.View)
            | Error _ ->
                do! Async.SwitchToContext mainThread

                DispatchQueue.MainQueue.DispatchAsync(fun _ ->
                    loadingView.Dismiss()
                    self.ShowAlertAndGoBack())
        }
        |> Async.Start
        
    member __.FollowerCollectionViewDelegate =
        { new UICollectionViewDelegate() with
            
            override __.ItemSelected(collectionView, indexPath) =
                let index = int indexPath.Item
                let follower = followers.[index]
                loadingView.Show(self.CollectionView)

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
                                loadingView.Show(self.CollectionView)
                                async {
                                    do! Async.SwitchToThreadPool()

                                    let! result =
                                        service.GetFollowers(username, 0)
                                        |> Async.AwaitTask

                                    match result with
                                    | Ok result ->
                                        followers <- result
                                        DispatchQueue.MainQueue.DispatchAsync(fun _ ->
                                            loadingView.Dismiss()
                                            self.Title <- username
                                                
                                            addRightNavigationItem self.NavigationItem UIBarButtonSystemItem.Add
                                                (fun _ -> addToFavorites self service userDefaults username)

                                            collectionView.ReloadData())
                                    | Error _ ->
                                        loadingView.Dismiss()
                                        presentFGAlertOnMainThread "Error" "Error while processing request. Please try again later." self
                                }
                                |> Async.Start)

                            let navController =
                                new UINavigationController(rootViewController = userInfoController)

                            self.PresentViewController(navController, true, null))
                    | Error _ ->
                        DispatchQueue.MainQueue.DispatchAsync(fun _ ->
                            loadingView.Dismiss()

                            presentFGAlertOnMainThread
                                "Error" "Error while processing request. Please try again later." self)
                }
                |> Async.Start

            override __.DraggingEnded(scrollView: UIScrollView, _) =
                let offsetY = scrollView.ContentOffset.Y
                let contentHeight = scrollView.ContentSize.Height
                let height = scrollView.Frame.Size.Height

                if offsetY > contentHeight - height then
                    page <- page + 1
                    loadingView.Show(self.CollectionView)

                    async {
                        do! Async.SwitchToThreadPool()

                        let! result =
                            service.GetFollowers(username, page)
                            |> Async.AwaitTask

                        match result with
                        | Ok result ->
                            followers <- result
                            DispatchQueue.MainQueue.DispatchAsync(fun _ ->
                                loadingView.Dismiss()
                                self.Title <- username
                                self.CollectionView.ReloadData())
                        | Error _ ->
                            DispatchQueue.MainQueue.DispatchAsync(fun _ -> loadingView.Dismiss())

                    }
                    |> Async.Start
                }

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