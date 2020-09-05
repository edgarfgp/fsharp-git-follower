namespace GitFollowers

open System

open System.IO
open UIKit
open Foundation


[<Register("AppDelegate")>]
type AppDelegate() =
    inherit UIApplicationDelegate()

    let getDbPath () =
        let docFolder =
            Environment.GetFolderPath(Environment.SpecialFolder.Personal)

        let libFolder =
            Path.Combine(docFolder, "..", "Library", "Databases")

        if not (Directory.Exists libFolder)
        then Directory.CreateDirectory(libFolder) |> ignore
        else ()

        Path.Combine(libFolder, "GitFollowers.db3")

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
        
        Repository.connect (getDbPath ()) |> Async.RunSynchronously |> ignore

        this.Window <- new UIWindow(UIScreen.MainScreen.Bounds)
        this.Window.RootViewController <- creteTabBar
        this.Window.MakeKeyAndVisible()
        configureNavigationBar

        true
