namespace GitFollowers

open System.Reactive.Disposables
open FSharp.Control.Reactive
open Foundation
open GitFollowers
open UIKit

type Section =
    | Main
    with static member Value = new NSObject()

type FollowerListViewController(username) as self =
    inherit UICollectionViewController(new UICollectionViewFlowLayout())

    let mutable followers = ResizeArray()
    
    let disposables = new CompositeDisposable()

    let mutable page : int = 1

    let dataSource =
        lazy
            (new UICollectionViewDiffableDataSource<_, FollowerObject>(
                self.CollectionView,
                UICollectionViewDiffableDataSourceCellProvider
                    (fun collectionView indexPath follower ->
                        let cell =
                            collectionView.DequeueReusableCell(FollowerCell.CellId, indexPath) :?> FollowerCell

                        let result = follower :?> FollowerObject
                        cell.SetUp(result)
                        upcast cell)
            ))

    let updateData followers =
        let snapshot =
            new NSDiffableDataSourceSnapshot<_, FollowerObject>()

        snapshot.AppendSections([| Section.Value |])
        snapshot.AppendItems(followers)
        mainThread { dataSource.Value.ApplySnapshot(snapshot, true) }

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

    let performSearch (searchTex:string) =
        match searchTex |> OfString with
        | Some text ->
            let filteredResult =
                followers
                |> Seq.distinct
                |> Seq.filter (fun c -> c.login.ToLower().Contains(text.ToLower()))
                |> Seq.map FollowerObject.Create
                |> Seq.toArray

            if filteredResult |> Array.isEmpty |> not then
                mainThread { updateData filteredResult }
        | _ ->
            let filteredResult =
                followers
                |> Seq.map FollowerObject.Create
                |> Seq.toArray

            mainThread { updateData filteredResult }
            
            
    let loadFollowers page = 
            async {
                let! followersResult =
                    GitHubService
                        .getFollowers(username, page)
                        .AsTask()
                    |> Async.AwaitTask

                match followersResult with
                | Ok result ->
                    followers.AddRange result
                    mainThread {
                        if result |> Seq.isEmpty |> not then
                            let data =
                                result
                                |> Seq.map FollowerObject.Create
                                |> Seq.toArray
                            self.DismissLoadingView()
                            updateData data
                        else
                            self.DismissLoadingView()
                            self.ShowEmptyView("No Followers")
                    }
                           
                | Error _ ->
                    mainThread {
                        self.DismissLoadingView()
                        self.PresentAlert "Error" "Error while processing your request. Please try again later"
                    }
            }
            |> Async.Start

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

                mainThread {
                    self.DismissLoadingView()
                    self.Title <- username

                    self.AddRightNavigationItem UIBarButtonSystemItem.Add
                        |> Observable.subscribe(fun _ -> addToFavorites username)
                        |> disposables.Add

                    let data =
                        result
                        |> Seq.map FollowerObject.Create
                        |> Seq.toArray

                    updateData data
                }
            | Error _ ->
                self.DismissLoadingView()
                self.PresentAlert "Error" "Error while processing request. Please try again later."

        }
        |> Async.Start

    override self.ViewDidLoad() =
        base.ViewDidLoad()

        self.AddRightNavigationItem UIBarButtonSystemItem.Add
        |> Observable.subscribe(fun _ -> addToFavorites username)
        |> disposables.Add

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
        |> Observable.delay (System.TimeSpan.FromMilliseconds(450.))
        |> Observable.subscribe(fun args -> performSearch args.SearchText)
        |> disposables.Add

        self.ShowLoadingView()
        
        loadFollowers 1
        
    override self.ViewWillDisappear _ =
        disposables.Dispose()
        
    override self.Dispose _ =
        disposables.Dispose()

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
                        mainThread {
                            self.DismissLoadingView()
                            let userInfoController = new UserInfoController(value)

                            userInfoController.DidRequestFollowers
                            |> Observable.subscribe(fun (_, username) ->
                                    self.ShowLoadingView()
                                    performDiDRequestFollowers username)
                            |> disposables.Add
                               

                            let navController =
                                new UINavigationController(rootViewController = userInfoController)

                            self.PresentViewController(navController, true, null)
                        }
                    | Error _ ->
                        mainThread {
                            self.DismissLoadingView()
                            self.PresentAlert "Error" "Error while processing request. Please try again later."
                        }
                }
                |> Async.Start

            member this.DraggingEnded(scrollView: UIScrollView, _) =
                let offsetY = scrollView.ContentOffset.Y
                let contentHeight = scrollView.ContentSize.Height
                let height = scrollView.Frame.Size.Height

                if offsetY > contentHeight - height then
                    page <- page + 1
                    self.ShowLoadingView()
                    loadFollowers page
            }