namespace GitFollowers

open System
open CoreFoundation
open Foundation
open GitFollowers
open UIKit
open FSharp.Control.Reactive

type Section() = inherit NSObject()

type FollowerListViewController(username) as self =
    inherit UICollectionViewController(new UICollectionViewFlowLayout())

    let mutable followers = []

    let loadingView = LoadingView.Instance

    let emptyView = FGEmptyView.Instance

    let mutable page: int = 1

    let dataSource =
        lazy
            (new UICollectionViewDiffableDataSource<Section, FollowerData>
                (self.CollectionView,
                   UICollectionViewDiffableDataSourceCellProvider(fun collectionView indexPath follower ->
                       let cell =
                           collectionView.DequeueReusableCell
                               (FollowerCell.CellId, indexPath) :?> FollowerCell

                       let result = follower :?> FollowerData
                       cell.SetUp(result)
                       upcast cell)))

    let updateData followers =
        let snapshot =
            new NSDiffableDataSourceSnapshot<Section, FollowerData>()

        snapshot.AppendSections([| new Section() |])
        snapshot.AppendItems(followers)
        DispatchQueue.MainQueue.DispatchAsync(fun _ -> dataSource.Value.ApplySnapshot(snapshot, true))

    let addToFavorites userName =
        async {
            let userInfo = GitHubService.getUserInfo(userName).AsTask() |> Async.AwaitTask
            match! userInfo with
            | Ok user ->
                let favorite =
                    { id = 0
                      login = user.login
                      avatar_url = user.avatar_url }

                let defaults = UserDefaultsService.saveFavorite favorite

                match defaults with
                | Added -> presentAlert "Favorites" "Favorite Added" self
                | FirstTimeAdded _ -> presentAlert "Favorites" "You have added your first favorite" self
                | AlreadyAdded -> presentAlert "Favorites" "This user is already in your favorites " self
            | Error _ ->
                presentAlert "Error" "We can not get the user info now. Please try again later." self
        }
        |> Async.Start
        
    let convertToData(follower: Follower) : FollowerData =
        new FollowerData(follower.id, follower.login,  follower.avatar_url)
        
    let performSearch (searchTextEvent : UISearchBarTextChangedEventArgs) = 
        match searchTextEvent.SearchText |> Option.OfString with
        | Some text -> 
            let filteredResult =
                followers
                |> List.distinct
                |> List.filter (fun c ->
                   c.login.ToLower().Contains(text.ToLower()))
                |> List.map(fun follower -> new FollowerData(follower.id, follower.login,  follower.avatar_url))
                |> List.toArray

            if filteredResult |> Array.isEmpty |> not then
                DispatchQueue.MainQueue.DispatchAsync(fun _ -> updateData filteredResult) 
        | _ ->
            let filteredResult =
                followers
                |> List.map convertToData
                |> List.toArray
            DispatchQueue.MainQueue.DispatchAsync(fun _ -> updateData filteredResult)

    let performDiDRequestFollowers username =
        async {
            let! followersResult = GitHubService.getFollowers(username, page).AsTask() |> Async.AwaitTask

            match followersResult with
            | Ok result ->
                followers <- result
                DispatchQueue.MainQueue.DispatchAsync(fun _ ->
                    loadingView.Dismiss()
                    self.Title <- username

                    addRightNavigationItem self.NavigationItem UIBarButtonSystemItem.Add (fun _ ->
                        addToFavorites username)

                    let data =
                        result
                        |> List.map convertToData
                        |> List.toArray
                    updateData data
                )
            | Error _ ->
                loadingView.Dismiss()
                presentAlert "Error" "Error while processing request. Please try again later." self
        }
        |> Async.Start

    override self.ViewDidLoad() =
        base.ViewDidLoad()

        addRightNavigationItem self.NavigationItem UIBarButtonSystemItem.Add (fun _ -> addToFavorites username)

        self.Title <- username

        self.NavigationController.SetNavigationBarHidden(hidden = false, animated = true)
        self.NavigationController.NavigationBar.PrefersLargeTitles <- true
        self.CollectionView <- new UICollectionView(self.View.Bounds, CreateThreeColumnFlowLayout(self.CollectionView))
        self.CollectionView.RegisterClassForCell(typeof<FollowerCell>, FollowerCell.CellId)
        self.CollectionView.BackgroundColor <- UIColor.SystemBackgroundColor
        self.CollectionView.Delegate <- self.FollowerCollectionViewDelegate
        self.NavigationItem.SearchController <-
            { new UISearchController() with
                member this.ObscuresBackgroundDuringPresentation = false }

        self.NavigationItem.SearchController.SearchBar.TextChanged
        |> Observable.delay (TimeSpan.FromMilliseconds(450.))
        |> Observable.subscribe performSearch
        |> ignore

        loadingView.Show(self.View)

        async {
            let! followersResult = GitHubService.getFollowers(username, page).AsTask() |> Async.AwaitTask
            match followersResult with
            | Ok result ->
                followers <- result

                DispatchQueue.MainQueue.DispatchAsync(fun _ ->
                    if result.Length > 0 then
                        let data =
                            result
                            |> List.map convertToData
                            |> List.toArray

                        DispatchQueue.MainQueue.DispatchAsync(fun _ ->
                            loadingView.Dismiss()
                            updateData data)
                    else
                        loadingView.Dismiss()
                        emptyView.Show self.View "No Followers")
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
                loadingView.Show(self.View)

                async {
                    let! result =
                        GitHubService.getUserInfo(follower.login).AsTask()
                        |> Async.AwaitTask

                    match result with
                    | Ok value ->
                        DispatchQueue.MainQueue.DispatchAsync(fun _ ->
                            loadingView.Dismiss()
                            let userInfoController = new UserInfoController(value)

                            userInfoController.DidRequestFollowers.Add(fun (_, username) ->
                                loadingView.Show(self.View)
                                performDiDRequestFollowers username)

                            let navController =
                                new UINavigationController(rootViewController = userInfoController)

                            self.PresentViewController(navController, true, null))
                    | Error _ ->
                        DispatchQueue.MainQueue.DispatchAsync(fun _ ->
                            loadingView.Dismiss()

                            presentAlert
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
                        let! followersResult = GitHubService.getFollowers(username, page).AsTask() |> Async.AwaitTask

                        match followersResult with
                        | Ok result ->
                            DispatchQueue.MainQueue.DispatchAsync(fun _ ->
                                loadingView.Dismiss()

                                if result.IsEmpty |> not then
                                    followers <- result
                                    let data =
                                        followers
                                        |> List.map convertToData
                                        |> List.toArray

                                    DispatchQueue.MainQueue.DispatchAsync(fun _ -> updateData data)

                                    self.CollectionView.ScrollToItem
                                        (NSIndexPath.Create([| 0; 0 |]), UICollectionViewScrollPosition.Top, true)
                                else
                                    printfn "No more followers")
                        | Error _ -> DispatchQueue.MainQueue.DispatchAsync(fun _ -> loadingView.Dismiss())

                    }
                    |> Async.Start
                }

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