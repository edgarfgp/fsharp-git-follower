namespace GitFollowers.ViewControllers

open GitFollowers.Buttons
open GitFollowers.TextFields
open System
open UIKit

type SearchViewController() =
    inherit UIViewController()

    let logoImageView =
        new UIImageView(
            TranslatesAutoresizingMaskIntoConstraints = false, Image = UIImage.FromBundle("gh-logo.png"),
            ContentMode = UIViewContentMode.ScaleAspectFit)
    let actionButton = new FGButton(UIColor.SystemGreenColor, "Get followers")

    let userNameTextField = new FGTextField("Enter username")

    override this.ViewDidLoad() =
        base.ViewDidLoad()
        base.View.BackgroundColor <- UIColor.SystemBackgroundColor
        base.View.AddSubview(logoImageView)
        base.View.AddSubview(actionButton)
        base.View.AddSubview(userNameTextField)

        NSLayoutConstraint.ActivateConstraints
            ([| logoImageView.TopAnchor.ConstraintEqualTo
                    (base.View.SafeAreaLayoutGuide.TopAnchor, constant = nfloat 80.)
                logoImageView.CenterXAnchor.ConstraintEqualTo(base.View.CenterXAnchor)
                logoImageView.HeightAnchor.ConstraintEqualTo(constant = nfloat 200.)
                logoImageView.WidthAnchor.ConstraintEqualTo(constant = nfloat 200.) |])

        NSLayoutConstraint.ActivateConstraints
            ([| userNameTextField.TopAnchor.ConstraintEqualTo(logoImageView.BottomAnchor, constant = nfloat 50.)
                userNameTextField.LeadingAnchor.ConstraintEqualTo(base.View.LeadingAnchor, constant = nfloat 50.)
                userNameTextField.TrailingAnchor.ConstraintEqualTo(base.View.TrailingAnchor, constant = nfloat -50.0)
                userNameTextField.HeightAnchor.ConstraintEqualTo(constant = nfloat 50.) |])

        NSLayoutConstraint.ActivateConstraints
            ([| actionButton.LeadingAnchor.ConstraintEqualTo(base.View.LeadingAnchor, constant = nfloat 50.)
                actionButton.TrailingAnchor.ConstraintEqualTo(base.View.TrailingAnchor, constant = nfloat -50.0)
                actionButton.BottomAnchor.ConstraintEqualTo
                    (base.View.SafeAreaLayoutGuide.BottomAnchor, constant = nfloat -50.0)
                actionButton.HeightAnchor.ConstraintEqualTo(constant = nfloat 50.) |])

    override x.ViewWillAppear(_) =
        base.ViewWillAppear(true)
        base.NavigationController.SetNavigationBarHidden(hidden = true, animated = true)

