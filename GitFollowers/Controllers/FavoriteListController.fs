namespace GitFollowers.ViewControllers

open UIKit
type FavoriteListViewController() =
    inherit UIViewController()

     override x.ViewDidLoad() =

            base.ViewDidLoad()
            base.View.BackgroundColor <- UIColor.SystemBackgroundColor

