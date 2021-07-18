namespace GitFollowers.Views

open System
open System.Reactive.Disposables
open GitFollowers
open GitFollowers.Controllers
open GitFollowers.DTOs
open GitFollowers.Elements
open UIKit

type SearchView() as self =
    inherit UIViewController()

    let logoImageView = new UIImageView()

    let disposables = new CompositeDisposable()

    let actionButton =
        new FGButton(UIColor.SystemGrayColor, "Get followers")

    let userNameTextField = new FGTextField("Enter username")

    let handleNavigation () =
        let userName = userNameTextField.Text
        self.ShowLoadingView()

        async {
            let! result = SearchController.getUserInfo(userName).AsTask() |> Async.AwaitTask
            match result with
            | UserReceivedEvent.Ok user ->
                mainThread {
                    self.DismissLoadingView()

                    let followerListVC = new FollowerListView(user.login)

                    self.NavigationController.PushViewController(followerListVC, animated = true)
                    userNameTextField.ResignFirstResponder() |> ignore
                }
            | UserReceivedEvent.NotFound _ ->
                mainThread {
                    self.DismissLoadingView()
                    self.PresentAlertOnMainThread "Error" $"{userName} was not found."
                }
        }
        |> Async.Start



    let textFieldDidChange () =
        let isValid =
            userNameTextField.Text
            |> SearchController.sanitizeUsername

        if isValid then
            actionButton.Enabled <- true
            actionButton.BackgroundColor <- UIColor.SystemGreenColor
        else
            actionButton.Enabled <- false
            actionButton.BackgroundColor <- UIColor.SystemGrayColor

    let configureActionButton () =
        self.View.AddSubview(actionButton)
        actionButton.Enabled <- false
        actionButton.BackgroundColor <- UIColor.SystemGrayColor
        actionButton.Enabled <- false

        actionButton.TouchUpInside
        |> Observable.subscribe (fun _ -> handleNavigation ())
        |> disposables.Add

        NSLayoutConstraint.ActivateConstraints [| actionButton.LeadingAnchor.ConstraintEqualTo(
                                                      self.View.LeadingAnchor,
                                                      constant = nfloat 50.
                                                  )
                                                  actionButton.TrailingAnchor.ConstraintEqualTo(
                                                      self.View.TrailingAnchor,
                                                      constant = nfloat -50.0
                                                  )
                                                  actionButton.BottomAnchor.ConstraintEqualTo(
                                                      self.View.SafeAreaLayoutGuide.BottomAnchor,
                                                      constant = nfloat -50.0
                                                  )
                                                  actionButton.HeightAnchor.ConstraintEqualTo(constant = nfloat 50.) |]

    let configureUserNameTextField () =
        self.View.AddSubview(userNameTextField)
        userNameTextField.ClearButtonMode <- UITextFieldViewMode.WhileEditing

        userNameTextField.Delegate <-
            { new UITextFieldDelegate() with
                member _.ShouldReturn(textField) =
                    handleNavigation ()
                    true }

        userNameTextField.EditingChanged
        |> Observable.subscribe (fun _ -> textFieldDidChange ())
        |> disposables.Add

        NSLayoutConstraint.ActivateConstraints [| userNameTextField.TopAnchor.ConstraintEqualTo(
                                                      logoImageView.BottomAnchor,
                                                      constant = nfloat 50.
                                                  )
                                                  userNameTextField.LeadingAnchor.ConstraintEqualTo(
                                                      self.View.LeadingAnchor,
                                                      constant = nfloat 50.
                                                  )
                                                  userNameTextField.TrailingAnchor.ConstraintEqualTo(
                                                      self.View.TrailingAnchor,
                                                      constant = nfloat -50.0
                                                  )
                                                  userNameTextField.HeightAnchor.ConstraintEqualTo(
                                                      constant = nfloat 50.
                                                  ) |]

    let configureLogoImageView () =
        logoImageView.TranslatesAutoresizingMaskIntoConstraints <- false
        logoImageView.ContentMode <- UIViewContentMode.ScaleAspectFill
        logoImageView.Image <- UIImage.FromBundle(ImageNames.ghLogo)
        self.View.AddSubview(logoImageView)

        NSLayoutConstraint.ActivateConstraints [| logoImageView.TopAnchor.ConstraintEqualTo(
                                                      self.View.SafeAreaLayoutGuide.TopAnchor,
                                                      constant = nfloat 80.
                                                  )
                                                  logoImageView.CenterXAnchor.ConstraintEqualTo(self.View.CenterXAnchor)
                                                  logoImageView.HeightAnchor.ConstraintEqualTo(constant = nfloat 200.)
                                                  logoImageView.WidthAnchor.ConstraintEqualTo(constant = nfloat 200.) |]

    override _.ViewDidLoad() =
        base.ViewDidLoad()
        self.NavigationController.SetNavigationBarHidden(hidden = true, animated = true)
        self.View.BackgroundColor <- UIColor.SystemBackgroundColor
        configureLogoImageView ()
        configureUserNameTextField ()
        configureActionButton ()

    override self.Dispose _ = disposables.Dispose()
