namespace GitFollowers

open System
open GitFollowers.Controllers
open GitFollowers.Persistence
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
        
    let createExchangesViewController: UINavigationController =
        let favouriteVC = new ExchangeOverviewController()
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
        async {
            GitHubRepository.connect.AsTask()
            |> Async.AwaitTask
            |> ignore
            
            ExchangeRepository.connectExchange.AsTask()
            |> Async.AwaitTask
            |> ignore
            
            ExchangeRepository.connectCurrencies.AsTask()
            |> Async.AwaitTask
            |> ignore
        }|> Async.Start
        
        this.Window <- new UIWindow(UIScreen.MainScreen.Bounds)
        this.Window.RootViewController <- creteTabBar
        this.Window.MakeKeyAndVisible()
        configureNavigationBar

        true
