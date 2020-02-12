namespace GitFollowers.ViewControllers

open CoreFoundation
open GitFollowers.Views.Buttons
open GitFollowers.Views.TextFields
open GitFollowers.Views.ViewControllers
open System
open UIKit

type SearchViewController() =
    inherit UIViewController()

    let logoImageView =
        new UIImageView(TranslatesAutoresizingMaskIntoConstraints = false, Image = UIImage.FromBundle("gh-logo.png"),
                        ContentMode = UIViewContentMode.ScaleAspectFit)

    let actionButton = new FGButton(UIColor.SystemGreenColor, "Get followers")

    let userNameTextField = new FGTextField("Enter username")

    member self.navigateToFollowerListController() =
        match userNameTextField.Text <> "" with
        | false -> self.presentFGAlertOnMainThread()
        | _ ->
            let foloowerListVC = new FollowerListViewController(userNameTextField.Text)
            foloowerListVC.Title <- userNameTextField.Text
            self.NavigationController.PushViewController(foloowerListVC, animated = true)
            userNameTextField.ResignFirstResponder() |> ignore

    member self.configureController() = self.View.BackgroundColor <- UIColor.SystemBackgroundColor

    member self.configureLogoImageView() =
        self.View.AddSubview(logoImageView)
        NSLayoutConstraint.ActivateConstraints
            ([| logoImageView.TopAnchor.ConstraintEqualTo
                    (base.View.SafeAreaLayoutGuide.TopAnchor, constant = nfloat 80.)
                logoImageView.CenterXAnchor.ConstraintEqualTo(base.View.CenterXAnchor)
                logoImageView.HeightAnchor.ConstraintEqualTo(constant = nfloat 200.)
                logoImageView.WidthAnchor.ConstraintEqualTo(constant = nfloat 200.) |])

    member self.configureUserNameTextField() =
        base.View.AddSubview(userNameTextField)

        userNameTextField.Delegate <-
            { new UITextFieldDelegate() with
                member x.ShouldReturn(textField) =
                    self.navigateToFollowerListController()
                    true }

        NSLayoutConstraint.ActivateConstraints
            ([| userNameTextField.TopAnchor.ConstraintEqualTo(logoImageView.BottomAnchor, constant = nfloat 50.)
                userNameTextField.LeadingAnchor.ConstraintEqualTo(base.View.LeadingAnchor, constant = nfloat 50.)
                userNameTextField.TrailingAnchor.ConstraintEqualTo(base.View.TrailingAnchor, constant = nfloat -50.0)
                userNameTextField.HeightAnchor.ConstraintEqualTo(constant = nfloat 50.) |])

    member self.presentFGAlertOnMainThread() =
        DispatchQueue.MainQueue.DispatchAsync(fun _ ->
            let alertVC =
                new FGAlertVC("Empty Username", "Please enter a username . We need to know who to look for 😀", "Ok")
            alertVC.ModalPresentationStyle <- UIModalPresentationStyle.OverFullScreen
            alertVC.ModalTransitionStyle <- UIModalTransitionStyle.CrossDissolve
            self.PresentViewController(alertVC, true, null))

    member self.configureActionButton() =
        base.View.AddSubview(actionButton)
        actionButton.TouchUpInside.Add(fun _ -> self.navigateToFollowerListController())

        NSLayoutConstraint.ActivateConstraints
            ([| actionButton.LeadingAnchor.ConstraintEqualTo(base.View.LeadingAnchor, constant = nfloat 50.)
                actionButton.TrailingAnchor.ConstraintEqualTo(base.View.TrailingAnchor, constant = nfloat -50.0)
                actionButton.BottomAnchor.ConstraintEqualTo
                    (base.View.SafeAreaLayoutGuide.BottomAnchor, constant = nfloat -50.0)
                actionButton.HeightAnchor.ConstraintEqualTo(constant = nfloat 50.) |])

    override self.ViewDidLoad() =
        base.ViewDidLoad()

        self.configureController()
        self.configureLogoImageView()
        self.configureUserNameTextField()
        self.configureActionButton()

    override self.ViewWillAppear(_) =
        base.ViewWillAppear(true)
        base.NavigationController.SetNavigationBarHidden(hidden = true, animated = true)
        userNameTextField.Text <- ""
