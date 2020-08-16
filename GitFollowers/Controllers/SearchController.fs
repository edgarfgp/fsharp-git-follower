namespace GitFollowers.ViewControllers

open GitFollowers
open GitFollowers.Helpers
open GitFollowers.Helpers
open GitFollowers.Helpers
open GitFollowers.Helpers
open GitFollowers.Views
open GitFollowers.Views.Buttons
open GitFollowers.Views.TextFields
open System
open GitFollowers.Views.Views
open UIKit
open Extensions

type SearchViewController() as self =
    inherit UIViewController()

    let logoImageView = new UIImageView()
    let actionButton = new FGButton(UIColor.SystemGrayColor, "Get followers")
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
        userNameTextField.Text <- String.Empty
        actionButton.Enabled <- false
        actionButton.BackgroundColor <- UIColor.SystemGrayColor

    member __.HandleNavigation() =
        let userName =
            userNameTextField.Text
            |> Option.OfString

        match userName with
        | Some text ->
            let followerListVC = new FollowerListViewController(text)
            self.NavigationController.PushViewController(followerListVC, animated = true)
            userNameTextField.ResignFirstResponder() |> ignore
        | _ ->
            presentFGAlertOnMainThread("Empty Username", "Please enter a username . We need to know who to look for 😀", self)

    member __.ConfigureLogoImageView() =
        logoImageView.TranslatesAutoresizingMaskIntoConstraints <- false
        logoImageView.ContentMode <- UIViewContentMode.ScaleAspectFill
        logoImageView.Image <- UIImage.FromBundle("gh-logo.png")
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
            { new UITextFieldDelegate()
                with member __.ShouldReturn(textField) =
                        self.HandleNavigation()
                        true }
        userNameTextField.EditingChanged
        |> Event.add(fun _ -> self.UserNameTextFieldDidChange())

        NSLayoutConstraint.ActivateConstraints
            ([| userNameTextField.TopAnchor.ConstraintEqualTo(logoImageView.BottomAnchor, constant = nfloat 50.)
                userNameTextField.LeadingAnchor.ConstraintEqualTo(self.View.LeadingAnchor, constant = nfloat 50.)
                userNameTextField.TrailingAnchor.ConstraintEqualTo(self.View.TrailingAnchor, constant = nfloat -50.0)
                userNameTextField.HeightAnchor.ConstraintEqualTo(constant = nfloat 50.) |])

    member __.UserNameTextFieldDidChange() =
        let text = userNameTextField.Text
        if text <> "" then
            actionButton.Enabled <- true
            actionButton.BackgroundColor <- UIColor.SystemGreenColor
        else
            actionButton.Enabled <- false
            actionButton.BackgroundColor <- UIColor.SystemGrayColor
            
    member __.ConfigureActionButton() =
        self.View.AddSubview(actionButton)
        actionButton.Enabled <- false
        actionButton.TouchUpInside
        |> Event.add(fun _ -> self.HandleNavigation())

        NSLayoutConstraint.ActivateConstraints
            ([| actionButton.LeadingAnchor.ConstraintEqualTo(self.View.LeadingAnchor, constant = nfloat 50.)
                actionButton.TrailingAnchor.ConstraintEqualTo(self.View.TrailingAnchor, constant = nfloat -50.0)
                actionButton.BottomAnchor.ConstraintEqualTo
                    (self.View.SafeAreaLayoutGuide.BottomAnchor, constant = nfloat -50.0)
                actionButton.HeightAnchor.ConstraintEqualTo(constant = nfloat 50.) |])
