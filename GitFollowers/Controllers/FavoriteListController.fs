namespace GitFollowers.ViewControllers

open CoreGraphics
open System
open UIKit

type FavoriteListViewController() as self =
    inherit UIViewController()
    override v.ViewDidLoad() =
        base.ViewDidLoad()
        self.View.BackgroundColor <- UIColor.SystemBackgroundColor

    override v.ViewWillAppear(_) =
        base.ViewWillAppear(true)
        self.NavigationController.NavigationBar.PrefersLargeTitles <- true
