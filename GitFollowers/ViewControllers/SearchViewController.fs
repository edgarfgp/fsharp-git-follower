namespace GitFollowers.ViewControllers

open System
open UIKit

type SearchViewController() =
    inherit UIViewController()

    let logoImageView =
        new UIImageView
            (TranslatesAutoresizingMaskIntoConstraints = false,
             Image = UIImage.FromBundle("gh-logo.png"),
             ContentMode = UIViewContentMode.ScaleAspectFit)

    override this.ViewDidLoad() =
            base.ViewDidLoad()
            base.View.BackgroundColor <- UIColor.SystemBackgroundColor
            base.View.Add(logoImageView)

            let activeConstraints =
                [| logoImageView.TopAnchor.ConstraintEqualTo(base.View.SafeAreaLayoutGuide.TopAnchor, constant = nfloat 80.);
                  logoImageView.CenterXAnchor.ConstraintEqualTo(base.View.CenterXAnchor);
                  logoImageView.HeightAnchor.ConstraintEqualTo(constant = nfloat 200.);
                  logoImageView.WidthAnchor.ConstraintEqualTo( constant = nfloat 200.) |]

            NSLayoutConstraint.ActivateConstraints(activeConstraints)

    override x.ViewWillAppear(_) =
        base.ViewWillAppear(true)

        base.NavigationController.SetNavigationBarHidden(hidden = true, animated = true)

