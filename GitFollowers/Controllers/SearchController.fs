namespace GitFollowers.ViewControllers

open CoreFoundation
open GitFollowers
open GitFollowers.Views.Buttons
open GitFollowers.Views.TextFields
open GitFollowers.Views.ViewControllers
open System
open UIKit

type SearchViewController() as self =
    inherit UIViewController()

    let logoImageView = new UIImageView(TranslatesAutoresizingMaskIntoConstraints = false,
                                        Image = UIImage.FromBundle("gh-logo.png"),
                                        ContentMode = UIViewContentMode.ScaleAspectFit)
    let actionButton = new FGButton(UIColor.SystemGreenColor, "Get followers")
    let userNameTextField = new FGTextField("Enter username")

    let presentFGAlertOnMainThread(title , message) =
        DispatchQueue.MainQueue.DispatchAsync(fun _ ->
            let alertVC =
                new FGAlertVC(title, message, "Ok")
            alertVC.ModalPresentationStyle <- UIModalPresentationStyle.OverFullScreen
            alertVC.ModalTransitionStyle <- UIModalTransitionStyle.CrossDissolve
            self.PresentViewController(alertVC, true, null))

    let handleNavigationController() =
        match userNameTextField.Text <> "" with
        | false ->
            presentFGAlertOnMainThread("Empty Username", "Please enter a username . We need to know who to look for 😀")
        | _ ->
            match NetworkService.getFollowers (userNameTextField.Text) with
            | Ok followers ->
                let foloowerListVC = new FollowerListViewController(followers)
                foloowerListVC.Title <- userNameTextField.Text
                self.NavigationController.PushViewController(foloowerListVC, animated = true)
                userNameTextField.ResignFirstResponder() |> ignore

            | Error _ ->
                presentFGAlertOnMainThread ("Error", "No userName found")

    let configureController() =
        self.View.BackgroundColor <- UIColor.SystemBackgroundColor

    let configureLogoImageView() =
        self.View.AddSubview(logoImageView)
        NSLayoutConstraint.ActivateConstraints
            ([| logoImageView.TopAnchor.ConstraintEqualTo
                    (self.View.SafeAreaLayoutGuide.TopAnchor, constant = nfloat 80.)
                logoImageView.CenterXAnchor.ConstraintEqualTo(self.View.CenterXAnchor)
                logoImageView.HeightAnchor.ConstraintEqualTo(constant = nfloat 200.)
                logoImageView.WidthAnchor.ConstraintEqualTo(constant = nfloat 200.) |])

    let configureUserNameTextField() =
        self.View.AddSubview(userNameTextField)
        userNameTextField.ClearButtonMode <- UITextFieldViewMode.WhileEditing
        userNameTextField.Delegate <-
            { new UITextFieldDelegate() with
                member __.ShouldReturn(textField) =
                    handleNavigationController()
                    true }
        NSLayoutConstraint.ActivateConstraints
            ([| userNameTextField.TopAnchor.ConstraintEqualTo(logoImageView.BottomAnchor, constant = nfloat 50.)
                userNameTextField.LeadingAnchor.ConstraintEqualTo(self.View.LeadingAnchor, constant = nfloat 50.)
                userNameTextField.TrailingAnchor.ConstraintEqualTo(self.View.TrailingAnchor, constant = nfloat -50.0)
                userNameTextField.HeightAnchor.ConstraintEqualTo(constant = nfloat 50.) |])

    let configureActionButton() =
        self.View.AddSubview(actionButton)
        actionButton.TouchUpInside.Add(fun _ -> handleNavigationController())
        NSLayoutConstraint.ActivateConstraints
            ([| actionButton.LeadingAnchor.ConstraintEqualTo(self.View.LeadingAnchor, constant = nfloat 50.)
                actionButton.TrailingAnchor.ConstraintEqualTo(self.View.TrailingAnchor, constant = nfloat -50.0)
                actionButton.BottomAnchor.ConstraintEqualTo
                    (self.View.SafeAreaLayoutGuide.BottomAnchor, constant = nfloat -50.0)
                actionButton.HeightAnchor.ConstraintEqualTo(constant = nfloat 50.) |])

    override __.ViewDidLoad() =
        base.ViewDidLoad()
        configureController()
        configureLogoImageView()
        configureUserNameTextField()
        configureActionButton()

    override __.ViewWillAppear(_) =
        base.ViewWillAppear(true)
        self.NavigationController.SetNavigationBarHidden(hidden = true, animated = true)
        userNameTextField.Text <- ""
