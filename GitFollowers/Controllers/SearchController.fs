namespace GitFollowers

open System
open GitFollowers
open UIKit

type SearchViewController() as self =
    inherit UIViewController()

    let logoImageView = new UIImageView()

    let actionButton =
        new FGButton(UIColor.SystemGrayColor, "Get followers")

    let userNameTextField = new FGTextField("Enter username")
    
    let handleNavigation() =
        let userName =
            userNameTextField.Text |> Option.OfString

        match userName with
        | Some text ->
            let followerListVC = new FollowerListViewController(text)
            self.NavigationController.PushViewController(followerListVC, animated = true)
            userNameTextField.ResignFirstResponder() |> ignore
        | _ ->
            self.PresentAlert
                "Empty Username" "Please enter a username . We need to know who to look for 😀"
                
    let userNameTextFieldDidChange() =
        let text = userNameTextField.Text
        if text <> "" then
            actionButton.Enabled <- true
            actionButton.BackgroundColor <- UIColor.SystemGreenColor
        else
            actionButton.Enabled <- false
            actionButton.BackgroundColor <- UIColor.SystemGrayColor
                
    let configureActionButton() =
            self.View.AddSubview(actionButton)
            actionButton.Enabled <- false
            actionButton.TouchUpInside
            |> Event.add (fun _ -> handleNavigation())

            NSLayoutConstraint.ActivateConstraints
                [| actionButton.LeadingAnchor.ConstraintEqualTo(self.View.LeadingAnchor, constant = nfloat 50.)
                   actionButton.TrailingAnchor.ConstraintEqualTo(self.View.TrailingAnchor, constant = nfloat -50.0)
                   actionButton.BottomAnchor.ConstraintEqualTo
                       (self.View.SafeAreaLayoutGuide.BottomAnchor, constant = nfloat -50.0)
                   actionButton.HeightAnchor.ConstraintEqualTo(constant = nfloat 50.) |]
                
    let configureUserNameTextField() =
        self.View.AddSubview(userNameTextField)
        userNameTextField.ClearButtonMode <- UITextFieldViewMode.WhileEditing
        userNameTextField.Delegate <-
            { new UITextFieldDelegate() with
                member _.ShouldReturn(textField) =
                    handleNavigation()
                    true
                }
        userNameTextField.EditingChanged
        |> Event.add (fun _ -> userNameTextFieldDidChange())

        NSLayoutConstraint.ActivateConstraints
            [| userNameTextField.TopAnchor.ConstraintEqualTo(logoImageView.BottomAnchor, constant = nfloat 50.)
               userNameTextField.LeadingAnchor.ConstraintEqualTo(self.View.LeadingAnchor, constant = nfloat 50.)
               userNameTextField.TrailingAnchor.ConstraintEqualTo(self.View.TrailingAnchor, constant = nfloat -50.0)
               userNameTextField.HeightAnchor.ConstraintEqualTo(constant = nfloat 50.) |]
            
    let configureLogoImageView() =
        logoImageView.TranslatesAutoresizingMaskIntoConstraints <- false
        logoImageView.ContentMode <- UIViewContentMode.ScaleAspectFill
        logoImageView.Image <- UIImage.FromBundle(ImageNames.ghLogo)
        self.View.AddSubview(logoImageView)
        NSLayoutConstraint.ActivateConstraints
            [| logoImageView.TopAnchor.ConstraintEqualTo
                   (self.View.SafeAreaLayoutGuide.TopAnchor, constant = nfloat 80.)
               logoImageView.CenterXAnchor.ConstraintEqualTo(self.View.CenterXAnchor)
               logoImageView.HeightAnchor.ConstraintEqualTo(constant = nfloat 200.)
               logoImageView.WidthAnchor.ConstraintEqualTo(constant = nfloat 200.) |]

    override _.ViewDidLoad() =
        base.ViewDidLoad()
        self.View.BackgroundColor <- UIColor.SystemBackgroundColor
        configureLogoImageView()
        configureUserNameTextField()
        configureActionButton()

    override _.ViewWillAppear _ =
        base.ViewWillAppear(true)
        self.NavigationController.SetNavigationBarHidden(hidden = true, animated = true)
        userNameTextField.Text <- String.Empty
        actionButton.Enabled <- false
        actionButton.BackgroundColor <- UIColor.SystemGrayColor