namespace GitFollowers.ViewControllers

open System
open UIKit

type FollowerListViewController () =
    inherit UIViewController()
    override self.ViewDidLoad() =
        base.ViewDidLoad()
        base.View.BackgroundColor <- UIColor.SystemBackgroundColor

    override self.ViewWillAppear(_) =
        base.ViewWillAppear(true)
        base.NavigationController.SetNavigationBarHidden(hidden = false, animated = true)
        base.NavigationController.NavigationBar.PrefersLargeTitles <- true





