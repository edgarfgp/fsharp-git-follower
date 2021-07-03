namespace GitFollowers

open System
open GitFollowers.Views
open UIKit
open Foundation

[<Register("AppDelegate")>]
type AppDelegate() =
    inherit UIApplicationDelegate()
        
    let createSearchViewController: UINavigationController =
        let searchVC = new SearchView()
        searchVC.Title <- "Search"
        searchVC.TabBarItem <- new UITabBarItem(UITabBarSystemItem.Search, nint 0)

        new UINavigationController(searchVC)

    let createFavouriteViewController: UINavigationController =
        let favouriteVC = new FavoriteListView()
        favouriteVC.Title <- "Favourites"
        favouriteVC.TabBarItem <- new UITabBarItem(UITabBarSystemItem.Favorites, nint 1)

        new UINavigationController(favouriteVC)
        
    let createExchangesViewController: UINavigationController =
        let favouriteVC = new ExchangeListView()
        favouriteVC.Title <- "Exchanges"
        let currenciesImage =  UIImage.GetSystemImage(ImageNames.currencies)

        favouriteVC.TabBarItem <- new UITabBarItem("Exchanges", currenciesImage, nint 0)

        new UINavigationController(favouriteVC)

    let creteTabBar: UITabBarController =
        let tabBar = new UITabBarController()
        UITabBar.Appearance.TintColor <- UIColor.SystemGreenColor
        tabBar.ViewControllers <-
            [| createSearchViewController
               createFavouriteViewController
               createExchangesViewController |]
        tabBar

    let configureNavigationBar =
        UINavigationBar.Appearance.TintColor <- UIColor.SystemGreenColor

    override val Window = null with get, set

    override this.FinishedLaunching(_, _) =
//        async {
//            let! result = ExchangeRepository.Instance.connect.AsTask() |> Async.AwaitTask
//            result.TableMappings
//            |> Seq.iter(fun table -> printfn $"{table.TableName} has been created" )
//        }|> Async.Start
//        
        this.Window <- new UIWindow(UIScreen.MainScreen.Bounds)
        this.Window.RootViewController <- creteTabBar
        this.Window.MakeKeyAndVisible()
        configureNavigationBar

        true
