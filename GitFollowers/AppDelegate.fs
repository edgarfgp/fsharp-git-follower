namespace GitFollowers

open System
open UIKit
open Foundation


[<Register("AppDelegate")>]
type AppDelegate() =
    inherit UIApplicationDelegate()

    let createSearchViewController: UINavigationController =
        let searchVC = new SearchViewController()
        searchVC.Title <- "Search"
        searchVC.TabBarItem <- new UITabBarItem(UITabBarSystemItem.Search, nint 0)

        new UINavigationController(searchVC)

    let createFavouriteViewController: UINavigationController =
        let favouriteVC = new FavoriteListViewController()
        favouriteVC.Title <- "Favourites"
        favouriteVC.TabBarItem <- new UITabBarItem(UITabBarSystemItem.Favorites, nint 1)

        new UINavigationController(favouriteVC)

    let creteTabBar: UITabBarController =
        let tabBar = new UITabBarController()
        UITabBar.Appearance.TintColor <- UIColor.SystemGreenColor
        tabBar.ViewControllers <-
            [| createSearchViewController
               createFavouriteViewController |]
        tabBar

    let configureNavigationBar =
        UINavigationBar.Appearance.TintColor <- UIColor.SystemGreenColor

    override val Window = null with get, set

    override this.FinishedLaunching(_, _) =
        async {
            let! _ = Repository.connect
            ()
        }|> Async.Start

        this.Window <- new UIWindow(UIScreen.MainScreen.Bounds)
        this.Window.RootViewController <- creteTabBar
        this.Window.MakeKeyAndVisible()
        configureNavigationBar

        true
