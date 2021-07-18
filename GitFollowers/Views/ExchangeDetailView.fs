namespace GitFollowers.Views

open UIKit

type ExchangeDetailView() as self =
    inherit UIViewController()

    override _.ViewDidLoad() =
        base.ViewDidLoad()
        
        self.View.BackgroundColor <- UIColor.SystemBackgroundColor

