namespace GitFollowers

open System
open CoreFoundation
open FSharp.Control.Reactive
open GitFollowers
open UIKit

type FollowerListViewController(service: IGitHubService, userDefaults: IUserDefaultsService, username) =
    inherit UICollectionViewController(new UICollectionViewFlowLayout())

    let mutable followers = []

    let loadingView = LoadingView.Instance
    
    let emptyView = FGEmptyView.Instance
    
    let mutable disposable : IDisposable = null
    
    let performSearch (searchController: UISearchController) (self: UICollectionViewController) =
        searchController.SearchBar.TextChanged
        |> Observable.delay (TimeSpan.FromMilliseconds(350.))
        |> Observable.subscribe (fun filter ->
            let filteredFollowers =
                followers
                |> List.distinct
                |> List.filter (fun c ->
                    c.login.ToLower().Contains(filter.SearchText.ToLower()))
            followers <- filteredFollowers
            DispatchQueue.MainQueue.DispatchAsync(fun _ -> self.CollectionView.ReloadData()))

    override self.ViewDidLoad() =
        base.ViewDidLoad()

        addRightNavigationItem self.NavigationItem UIBarButtonSystemItem.Add (fun _ ->
            addToFavorites self service userDefaults username)

        self.Title <- username

        self.NavigationController.SetNavigationBarHidden(hidden = false, animated = true)
        self.NavigationController.NavigationBar.PrefersLargeTitles <- true
        self.CollectionView <- new UICollectionView(self.View.Bounds, CreateThreeColumnFlowLayout(self.CollectionView))
        self.CollectionView.RegisterClassForCell(typeof<FollowerCell>, FollowerCell.CellId)
        self.CollectionView.BackgroundColor <- UIColor.SystemBackgroundColor

        loadingView.Show(self.View)
        
        async {
            let! result =
                service.GetFollowers(username, page)
                |> Async.AwaitTask

            match result with
            | Ok result ->
                followers <- result
                self.ConfigureCollectionView result
            | Error _ ->
                DispatchQueue.MainQueue.DispatchAsync(fun _ ->
                    loadingView.Dismiss()
                    self.ShowAlertAndGoBack())
        }
        |> Async.Start
        
    override self.ViewWillDisappear(animated)=
        base.ViewWillDisappear(animated)
        disposable.Dispose()

    member self.FollowerCollectionViewDelegate =
        { new UICollectionViewDelegate() with

            member this.ItemSelected(collectionView, indexPath) =
                let index = int indexPath.Item
                let follower = followers.[index]
                loadingView.Show(self.CollectionView)

                async {
                    let! result =
                        service.GetUserInfo follower.login
                        |> Async.AwaitTask

                    match result with
                    | Ok value ->
                        DispatchQueue.MainQueue.DispatchAsync(fun _ ->
                            loadingView.Dismiss()
                            let userInfoController = new UserInfoController(value)

                            userInfoController.DidRequestFollowers.Add(fun (_, username) ->
                                loadingView.Show(self.View)

                                async {
                                    let! result =
                                        service.GetFollowers(username, page)
                                        |> Async.AwaitTask

                                    match result with
                                    | Ok result ->
                                        followers <- result

                                        DispatchQueue.MainQueue.DispatchAsync(fun _ ->
                                            loadingView.Dismiss()
                                            self.Title <- username

                                            addRightNavigationItem self.NavigationItem UIBarButtonSystemItem.Add (fun _ ->
                                                addToFavorites self service userDefaults username)

                                            collectionView.ReloadData())
                                    | Error _ ->
                                        loadingView.Dismiss()

                                        presentFGAlertOnMainThread
                                            "Error"
                                            "Error while processing request. Please try again later."
                                            self
                                }
                                |> Async.Start)

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
                            service.GetFollowers(username, page)
                            |> Async.AwaitTask

                        match result with
                        | Ok result ->
                            followers <- result

                            DispatchQueue.MainQueue.DispatchAsync(fun _ ->
                                loadingView.Dismiss()
                                self.Title <- username
                                self.CollectionView.ReloadData())
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
                                disposable <- performSearch searchController self
                            | _ -> followers <- result }

                self.CollectionView.Delegate <- self.FollowerCollectionViewDelegate
            else
                loadingView.Dismiss()
                emptyView.Show self.View "No Followers" )