namespace GitFollowers.Controllers

open GitFollowers.Models
open UIKit

type UserInfoController(follower: Follower) as self =
    inherit UIViewController()

    override x.ViewDidLoad() =
        base.ViewDidLoad()

        self.View.BackgroundColor <- UIColor.SystemBackgroundColor



