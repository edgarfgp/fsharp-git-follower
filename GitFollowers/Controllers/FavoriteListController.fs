namespace GitFollowers.ViewControllers

open UIKit

type FavoriteListViewController() as self =
    inherit UIViewController()
    override __.ViewDidLoad() =
        base.ViewDidLoad()
        self.View.BackgroundColor <- UIColor.SystemBackgroundColor

    override __.ViewWillAppear(_) =
        base.ViewWillAppear(true)
        self.NavigationController.NavigationBar.PrefersLargeTitles <- true
