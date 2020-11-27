namespace GitFollowers

open System
open Foundation
open GitFollowers
open UIKit

type FavoritesTableViewDelegate(favorites: Follower List, viewController : UITableViewController) =
    inherit UITableViewDelegate()
    
    override __.RowSelected(_, indexPath: NSIndexPath) =
        let favorite = favorites.[int indexPath.Row]
        let destinationVC =
            new FollowerListViewController(GitHubService(), UserDefaultsService(), favorite.login)
        viewController.NavigationController.PushViewController(destinationVC, true)

type FavoritesTableViewDataSource(favorites: Follower list, service: IGitHubService, userDefaults: IUserDefaultsService) =
    inherit UITableViewDataSource()

    let mutable storedFavorites = favorites
    
    override __.CommitEditingStyle(tableView, editingStyle, indexPath) =
        match editingStyle with
        | UITableViewCellEditingStyle.Delete ->
            let favoriteToDelete = storedFavorites.[indexPath.Row]
            userDefaults.RemoveFavorite favoriteToDelete
            let updatedFavorites = storedFavorites |> List.removeItem (fun f -> f.login = favoriteToDelete.login)
            storedFavorites <- updatedFavorites
            tableView.DeleteRows([|indexPath|], UITableViewRowAnimation.Left)
        | _ -> failwith ""
    
    override __.GetCell(tableView: UITableView, indexPath) =
        let cell =
            tableView.DequeueReusableCell(FavoriteCell.CellId, indexPath) :?> FavoriteCell
        let follower = storedFavorites.[int indexPath.Item]
        cell.SetUp(follower, service)
        upcast cell

    override __.RowsInSection(_, _) = nint storedFavorites.Length
    
type FavoriteListViewController(service: IGitHubService, userDefaults  : IUserDefaultsService) as self =
    inherit UITableViewController()

    override __.ViewDidLoad() =
        base.ViewDidLoad()
        self.View.BackgroundColor <- UIColor.SystemBackgroundColor
        self.NavigationController.NavigationBar.PrefersLargeTitles <- true
        
        self.TableView <- new UITableView(self.View.Bounds)
        self.TableView.RowHeight <- nfloat 100.
        self.TableView.SeparatorStyle <- UITableViewCellSeparatorStyle.None
        self.TableView.RegisterClassForCellReuse(typeof<FavoriteCell>, FavoriteCell.CellId)

    override __.ViewWillAppear _ =
        base.ViewWillAppear(true)
        match userDefaults.GetFavorites() with
        | Some favorites ->
            self.TableView.Delegate <- new FavoritesTableViewDelegate(favorites, self)
            self.TableView.DataSource <- new FavoritesTableViewDataSource(favorites, service, userDefaults)
            self.TableView.ReloadData()
        | None ->
            showEmptyView ("No Favorites", self)