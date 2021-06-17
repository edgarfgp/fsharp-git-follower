namespace GitFollowers

open System
open CoreFoundation
open Foundation
open GitFollowers
open UIKit
open FSharp.Control.Reactive

type Section() =
    inherit NSObject()

type FollowerListViewController(username) as self =
    inherit UICollectionViewController(new UICollectionViewFlowLayout())

    let mutable followers = []

    let mutable page : int = 1

    let dataSource =
        lazy
            (new UICollectionViewDiffableDataSource<Section, FollowerData>(
                self.CollectionView,
                UICollectionViewDiffableDataSourceCellProvider
                    (fun collectionView indexPath follower ->
                        let cell =
                            collectionView.DequeueReusableCell(FollowerCell.CellId, indexPath) :?> FollowerCell

                        let result = follower :?> FollowerData
                        cell.SetUp(result)
                        upcast cell)
            ))

    let updateData followers =
        let snapshot =
            new NSDiffableDataSourceSnapshot<Section, FollowerData>()

        snapshot.AppendSections([| new Section() |])
        snapshot.AppendItems(followers)
        DispatchQueue.MainQueue.DispatchAsync(fun _ -> dataSource.Value.ApplySnapshot(snapshot, true))

    let addToFavorites userName =
        async {
            let userInfo =
                GitHubService.getUserInfo(userName).AsTask()
                |> Async.AwaitTask

            match! userInfo with
            | Ok user ->
                let favorite =
                    { id = 0
                      login = user.login
                      avatar_url = user.avatar_url }

                let defaults =
                    UserDefaultsService.saveFavorite favorite

                match defaults with
                | Added -> self.PresentAlert "Favorites" "Favorite Added"
                | FirstTimeAdded _ -> self.PresentAlert "Favorites" "You have added your first favorite"
                | AlreadyAdded -> self.PresentAlert "Favorites" "This user is already in your favorites "
            | Error _ -> self.PresentAlert "Error" "We can not get the user info now. Please try again later."
        }
        |> Async.Start

    let convertToData (follower: Follower) : FollowerData =
        new FollowerData(follower.id, follower.login, follower.avatar_url)

    let performSearch (searchTextEvent: UISearchBarTextChangedEventArgs) =
        match searchTextEvent.SearchText |> Option.OfString with
        | Some text ->
            let filteredResult =
                followers
                |> List.distinct
                |> List.filter (fun c -> c.login.ToLower().Contains(text.ToLower()))
                |> List.map (fun follower -> new FollowerData(follower.id, follower.login, follower.avatar_url))
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
            let! followersResult =
                GitHubService
                    .getFollowers(username, page)
                    .AsTask()
                |> Async.AwaitTask

            match followersResult with
            | Ok result ->
                followers <- result

                DispatchQueue.MainQueue.DispatchAsync
                    (fun _ ->
                        self.DismissLoadingView()
                        self.Title <- username

                        self.AddRightNavigationItem UIBarButtonSystemItem.Add (fun _ -> addToFavorites username)

                        let data =
                            result |> List.map convertToData |> List.toArray

                        updateData data)
            | Error _ ->
                self.DismissLoadingView()
                self.PresentAlert "Error" "Error while processing request. Please try again later."
        }
        |> Async.Start

    override self.ViewDidLoad() =
        base.ViewDidLoad()

        self.AddRightNavigationItem UIBarButtonSystemItem.Add (fun _ -> addToFavorites username)

        self.Title <- username

        self.NavigationController.SetNavigationBarHidden(hidden = false, animated = true)
        self.NavigationController.NavigationBar.PrefersLargeTitles <- true

        self.CollectionView <-
            new UICollectionView(self.View.Bounds, self.CollectionView.CreateThreeColumnFlowLayout(self.CollectionView))

        self.CollectionView.RegisterClassForCell(typeof<FollowerCell>, FollowerCell.CellId)
        self.CollectionView.BackgroundColor <- UIColor.SystemBackgroundColor
        self.CollectionView.Delegate <- self.FollowerCollectionViewDelegate

        self.NavigationItem.SearchController <-
            { new UISearchController() with
                override this.ObscuresBackgroundDuringPresentation = false }

        self.NavigationItem.SearchController.SearchBar.TextChanged
        |> Observable.delay (TimeSpan.FromMilliseconds(450.))
        |> Observable.subscribe performSearch
        |> ignore

        self.ShowLoadingView()

        async {
            let! followersResult =
                GitHubService
                    .getFollowers(username, page)
                    .AsTask()
                |> Async.AwaitTask

            match followersResult with
            | Ok result ->
                followers <- result

                DispatchQueue.MainQueue.DispatchAsync
                    (fun _ ->
                        if result.Length > 0 then
                            let data =
                                result |> List.map convertToData |> List.toArray

                            DispatchQueue.MainQueue.DispatchAsync
                                (fun _ ->
                                    self.DismissLoadingView()
                                    updateData data)
                        else
                            self.DismissLoadingView()
                            self.ShowEmptyView("No Followers"))
                           
            | Error _ ->
                DispatchQueue.MainQueue.DispatchAsync
                    (fun _ ->
                        self.DismissLoadingView()
                        self.ShowAlertAndGoBack())
        }
        |> Async.Start

    member self.FollowerCollectionViewDelegate =
        { new UICollectionViewDelegate() with

            member this.ItemSelected(collectionView, indexPath) =
                let index = int indexPath.Item
                let follower = followers.[index]
                self.ShowLoadingView()

                async {
                    let! result =
                        GitHubService.getUserInfo(follower.login).AsTask()
                        |> Async.AwaitTask

                    match result with
                    | Ok value ->
                        DispatchQueue.MainQueue.DispatchAsync
                            (fun _ ->
                                self.DismissLoadingView()
                                let userInfoController = new UserInfoController(value)

                                userInfoController.DidRequestFollowers.Add
                                    (fun (_, username) ->
                                        self.ShowLoadingView()
                                        performDiDRequestFollowers username)

                                let navController =
                                    new UINavigationController(rootViewController = userInfoController)

                                self.PresentViewController(navController, true, null))
                    | Error _ ->
                        DispatchQueue.MainQueue.DispatchAsync
                            (fun _ ->
                                self.DismissLoadingView()

                                self.PresentAlert "Error" "Error while processing request. Please try again later.")
                }
                |> Async.Start

            member this.DraggingEnded(scrollView: UIScrollView, _) =
                let offsetY = scrollView.ContentOffset.Y
                let contentHeight = scrollView.ContentSize.Height
                let height = scrollView.Frame.Size.Height

                if offsetY > contentHeight - height then
                    page <- page + 1
                    self.ShowLoadingView()

                    async {
                        let! followersResult =
                            GitHubService
                                .getFollowers(username, page)
                                .AsTask()
                            |> Async.AwaitTask

                        match followersResult with
                        | Ok result ->
                            DispatchQueue.MainQueue.DispatchAsync
                                (fun _ ->
                                    self.DismissLoadingView()

                                    if result.IsEmpty |> not then
                                        followers <- result

                                        let data =
                                            followers
                                            |> List.map convertToData
                                            |> List.toArray

                                        DispatchQueue.MainQueue.DispatchAsync(fun _ -> updateData data)

                                        self.CollectionView.ScrollToItem(
                                            NSIndexPath.Create([| 0; 0 |]),
                                            UICollectionViewScrollPosition.Top,
                                            true
                                        )
                                    else
                                        printfn "No more followers")
                        | Error _ -> DispatchQueue.MainQueue.DispatchAsync(fun _ -> self.DismissLoadingView())

                    }
                    |> Async.Start }

    member self.ShowAlertAndGoBack() =
        let alertVC =
            new FGAlertVC("Error", "Error while processing your request. Please try again later", "Ok")

        alertVC.ModalPresentationStyle <- UIModalPresentationStyle.OverFullScreen
        alertVC.ModalTransitionStyle <- UIModalTransitionStyle.CrossDissolve
        self.PresentViewController(alertVC, true, null)

        alertVC.ActionButtonClicked
            (fun _ ->
                alertVC.DismissViewController(true, null)

                self.NavigationController.PopToRootViewController(true)
                |> ignore)
