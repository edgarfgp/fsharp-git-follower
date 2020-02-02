namespace GitFollowers.ViewControllers

open UIKit
type FavouriteListViewController() =
    inherit UIViewController()

     override x.ViewDidLoad() =

            base.ViewDidLoad()
            base.View.BackgroundColor <- UIColor.SystemBackgroundColor

