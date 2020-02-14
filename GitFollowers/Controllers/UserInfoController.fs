namespace GitFollowers.Controllers

open System
open GitFollowers.Models
open GitFollowers.Views.Labels
open UIKit

type UserInfoController(follower: Follower) as self =
    inherit UIViewController()

    let titleLabel = new FGBodyLabel(follower.login, UITextAlignment.Center, nint 2)

    let configureTitleLabel() =
        NSLayoutConstraint.ActivateConstraints([|
            titleLabel.TopAnchor.ConstraintEqualTo(self.View.SafeAreaLayoutGuide.TopAnchor);
            titleLabel.LeadingAnchor.ConstraintEqualTo(self.View.LeadingAnchor);
            titleLabel.TrailingAnchor.ConstraintEqualTo(self.View.TrailingAnchor);
            titleLabel.HeightAnchor.ConstraintEqualTo(nfloat 50.)
        |])

    let configureViewController()=
        self.View.BackgroundColor <- UIColor.SystemBackgroundColor
        let doneButton = new UIBarButtonItem(UIBarButtonSystemItem.Done)
        doneButton.Clicked.Add(fun _ -> self.DismissModalViewController(true))
        self.NavigationItem.RightBarButtonItem <- doneButton
    override v.ViewDidLoad() =
        base.ViewDidLoad()
        self.View.AddSubview titleLabel
        configureViewController()
        configureTitleLabel()







