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

    let favoritesData = ResizeArray<DTOs.Follower>()
    let persistence = FavoritesUserDefaults.Instance
    
    let favoritesDelegate =
        { new UITableViewDelegate() with
            member this.RowSelected(_, indexPath: NSIndexPath) =
                let favorite = favoritesData.[int indexPath.Row]

                let destinationVC =
                    new FollowerListViewController(favorite.login)

                self.NavigationController.PushViewController(destinationVC, true) }
        
    let favoritesDataSource =
        { new UITableViewDataSource() with
            member this.CommitEditingStyle(tableView, editingStyle, indexPath) =
                match editingStyle with
                | UITableViewCellEditingStyle.Delete ->
                    let favoriteToDelete = favoritesData.[indexPath.Row]
                    let favoriteStatus =
                        persistence.Remove favoriteToDelete

                    match favoriteStatus with
                    | RemovedOk ->
                        favoritesData.Remove(favoriteToDelete) |> ignore

                        if favoritesData.Count = 0 then
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

                let follower = favoritesData.[int indexPath.Item]
                cell.SetUp(follower)
                upcast cell

            member this.RowsInSection(_, _) = nint favoritesData.Count }
    
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
                for x in favoritesData.ToArray() do
                    favoritesData.Remove(x) |> ignore

                favoritesData.AddRange result
                mainThread {
                    self.DismissEmptyView()
                    self.TableView.ReloadData()
                }
        | None -> self.ShowEmptyView("No Favorites")
