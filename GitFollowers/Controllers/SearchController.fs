namespace GitFollowers.ViewControllers

open GitFollowers
open GitFollowers.Views
open GitFollowers.Views.Buttons
open GitFollowers.Views.TextFields
open System
open UIKit
open Extensions

type SearchViewController() as self =
    inherit UIViewController()

    let logoImageView = new UIImageView(TranslatesAutoresizingMaskIntoConstraints = false,
                                        Image = UIImage.FromBundle("gh-logo.png"),
                                        ContentMode = UIViewContentMode.ScaleAspectFit)
    let actionButton = new FGButton(UIColor.SystemGreenColor, "Get followers")
    let userNameTextField = new FGTextField("Enter username")

    override __.ViewDidLoad() =
        base.ViewDidLoad()
        self.View.BackgroundColor <- UIColor.SystemBackgroundColor
        self.ConfigureLogoImageView()
        self.ConfigureUserNameTextField()
        self.ConfigureActionButton()

    override __.ViewWillAppear(_) =
        base.ViewWillAppear(true)
        self.NavigationController.SetNavigationBarHidden(hidden = true, animated = true)
        userNameTextField.Text <- ""


    member __.HandleNavigation() =
        match userNameTextField.Text <> "" with
        | false ->
             presentFGAlertOnMainThread("Empty Username", "Please enter a username . We need to know who to look for 😀", self)
        | _ ->
            match NetworkService.getUserInfo userNameTextField.Text with
            | Ok value ->
                let foloowerListVC = new FollowerListViewController(value.login)
                self.NavigationController.PushViewController(foloowerListVC, animated = true)
                userNameTextField.ResignFirstResponder() |> ignore
            | Error _->
                presentFGAlertOnMainThread ("Error", "No userName found", self)

    member __.ConfigureLogoImageView() =
        self.View.AddSubview(logoImageView)
        NSLayoutConstraint.ActivateConstraints
            ([| logoImageView.TopAnchor.ConstraintEqualTo
                    (self.View.SafeAreaLayoutGuide.TopAnchor, constant = nfloat 80.)
                logoImageView.CenterXAnchor.ConstraintEqualTo(self.View.CenterXAnchor)
                logoImageView.HeightAnchor.ConstraintEqualTo(constant = nfloat 200.)
                logoImageView.WidthAnchor.ConstraintEqualTo(constant = nfloat 200.) |])

    member __.ConfigureUserNameTextField() =
        self.View.AddSubview(userNameTextField)
        userNameTextField.ClearButtonMode <- UITextFieldViewMode.WhileEditing
        userNameTextField.Delegate <-
            { new UITextFieldDelegate() with
                member __.ShouldReturn(textField) =
                    self.HandleNavigation()
                    true }
        NSLayoutConstraint.ActivateConstraints
            ([| userNameTextField.TopAnchor.ConstraintEqualTo(logoImageView.BottomAnchor, constant = nfloat 50.)
                userNameTextField.LeadingAnchor.ConstraintEqualTo(self.View.LeadingAnchor, constant = nfloat 50.)
                userNameTextField.TrailingAnchor.ConstraintEqualTo(self.View.TrailingAnchor, constant = nfloat -50.0)
                userNameTextField.HeightAnchor.ConstraintEqualTo(constant = nfloat 50.) |])

    member __.ConfigureActionButton() =
        self.View.AddSubview(actionButton)
        actionButton.TouchUpInside.Add(fun _ -> self.HandleNavigation())
        NSLayoutConstraint.ActivateConstraints
            ([| actionButton.LeadingAnchor.ConstraintEqualTo(self.View.LeadingAnchor, constant = nfloat 50.)
                actionButton.TrailingAnchor.ConstraintEqualTo(self.View.TrailingAnchor, constant = nfloat -50.0)
                actionButton.BottomAnchor.ConstraintEqualTo
                    (self.View.SafeAreaLayoutGuide.BottomAnchor, constant = nfloat -50.0)
                actionButton.HeightAnchor.ConstraintEqualTo(constant = nfloat 50.) |])
