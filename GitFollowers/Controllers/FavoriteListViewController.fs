namespace GitFollowers

open System
open GitFollowers
open GitFollowers.Models
open GitFollowers.Views
open UIKit

[<AutoOpen>]
module FavoriteListController =
    type FavoriteListViewController() as self =
        inherit UIViewController()

        let userDefaults = UserDefaults.Instance

        let mutable followers: Follower list = []

        override __.ViewDidLoad() =
            base.ViewDidLoad()
            self.View.BackgroundColor <- UIColor.SystemBackgroundColor

        member __.ConfigureTableView(followers: Follower list) =
            let tableView = new UITableView(self.View.Bounds)
            tableView.TranslatesAutoresizingMaskIntoConstraints <- false
            tableView.RowHeight <- nfloat 100.
            self.View.AddSubview tableView
            tableView.Delegate <-
                { new UITableViewDelegate() with
                    member __.RowSelected(tableView, indexPath) =
                        let favorite = followers.[int indexPath.Row]

                        let destinationVC =
                            new FollowerListViewController(GitHubService(), favorite.login)

                        self.NavigationController.PushViewController(destinationVC, true) }

            tableView.DataSource <-
                { new UITableViewDataSource() with
                    member __.GetCell(tableView, indexPath) =
                        let cell =
                            tableView.DequeueReusableCell(FavoriteCell.CellId, indexPath) :?> FavoriteCell
                        let follower = followers.[int indexPath.Item]
                        cell.SetUp(follower)
                        upcast cell

                    member __.RowsInSection(tableView, section) = nint followers.Length }

            tableView.RegisterClassForCellReuse(typeof<FavoriteCell>, FavoriteCell.CellId)

        override __.ViewWillAppear(_) =
            base.ViewWillAppear(true)
            self.NavigationController.NavigationBar.PrefersLargeTitles <- true
            let favorites = userDefaults.RetrieveFavorites()
            match favorites with
            | Ok fav ->
                self.ConfigureTableView(fav)
                followers <- fav
            | Error _ -> showEmptyView ("No Favorites", self)
