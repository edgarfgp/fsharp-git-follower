namespace GitFollowers.Controllers

open System
open Foundation
open GitFollowers
open GitFollowers
open GitFollowers.Persistence
open GitFollowers.Views
open UIKit

type FavoriteListViewController() as self =
    inherit UITableViewController()

    let favorites = ResizeArray<DTOs.Follower>()
    let persistence = FavoritesUserDefaults.Instance
    
    let favoritesDelegate =
        { new UITableViewDelegate() with
            member this.RowSelected(_, indexPath: NSIndexPath) =
                let favorite = favorites.[int indexPath.Row]

                let destinationVC =
                    new FollowerListViewController(favorite.login)

                self.NavigationController.PushViewController(destinationVC, true) }
        
    let favoritesDataSource =
        { new UITableViewDataSource() with
            member this.CommitEditingStyle(tableView, editingStyle, indexPath) =
                match editingStyle with
                | UITableViewCellEditingStyle.Delete ->
                    let favoriteToDelete = favorites.[indexPath.Row]
                    let favoriteStatus =
                        persistence.Remove favoriteToDelete

                    match favoriteStatus with
                    | RemovedOk ->
                        favorites.Remove(favoriteToDelete) |> ignore

                        if favorites.Count = 0 then
                            self.ShowEmptyView("No Favorites")
                        else
                            self.DismissEmptyView()
                            tableView.DeleteRows([| indexPath |], UITableViewRowAnimation.Left)

                        self.TableView.ReloadData()

                    | _ -> self.PresentAlertOnMainThread "Favorites" "Unable to delete"

                | _ -> failwith "Unrecognized UITableViewCellEditingStyle"

            member this.GetCell(tableView: UITableView, indexPath) =
                let cell =
                    tableView.DequeueReusableCell(FavoriteCell.CellId, indexPath) :?> FavoriteCell

                let follower = favorites.[int indexPath.Item]
                cell.SetUp(follower)
                upcast cell

            member this.RowsInSection(_, _) = nint favorites.Count }
    
    override self.ViewDidLoad() =
        base.ViewDidLoad()

        self.TableView.BackgroundColor <- UIColor.SystemBackgroundColor
        self.NavigationController.NavigationBar.PrefersLargeTitles <- true
        self.TableView.RowHeight <- nfloat 100.
        self.TableView.SeparatorStyle <- UITableViewCellSeparatorStyle.None
        self.TableView.RegisterClassForCellReuse(typeof<FavoriteCell>, FavoriteCell.CellId)
        self.TableView.Delegate <- favoritesDelegate
        self.TableView.DataSource <- favoritesDataSource

    override self.ViewWillAppear _ =
        base.ViewWillAppear(true)

        match persistence.GetAll with
        | Some result ->
            if result.Length = 0 then
                self.ShowEmptyView("No Favorites")
            else
                for x in favorites.ToArray() do
                    favorites.Remove(x) |> ignore

                favorites.AddRange result
                mainThread {
                    self.DismissEmptyView()
                    self.TableView.ReloadData()
                }
        | None -> self.PresentAlertOnMainThread "Favorites" "Unable to load favorites"
