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
            new FollowerListViewController(GitHubService(), favorite.login)
        viewController.NavigationController.PushViewController(destinationVC, true)

type FavoritesTableViewDataSource(favorites: Follower List) =
    inherit UITableViewDataSource()
    
    override __.GetCell(tableView: UITableView, indexPath) =
        let cell =
            tableView.DequeueReusableCell(FavoriteCell.CellId, indexPath) :?> FavoriteCell
        let follower = favorites.[int indexPath.Item]
        cell.SetUp(follower, GitHubService())
        upcast cell

    override __.RowsInSection(_, _) = nint favorites.Length
    
type FavoriteListViewController() as self =
    inherit UITableViewController()

    let userDefaults = UserDefaultsService.Instance

    override __.ViewDidLoad() =
        base.ViewDidLoad()
        self.View.BackgroundColor <- UIColor.SystemBackgroundColor
        self.NavigationController.NavigationBar.PrefersLargeTitles <- true
        
        self.TableView <- new UITableView(self.View.Bounds)
        self.TableView.RowHeight <- nfloat 100.
        self.TableView.SeparatorStyle <- UITableViewCellSeparatorStyle.None
        self.TableView.RegisterClassForCellReuse(typeof<FavoriteCell>, FavoriteCell.CellId)

    override __.ViewWillAppear(_) =
        base.ViewWillAppear(true)
        match userDefaults.GetFavorites() with
        | Present favorites ->
            self.TableView.Delegate <- new FavoritesTableViewDelegate(favorites, self)
            self.TableView.DataSource <- new FavoritesTableViewDataSource(favorites)
            self.TableView.ReloadData()
        | NotPresent ->
            showEmptyView ("No Favorites", self)
