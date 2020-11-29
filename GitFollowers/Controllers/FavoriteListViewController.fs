namespace GitFollowers

open System
open Foundation
open GitFollowers
open UIKit

type FavoriteListViewController(networkService: IGitHubService, persistenceService: IUserDefaultsService) as self =
    inherit UITableViewController()

    let mutable favorites : Follower list = []

    override __.ViewDidLoad() =
        base.ViewDidLoad()
        self.TableView.BackgroundColor <- UIColor.Green
        self.NavigationController.NavigationBar.PrefersLargeTitles <- true

        self.TableView <- new UITableView(self.View.Bounds)
        self.TableView.RowHeight <- nfloat 100.
        self.TableView.SeparatorStyle <- UITableViewCellSeparatorStyle.None
        self.TableView.RegisterClassForCellReuse(typeof<FavoriteCell>, FavoriteCell.CellId)

    override __.ViewWillAppear _ =
        base.ViewWillAppear(true)

        match persistenceService.GetFavorites() with
        | Some result ->
            favorites <- result
            self.TableView.Delegate <- __.FavoritesTableViewDelegate
            self.TableView.DataSource <- __.FavoritesTableViewDataSource
            self.TableView.ReloadData()
        | None -> showEmptyView "No Favorites" self.View

    member private __.FavoritesTableViewDelegate: UITableViewDelegate =
        { new UITableViewDelegate() with
            member __.RowSelected(_, indexPath: NSIndexPath) =
                let favorite = favorites.[int indexPath.Row]

                let destinationVC =
                    new FollowerListViewController(networkService, persistenceService, favorite.login)

                self.NavigationController.PushViewController(destinationVC, true) }

    member __.FavoritesTableViewDataSource: UITableViewDataSource =
        { new UITableViewDataSource() with
            member __.CommitEditingStyle(tableView, editingStyle, indexPath) =
                match editingStyle with
                | UITableViewCellEditingStyle.Delete ->
                    let favoriteToDelete = favorites.[indexPath.Row]
                    let favoriteStatus = persistenceService.RemoveFavorite favoriteToDelete
                    match favoriteStatus with
                    | RemovedOk ->
                        let updatedFavorites =
                            favorites
                            |> List.removeItem (fun f -> f.login = favoriteToDelete.login)

                        favorites <- updatedFavorites
                        tableView.DeleteRows([| indexPath |], UITableViewRowAnimation.Left)
                        if updatedFavorites.IsEmpty then
                            showEmptyView "No Favorites" self.View

                    | _ -> presentFGAlertOnMainThread "Favorites" "Unable to delete" self

                | _ -> failwith "Unrecognized UITableViewCellEditingStyle"

            member __.GetCell(tableView: UITableView, indexPath) =
                let cell =
                    tableView.DequeueReusableCell(FavoriteCell.CellId, indexPath) :?> FavoriteCell

                let follower = favorites.[int indexPath.Item]
                cell.SetUp(follower, networkService)
                upcast cell

            member __.RowsInSection(_, _) = nint favorites.Length }