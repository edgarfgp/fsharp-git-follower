namespace GitFollowers.ViewControllers

open System
open System.Threading
open CoreGraphics
open Foundation
open GitFollowers
open GitFollowers.Models
open GitFollowers.Views
open GitFollowers.Views.Cells
open GitFollowers.Views.ViewControllers
open GitFollowers.Views.Views
open SafariServices
open UIKit
open Extensions

type FavoriteListViewController() as self =
    inherit UIViewController()

    let userDefaults = UserDefaults.Instance
    override __.ViewDidLoad() =
        base.ViewDidLoad()
        self.View.BackgroundColor <- UIColor.SystemBackgroundColor

    member __.ConfigureTableView(followers:  Follower list) =
        let tableView = new UITableView(self.View.Bounds)
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
        let favorites = userDefaults.RetrieveFavorites()
        match favorites with
        | Ok favs ->
            self.ConfigureTableView(favs)
        | Error _  ->  showEmptyView("No Favorites", self)

and FollowerListViewController(userName : string) as self =
    inherit UIViewController()

    let loadingView = LoadingView.Instance
    let userDefaults = UserDefaults.Instance
    
    member __.userName = userName
    
    member __.GetUserInfo(userName : string) =
        let mainThread = SynchronizationContext.Current
        async {
            do! Async.SwitchToThreadPool()
            let! result = NetworkService.getUserInfo userName
            match result with
            | Ok value ->
                let follower = Follower.CreateFollower(value.login, value.avatar_url)
                let defaults = (userDefaults.Update follower)
                match defaults with
                | Ok status ->
                    match status with
                    | AlreadyExists ->
                        do! Async.SwitchToContext mainThread
                        presentFGAlertOnMainThread ("Favorite", "This user is already in your favorites ", self)
                    | FavouriteAdded ->
                        do! Async.SwitchToContext mainThread
                        presentFGAlertOnMainThread ("Favorite", "Favorite Added", self)
                | Error _ ->
                    do! Async.SwitchToContext mainThread
                    presentFGAlertOnMainThread ("Error", "Error while trying to save the favorite", self)
            | Error _ ->
                do! Async.SwitchToContext mainThread
                presentFGAlertOnMainThread ("Error", "Error while processing request. Please try again later.", self)
        }
        |> Async.Start
    
    member __.GetFollowers(userName : string) =
        loadingView.Show(self.View)
        let mainThread = SynchronizationContext.Current
        async {
            do! Async.SwitchToThreadPool()
            let! result = NetworkService.getFollowers userName
            match result with
            | Ok followers  ->
                if followers.Length > 0
                then
                    do! Async.SwitchToContext mainThread
                    loadingView.Dismiss()
                    self.ConfigureCollectionView(followers)
                else
                    do! Async.SwitchToContext mainThread
                    loadingView.Dismiss()
                    showEmptyView("This user has no followers. Go follow him", self)
            | Error _ ->
                do! Async.SwitchToContext mainThread
                loadingView.Dismiss()
                presentFGAlertOnMainThread ("Error", "Error while processing your request. Please try again later", self)    
        }
        |> Async.Start

    override __.ViewDidLoad() =
        base.ViewDidLoad()
        self.View.BackgroundColor <- UIColor.SystemBackgroundColor
        self.Title <- userName
        self.GetFollowers(userName)
        
    override __.ViewWillAppear(_) =
        base.ViewWillAppear(true)
        self.NavigationController.SetNavigationBarHidden(hidden = false, animated = true)
        self.NavigationController.NavigationBar.PrefersLargeTitles <- true
        self.NavigationItem.RightBarButtonItem <- new UIBarButtonItem(systemItem= UIBarButtonSystemItem.Add)
        self.NavigationItem.RightBarButtonItem.Clicked
        |> Event.add(fun _ -> self.AddFavoriteTapped())

    member private __.ConfigureCollectionView(followers : Follower list) =
        let collectionView = new UICollectionView(self.View.Bounds, self.CreateThreeColumnFlowLayout(self.View))
        collectionView.TranslatesAutoresizingMaskIntoConstraints <- false
        self.View.AddSubview collectionView
        collectionView.BackgroundColor <- UIColor.SystemBackgroundColor

        collectionView.Delegate <- {
            new UICollectionViewDelegate() with
                member __.ItemSelected(_, indexPath) =
                    let index = int indexPath.Item
                    let follower = followers.[index]
                    let userInfoController = new UserInfoController(follower.login)
                    let navController = new UINavigationController(rootViewController = userInfoController)
                    self.PresentViewController(navController, true, null) }

        collectionView.DataSource <- {
            new UICollectionViewDataSource() with
                member __.GetCell(collectionView, indexPath) =
                    let cell = collectionView.DequeueReusableCell(FollowerCell.CellId, indexPath) :?> FollowerCell
                    let follower = followers.[int indexPath.Item]
                    cell.Follower <- follower
                    upcast cell
                member __.GetItemsCount(_, _) =
                    nint followers.Length }

        self.NavigationItem.SearchController <-
            { new UISearchController() with
                member __.ObscuresBackgroundDuringPresentation = false }

        self.NavigationItem.SearchController.SearchResultsUpdater <-
            { new UISearchResultsUpdating() with
                member __.UpdateSearchResultsForSearchController(searchController) =
                   // Implement search logic
                   ()}

        collectionView.RegisterClassForCell(typeof<FollowerCell>, FollowerCell.CellId)

    member private __.CreateThreeColumnFlowLayout(view: UIView) =
            let width = view.Bounds.Width
            let padding  = nfloat 12.
            let minimumItemSpacing = nfloat 10.
            let availableWidth = width - (padding * nfloat 2.) - (minimumItemSpacing * nfloat 2.)
            let itemWidth = availableWidth / nfloat 3.
            let flowLayout = new  UICollectionViewFlowLayout()
            flowLayout.SectionInset <- UIEdgeInsets(padding, padding, padding, padding)
            flowLayout.ItemSize <- CGSize(itemWidth,  itemWidth + nfloat 40.)
            flowLayout

    member private __.AddFavoriteTapped() =
        self.GetUserInfo(userName)
and UserInfoController(userName : string) as self =
    inherit UIViewController()

    let padding = nfloat 20.
    let contentView = new UIView()
    let headerView = new UIView()
    let itemViewOne = new UIView()
    let itemViewTwo = new UIView()
    
    member __.userName = userName
    
    member __.GetUserInfo(userName : string) =
        let mainThread = SynchronizationContext.Current
        async {
            do! Async.SwitchToThreadPool()
            let! result = NetworkService.getUserInfo userName
            match result with
            | Ok value ->
                do! Async.SwitchToContext mainThread
                self.ConfigureElements value
            | Error _ ->
                do! Async.SwitchToContext mainThread
                presentFGAlertOnMainThread ("Error", "Error while processing request. Please try again later.", self)
        }
        |> Async.Start

    override __.ViewDidLoad() =
        base.ViewDidLoad()
        self.GetUserInfo userName
        self.ConfigureViewController()
        self.ConfigureScrollView()
        self.ConfigureContentView()

    member private __.AddChildViewController(childVC: UIViewController,containerView: UIView) =
        self.AddChildViewController childVC
        containerView.AddSubview(childVC.View)
        childVC.View.Frame <- containerView.Bounds
        childVC.DidMoveToParentViewController(self)

    member private __.ConfigureViewController () =
        let doneButton = new UIBarButtonItem(UIBarButtonSystemItem.Done)
        doneButton.Clicked.Add(fun _ -> self.DismissModalViewController(true))
        self.NavigationItem.RightBarButtonItem <- doneButton

    member private __.ConfigureContentView () =
        headerView.TranslatesAutoresizingMaskIntoConstraints <- false
        itemViewOne.TranslatesAutoresizingMaskIntoConstraints <- false
        itemViewTwo.TranslatesAutoresizingMaskIntoConstraints <- false

        contentView.AddSubviews headerView
        contentView.AddSubviews itemViewOne
        contentView.AddSubviews itemViewTwo

        NSLayoutConstraint.ActivateConstraints([|

            headerView.TopAnchor.ConstraintEqualTo(contentView.SafeAreaLayoutGuide.TopAnchor, padding)
            headerView.LeadingAnchor.ConstraintEqualTo(contentView.LeadingAnchor, padding)
            headerView.TrailingAnchor.ConstraintEqualTo(contentView.TrailingAnchor, -padding)
            headerView.HeightAnchor.ConstraintEqualTo(nfloat 210.)

            itemViewOne.TopAnchor.ConstraintEqualTo(headerView.BottomAnchor, padding)
            itemViewOne.LeadingAnchor.ConstraintEqualTo(contentView.LeadingAnchor, padding)
            itemViewOne.TrailingAnchor.ConstraintEqualTo(contentView.TrailingAnchor, -padding)
            itemViewOne.HeightAnchor.ConstraintEqualTo(nfloat 140.)

            itemViewTwo.TopAnchor.ConstraintEqualTo(itemViewOne.BottomAnchor, padding)
            itemViewTwo.LeadingAnchor.ConstraintEqualTo(contentView.LeadingAnchor, padding)
            itemViewTwo.TrailingAnchor.ConstraintEqualTo(contentView.TrailingAnchor, -padding)
            itemViewTwo.HeightAnchor.ConstraintEqualTo(nfloat 140.)
            itemViewTwo.BottomAnchor.ConstraintEqualTo(contentView.BottomAnchor)
        |])

    member private __.ConfigureScrollView () =
        let scrollView = new UIScrollView(BackgroundColor = UIColor.SystemBackgroundColor)
        self.View.AddSubview scrollView
        scrollView.AddSubview contentView

        scrollView.TranslatesAutoresizingMaskIntoConstraints <- false
        contentView.TranslatesAutoresizingMaskIntoConstraints <- false

        NSLayoutConstraint.ActivateConstraints([|
            scrollView.TopAnchor.ConstraintEqualTo(self.View.TopAnchor);
            scrollView.LeadingAnchor.ConstraintEqualTo(self.View.LeadingAnchor);
            scrollView.TrailingAnchor.ConstraintEqualTo(self.View.TrailingAnchor)
            scrollView.BottomAnchor.ConstraintEqualTo(self.View.BottomAnchor)
        |])

        NSLayoutConstraint.ActivateConstraints([|
            contentView.TopAnchor.ConstraintEqualTo(scrollView.TopAnchor)
            contentView.LeadingAnchor.ConstraintEqualTo(scrollView.LeadingAnchor)
            contentView.TrailingAnchor.ConstraintEqualTo(scrollView.TrailingAnchor)
            contentView.BottomAnchor.ConstraintEqualTo(scrollView.BottomAnchor)
        |])

        NSLayoutConstraint.ActivateConstraints([|
            contentView.WidthAnchor.ConstraintEqualTo(scrollView.WidthAnchor)
        |])

    member private __.ConfigureElements user =
        let itemInfoOne =
            new ItemInfoVC(UIColor.SystemPurpleColor, "Github Profile", ItemInfoType.Repo, user.public_repos, ItemInfoType.Gists, user.public_gists)
        let itemInfoTwo =
            new ItemInfoVC(UIColor.SystemGreenColor, "Get Followers", ItemInfoType.Followers, user.followers, ItemInfoType.Following, user.following)
        self.AddChildViewController(new FGUserInfoHeaderVC(user), headerView)
        self.AddChildViewController(itemInfoOne, itemViewOne)
        self.AddChildViewController(itemInfoTwo, itemViewTwo)

        itemInfoOne.ActionButtonClicked(fun _ ->
            let safariVC = new SFSafariViewController(url = new NSUrl(user.html_url))
            safariVC.PreferredControlTintColor <- UIColor.SystemGreenColor
            self.PresentViewController(safariVC, true, null))

        itemInfoTwo.ActionButtonClicked(fun _ ->
            let userFollowerListViewController = new FollowerListViewController(user.login)
            userFollowerListViewController.View.BackgroundColor <- UIColor.SystemBackgroundColor
            self.NavigationController.PushViewController(userFollowerListViewController, animated = true))