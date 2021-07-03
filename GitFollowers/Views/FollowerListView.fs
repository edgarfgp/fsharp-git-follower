namespace GitFollowers.Views

open System
open System.Reactive.Disposables
open FSharp.Control.Reactive
open Foundation
open GitFollowers
open GitFollowers.DTOs
open GitFollowers.Entities
open GitFollowers.Persistence
open GitFollowers.Services
open GitFollowers.Elements
open UIKit

[<Struct>]
type Section =
    private Main of NSObject
        member this.Value = this |> fun(Main m) -> m

type FollowerListView(username) as self =
    inherit UICollectionViewController(new UICollectionViewFlowLayout())

    let followers = ResizeArray()
    let persistence = FavoritesUserDefaults.Instance
    let disposables = new CompositeDisposable()    
    let mutable page : int = 1
    let dataSource =
        lazy new UICollectionViewDiffableDataSource<_, _>(
                self.CollectionView,
                UICollectionViewDiffableDataSourceCellProvider
                    (fun collectionView indexPath follower ->
                        let cell =
                            collectionView.DequeueReusableCell(FollowerCell.CellId, indexPath) :?> FollowerCell

                        let result = follower :?> Follower
                        cell.SetUp(result)
                        upcast cell))

    let updateData followers =
        let snapshot = new NSDiffableDataSourceSnapshot<_, _>()
        let main = Main(new NSObject()).Value
        snapshot.AppendSections([| main |])
        snapshot.AppendItems(followers)
        mainThread { dataSource.Value.ApplySnapshot(snapshot, true) }

    let addToFavorites userName =
        async {
            let userInfo =
                GitHubService.getUserInfo(userName).AsTask()
                |> Async.AwaitTask

            match! userInfo with
            | Ok user ->
                let follower =
                    { id = 0
                      login = user.login
                      avatar_url = user.avatar_url }

                let defaults = persistence.Save follower

                match defaults with
                | Added -> self.PresentAlertOnMainThread "Favorites" "Favorite Added"
                | FirstTimeAdded _ -> self.PresentAlertOnMainThread "Favorites" "You have added your first favorite"
                | AlreadyAdded -> self.PresentAlertOnMainThread "Favorites" "This user is already in your favorites "
            | Error _ -> self.PresentAlertOnMainThread "Error" "We can not get the user info now. Please try again later."
        }
        |> Async.Start

    let performSearch (searchTex:string) =
        match searchTex |> Option.ofString with
        | Some text ->
            let filteredResult =
                followers
                |> Seq.distinct
                |> Seq.filter (fun c -> c.login.ToLower().Contains(text.ToLower()))
                |> Seq.map Follower.toDomain
                |> Seq.toArray

            if filteredResult |> Array.isEmpty |> not then
                mainThread { updateData filteredResult }
        | _ ->
            let filteredResult =
                followers
                |> Seq.map Follower.toDomain
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
                                |> Seq.map Follower.toDomain
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
                        self.PresentAlertOnMainThread "Error" "Error while processing your request. Please try again later"
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
                followers.AddRange result

                mainThread {
                    self.DismissLoadingView()
                    self.Title <- username

                    self.AddRightNavigationItem UIBarButtonSystemItem.Add
                        |> Observable.subscribe(fun _ -> addToFavorites username)
                        |> disposables.Add

                    let data =
                        result
                        |> Seq.map Follower.toDomain
                        |> Seq.toArray

                    updateData data
                }
            | Error _ ->
                self.DismissLoadingView()
                self.PresentAlertOnMainThread "Error" "Error while processing request. Please try again later."

        }
        |> Async.Start
        
    let followerViewDelegate =
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
                            let userInfoController = new UserInfoView(value)

                            userInfoController.DidRequestFollowers
                            |> Observable.subscribe(fun username ->
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
                            self.PresentAlertOnMainThread "Error" "Error while processing request. Please try again later."
                        }
                }
                |> Async.Start }

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
        self.CollectionView.Delegate <- followerViewDelegate

        self.NavigationItem.SearchController <-
            { new UISearchController() with
                override this.ObscuresBackgroundDuringPresentation = false }

        self.NavigationItem.SearchController.SearchBar.TextChanged
        |> Observable.delay (TimeSpan.FromMilliseconds(450.))
        |> Observable.subscribe(fun args -> performSearch args.SearchText)
        |> disposables.Add

        self.ShowLoadingView()
        
        loadFollowers 1
        
    override self.ViewWillDisappear _ =
        disposables.Dispose()
        
    override self.Dispose _ =
        disposables.Dispose()

//            member this.DraggingEnded(scrollView: UIScrollView, _) =
//                let offsetY = scrollView.ContentOffset.Y
//                let contentHeight = scrollView.ContentSize.Height
//                let height = scrollView.Frame.Size.Height
//
//                if offsetY > contentHeight - height then
//                    page <- page + 1
//                    self.ShowLoadingView()
//                    loadFollowers page