namespace GitFollowers.Views

open System
open System.Reactive.Disposables
open FSharp.Control.Reactive
open Foundation
open GitFollowers
open GitFollowers.Controllers
open GitFollowers.DTOs
open GitFollowers.Entities
open GitFollowers.Persistence
open GitFollowers.Services
open GitFollowers.Elements
open UIKit

[<Struct>]
type Section =
    private
    | Main of NSObject
    member this.Value = this |> fun (Main m) -> m

type FollowerListView(userData: string) as self =
    inherit UICollectionViewController(new UICollectionViewFlowLayout())

    let followersData = ResizeArray()
    let disposables = new CompositeDisposable()

    let mutable page : int = 1
    let mutable username = userData

    let dataSource =
        lazy
            (new UICollectionViewDiffableDataSource<_, Follower>(
                self.CollectionView,
                UICollectionViewDiffableDataSourceCellProvider
                    (fun collectionView indexPath follower ->
                        let cell =
                            collectionView.DequeueReusableCell(FollowerCell.CellId, indexPath) :?> FollowerCell

                        let result = follower :?> Follower
                        cell.SetUp(result)
                        upcast cell)
            ))

    let updateData followers =
        let snapshot = new NSDiffableDataSourceSnapshot<_, _>()
        let main = Main(new NSObject()).Value
        snapshot.AppendSections([| main |])
        snapshot.AppendItems(followers)
        mainThread { dataSource.Value.ApplySnapshot(snapshot, true) }

    let performSearch (searchTex: string) =
        match searchTex |> Option.ofNullableString with
        | Some text ->
            let filteredResult =
                followersData
                |> Seq.distinct
                |> Seq.filter (fun c -> c.login.ToLower().Contains(text.ToLower()))
                |> Seq.map Follower.toDomain
                |> Seq.toArray

            if filteredResult |> Array.isEmpty |> not then
                mainThread { updateData filteredResult }
        | _ ->
            let filteredResult =
                followersData
                |> Seq.map Follower.toDomain
                |> Seq.toArray

            mainThread { updateData filteredResult }

    let loadFollowers page =
        async {
            let! result =
                FollowerListController
                    .getFollowers(username, page)
                    .AsTask()
                |> Async.AwaitTask

            match result with
            | FollowersReceivedEvent.Ok followers ->
                followersData.AddRange followers

                mainThread {
                    self.Title <- username

                    let data =
                        followersData
                        |> Seq.map Follower.toDomain
                        |> Seq.toArray

                    self.DismissLoadingView()
                    updateData data
                }
            | FollowersReceivedEvent.Empty ->
                mainThread {
                    if followersData |> Seq.isEmpty then
                        self.ShowEmptyView("No Followers")
                    else
                        self.DismissLoadingView()
                }

            | FollowersReceivedEvent.Error error ->
                mainThread {
                    match error with
                    | GitHubError.Non200Response ->
                        self.PresentAlertOnMainThread "Error" "Error while processing request. Please try again later."
                    | GitHubError.ParseError _ ->
                        self.PresentAlertOnMainThread "Error" "Error while processing request. Please try again later."
                }
        }
        |> Async.Start

    let followerViewDelegate =
        { new UICollectionViewDelegate() with
            member this.ItemSelected(collectionView, indexPath) =
                let index = int indexPath.Item
                let follower = followersData.[index]
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
                            |> Observable.subscribe
                                (fun user ->
                                    username <- user
                                    self.ShowLoadingView()

                                    for x in followersData.ToArray() do
                                        followersData.Remove(x) |> ignore

                                    loadFollowers page)
                            |> disposables.Add

                            let navController =
                                new UINavigationController(rootViewController = userInfoController)

                            self.PresentViewController(navController, true, null)
                        }
                    | Error _ ->
                        mainThread {
                            self.DismissLoadingView()

                            self.PresentAlertOnMainThread
                                "Error"
                                "Error while processing request. Please try again later."
                        }
                }
                |> Async.Start

            member this.DraggingEnded(scrollView: UIScrollView, _) =
                let offsetY = scrollView.ContentOffset.Y
                let contentHeight = scrollView.ContentSize.Height
                let height = scrollView.Frame.Size.Height

                if (offsetY > contentHeight - height) then
                    page <- page + 1
                    self.ShowLoadingView()
                    loadFollowers page }

    override self.ViewDidLoad() =
        base.ViewDidLoad()

        self.AddRightNavigationItem UIBarButtonSystemItem.Add
        |> Observable.flatmapAsync
            (fun _ ->
                async {
                    let! result =
                        FollowerListController
                            .addToFavorites(username)
                            .AsTask()
                        |> Async.AwaitTask

                    match result with
                    | Added -> self.PresentAlertOnMainThread "Favorites" "Favorite Added"
                    | FirstTimeAdded _ -> self.PresentAlertOnMainThread "Favorites" "You have added your first favorite"
                    | AlreadyAdded ->
                        self.PresentAlertOnMainThread "Favorites" "This user is already in your favorites "
                    | NotAdded ->
                        self.PresentAlertOnMainThread
                            "Error"
                            "We can not get the user info now. Please try again later."
                })
        |> Observable.subscribe (fun _ -> ())
        |> disposables.Add

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
        |> Observable.subscribe (fun args -> performSearch args.SearchText)
        |> disposables.Add

        self.ShowLoadingView()

        loadFollowers 1

    override self.ViewWillDisappear _ = disposables.Dispose()

    override self.Dispose _ = disposables.Dispose()
