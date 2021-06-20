namespace GitFollowers

open System
open Foundation
open GitFollowers
open UIKit

type FavoriteListViewController() =
    inherit UITableViewController()

    let mutable favorites = []
    
    let rec removeItem predicate list =
        match list with
        | h::t when predicate h -> t
        | h::t -> h::removeItem predicate t
        | _ -> []

    override self.ViewDidLoad() =
        base.ViewDidLoad()
        
        self.TableView.BackgroundColor <- UIColor.SystemBackgroundColor
        self.NavigationController.NavigationBar.PrefersLargeTitles <- true
        self.TableView.RowHeight <- nfloat 100.
        self.TableView.SeparatorStyle <- UITableViewCellSeparatorStyle.None
        self.TableView.RegisterClassForCellReuse(typeof<FavoriteCell>, FavoriteCell.CellId)

    override self.ViewWillAppear _ =
        base.ViewWillAppear(true)

        match UserDefaultsService.getFavorites with
        | Some result ->
            favorites <- result

            if favorites.IsEmpty then
                self.ShowEmptyView("No Favorites")
            else
                self.DismissEmptyView()
                self.TableView.Delegate <- self.FavoritesTableViewDelegate
                self.TableView.DataSource <- self.FavoritesTableViewDataSource
                self.TableView.ReloadData()
        | None -> self.PresentAlert "Favorites" "Unable to load favorites"

    member private self.FavoritesTableViewDelegate : UITableViewDelegate =
        { new UITableViewDelegate() with
            member this.RowSelected(_, indexPath: NSIndexPath) =
                let favorite = favorites.[int indexPath.Row]

                let destinationVC =
                    new FollowerListViewController(favorite.login)

                self.NavigationController.PushViewController(destinationVC, true) }

    member self.FavoritesTableViewDataSource : UITableViewDataSource =
        { new UITableViewDataSource() with
            member this.CommitEditingStyle(tableView, editingStyle, indexPath) =
                match editingStyle with
                | UITableViewCellEditingStyle.Delete ->
                    let favoriteToDelete = favorites.[indexPath.Row]

                    let favoriteStatus =
                        UserDefaultsService.removeFavorite favoriteToDelete

                    match favoriteStatus with
                    | RemovedOk ->
                        let updatedFavorites =
                            favorites
                            |> removeItem (fun f -> f.login = favoriteToDelete.login)

                        favorites <- updatedFavorites

                        if updatedFavorites.IsEmpty then
                            self.ShowEmptyView("No Favorites")
                        else
                            self.DismissEmptyView()
                            tableView.DeleteRows([| indexPath |], UITableViewRowAnimation.Left)

                        self.TableView.ReloadData()

                    | _ -> self.PresentAlert "Favorites" "Unable to delete"

                | _ -> failwith "Unrecognized UITableViewCellEditingStyle"

            member this.GetCell(tableView: UITableView, indexPath) =
                let cell =
                    tableView.DequeueReusableCell(FavoriteCell.CellId, indexPath) :?> FavoriteCell

                let follower = favorites.[int indexPath.Item]
                cell.SetUp(follower)
                upcast cell

            member this.RowsInSection(_, _) = nint favorites.Length }
