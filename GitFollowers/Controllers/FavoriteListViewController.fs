namespace GitFollowers

open System
open Foundation
open GitFollowers
open UIKit

type FavoriteListViewController(networkService: IGitHubService, persistenceService: IUserDefaultsService) =
    inherit UITableViewController()

    let mutable favorites = []
    
    let emptyView = FGEmptyView.Instance

    override self.ViewDidLoad() =
        base.ViewDidLoad()
        self.TableView.BackgroundColor <- UIColor.SystemBackgroundColor
        self.NavigationController.NavigationBar.PrefersLargeTitles <- true
        self.TableView.RowHeight <- nfloat 100.
        self.TableView.SeparatorStyle <- UITableViewCellSeparatorStyle.None
        self.TableView.RegisterClassForCellReuse(typeof<FavoriteCell>, FavoriteCell.CellId)

    override self.ViewWillAppear _ =
        base.ViewWillAppear(true)

        match persistenceService.GetFavorites() with
        | Some result ->
            favorites <- result
            if favorites.IsEmpty then
                emptyView.Show self.View "No Favorites"
            else
                emptyView.Dismiss()
                self.TableView.Delegate <- self.FavoritesTableViewDelegate
                self.TableView.DataSource <- self.FavoritesTableViewDataSource 
                self.TableView.ReloadData()
        | None ->
            presentFGAlertOnMainThread "Favorites" "Unable to load favorites" self

    member private self.FavoritesTableViewDelegate: UITableViewDelegate =
        { new UITableViewDelegate() with
            member this.RowSelected(_, indexPath: NSIndexPath) =
                let favorite = favorites.[int indexPath.Row]

                let destinationVC =
                    new FollowerListViewController(networkService, persistenceService, favorite.login)

                self.NavigationController.PushViewController(destinationVC, true) }

    member self.FavoritesTableViewDataSource: UITableViewDataSource =
        { new UITableViewDataSource() with
            member this.CommitEditingStyle(tableView, editingStyle, indexPath) =
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
                        if updatedFavorites.IsEmpty then
                            emptyView.Show self.View "No Favorites"
                        else
                            emptyView.Dismiss()
                            tableView.DeleteRows([| indexPath |], UITableViewRowAnimation.Left)

                        self.TableView.ReloadData()

                    | _ -> presentFGAlertOnMainThread "Favorites" "Unable to delete" self

                | _ -> failwith "Unrecognized UITableViewCellEditingStyle"

            member this.GetCell(tableView: UITableView, indexPath) =
                let cell =
                    tableView.DequeueReusableCell(FavoriteCell.CellId, indexPath) :?> FavoriteCell

                let follower = favorites.[int indexPath.Item]
                cell.SetUp(follower, networkService)
                upcast cell

            member this.RowsInSection(_, _) = nint favorites.Length }