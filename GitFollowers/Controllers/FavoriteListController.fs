namespace GitFollowers.ViewControllers

open UIKit
type FavoriteListViewController() as self =
    inherit UIViewController()

     override v.ViewDidLoad() =
            base.ViewDidLoad()

            self.View.BackgroundColor <- UIColor.SystemBackgroundColor

