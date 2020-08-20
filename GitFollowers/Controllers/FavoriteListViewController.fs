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

        override __.ViewDidLoad() =
            base.ViewDidLoad()
            self.View.BackgroundColor <- UIColor.SystemBackgroundColor

        member __.ConfigureTableView(followers: Follower list) =
            let tableView = new UITableView(self.View.Bounds)
            tableView.TranslatesAutoresizingMaskIntoConstraints <- false
            tableView.RowHeight <- nfloat 100.
            self.View.AddSubview tableView
            tableView.DataSource <-
                { new UITableViewDataSource() with
                    member __.GetCell(tableView, indexPath) =
                        let cell =
                            tableView.DequeueReusableCell(FavoriteCell.CellId, indexPath) :?> FavoriteCell

                        let follower = followers.[int indexPath.Item]

                        let user =
                            { login = follower.login
                              avatar_url = follower.avatar_url
                              name = None
                              location = None
                              bio = None
                              public_repos = 0
                              public_gists = 0
                              html_url = ""
                              following = 0
                              followers = 0
                              created_at = DateTime.Now }

                        cell.SetUp user
                        upcast cell

                    member __.RowsInSection(tableView, section) = nint followers.Length }

            tableView.RegisterClassForCellReuse(typeof<FavoriteCell>, FavoriteCell.CellId)

        override __.ViewWillAppear(_) =
            base.ViewWillAppear(true)
            self.NavigationController.NavigationBar.PrefersLargeTitles <- true
            let favorites = userDefaults.RetrieveFavorites()
            match favorites with
            | Ok fav -> self.ConfigureTableView(fav)
            | Error _ -> showEmptyView ("No Favorites", self)

