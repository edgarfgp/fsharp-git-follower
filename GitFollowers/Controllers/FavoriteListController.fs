namespace GitFollowers.ViewControllers

open GitFollowers
open GitFollowers.Models
open GitFollowers.Views.Cells
open UIKit
open System
open GitFollowers.Views.Extensions

type FavoriteListViewController() as self =
    inherit UIViewController()

    let userDefaults = UserDefaults.Instance

    let tableView = new UITableView()
    override __.ViewDidLoad() =
        base.ViewDidLoad()
        self.View.BackgroundColor <- UIColor.SystemBackgroundColor

    member __.ConfigureTableView(followers:  Follower list) =
        tableView.Frame <- self.View.Bounds
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
        match userDefaults.RetrieveFavorites() with
        | Some favourites ->
            match favourites with
            | [] ->
                showEmptyView("No Favorites", self)
            | _ ->
                self.ConfigureTableView(favourites)

        | None  ->  presentFGAlertOnMainThread("Error", "Error getting Favorites", self)



