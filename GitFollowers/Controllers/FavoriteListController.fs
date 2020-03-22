namespace GitFollowers.ViewControllers

open FSharp.Json
open GitFollowers
open GitFollowers.Models
open GitFollowers.Views.Cells
open UIKit
open System
open GitFollowers.Views.Extensions
open FSharp.Json

type FavoriteListViewController() as self =
    inherit UIViewController()
    override __.ViewDidLoad() =
        base.ViewDidLoad()

        self.View.BackgroundColor <- UIColor.SystemBackgroundColor

        let favorites = PersistenceService.Instance
        let result = favorites.RetrieveFavorites()
        let favorites = Json.deserialize<Follower list> result
        self.ConfigureTableView(favorites)

        //let loadingView = showLoadingView(self.View)
//        match (NetworkService.getUserInfo "") |> Async.RunSynchronously with
//        | Ok follower ->
//            loadingView.Dismiss()
//            self.ConfigureTableView(follower)
//        | Error _ ->
//            loadingView.Dismiss()
//            showEmptyView("This user has no favorites.", self)

    member __.ConfigureTableView(followers:  Follower list) =
        let tableView = new UITableView(frame = self.View.Bounds)
        tableView.TranslatesAutoresizingMaskIntoConstraints <- false
        tableView.RowHeight <- nfloat 100.
        self.View.AddSubview tableView
        tableView.DataSource <- {
            new UITableViewDataSource() with
                member __.GetCell(tableView, indexPath) =
                    let cell = tableView.DequeueReusableCell(FavoriteCell.CellId, indexPath) :?> FavoriteCell
                    let follower = followers.[int indexPath.Item]
                    let user = User.CreateUser(follower.login, follower.avatar_url)
                    cell.User <- user
                    upcast cell
                member __.RowsInSection(tableView, section) =
                    nint followers.Length
        }

        tableView.RegisterClassForCellReuse(typeof<FavoriteCell>, FavoriteCell.CellId)

    override __.ViewWillAppear(_) =
        base.ViewWillAppear(true)
        self.NavigationController.NavigationBar.PrefersLargeTitles <- true
