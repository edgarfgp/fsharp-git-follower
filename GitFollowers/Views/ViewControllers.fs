namespace GitFollowers

open System
open System.Threading
open CoreFoundation
open Foundation
open UIKit

type FGAlertVC(title: string, message: string, buttonTitle: string) as self =
    inherit UIViewController()
    let containerView = new UIView()

    let titleLabel =
        new FGTitleLabel(UITextAlignment.Center, nfloat 20.)

    let messageLabel = new FGBodyLabel()

    let actionButton =
        new FGButton(UIColor.SystemPinkColor, "Ok")

    let padding = nfloat 20.

    do
        self.View.BackgroundColor <- UIColor.Black.ColorWithAlpha(nfloat 0.75)

        messageLabel.Text <- message
        messageLabel.TextAlignment <- UITextAlignment.Center
        messageLabel.Lines <- nint 1

        titleLabel.Text <- title
        self.View.AddSubview containerView
        containerView.AddSubview titleLabel
        containerView.AddSubview actionButton
        containerView.AddSubview messageLabel

        messageLabel.Lines <- nint 4
        actionButton.SetTitle(buttonTitle, UIControlState.Normal)

        containerView.BackgroundColor <- UIColor.SystemBackgroundColor
        containerView.Layer.CornerRadius <- nfloat 16.
        containerView.Layer.BorderWidth <- nfloat 2.
        containerView.Layer.BorderColor <- UIColor.White.CGColor
        containerView.TranslatesAutoresizingMaskIntoConstraints <- false

        NSLayoutConstraint.ActivateConstraints
            ([| containerView.CenterYAnchor.ConstraintEqualTo(self.View.CenterYAnchor)
                containerView.CenterXAnchor.ConstraintEqualTo(self.View.CenterXAnchor)
                containerView.WidthAnchor.ConstraintEqualTo(constant = nfloat 280.)
                containerView.HeightAnchor.ConstraintEqualTo(constant = nfloat 220.) |])

        NSLayoutConstraint.ActivateConstraints
            ([| titleLabel.TopAnchor.ConstraintEqualTo(containerView.TopAnchor, padding)
                titleLabel.LeadingAnchor.ConstraintEqualTo(containerView.LeadingAnchor, constant = padding)
                titleLabel.TrailingAnchor.ConstraintEqualTo(containerView.TrailingAnchor, constant = -padding)
                titleLabel.HeightAnchor.ConstraintEqualTo(nfloat 28.) |])

        NSLayoutConstraint.ActivateConstraints
            ([| messageLabel.TopAnchor.ConstraintEqualTo(titleLabel.BottomAnchor, constant = nfloat 8.)
                messageLabel.LeadingAnchor.ConstraintEqualTo(containerView.LeadingAnchor, constant = padding)
                messageLabel.TrailingAnchor.ConstraintEqualTo(containerView.TrailingAnchor, constant = -padding)
                messageLabel.BottomAnchor.ConstraintEqualTo(actionButton.TopAnchor, constant = nfloat -12.0) |])

        NSLayoutConstraint.ActivateConstraints
            ([| actionButton.BottomAnchor.ConstraintEqualTo(containerView.BottomAnchor, -padding)
                actionButton.LeadingAnchor.ConstraintEqualTo(containerView.LeadingAnchor, constant = padding)
                actionButton.TrailingAnchor.ConstraintEqualTo(containerView.TrailingAnchor, constant = -padding)
                actionButton.HeightAnchor.ConstraintEqualTo(nfloat 44.) |])

    member __.ActionButtonClicked(handler) =
        actionButton.TouchUpInside |> Event.add handler

type FGUserInfoHeaderVC(user: User) as self =
    inherit UIViewController()

    let textImageViewPadding = nfloat 12.
    let avatarImageView = new FGAvatarImageView()

    let userNameLabel =
        new FGTitleLabel(UITextAlignment.Left, nfloat 34.)

    let nameLabel = new FGSecondaryTitleLabel(nfloat 18.)
    let locationImageView = new UIImageView()
    let locationLabel = new FGSecondaryTitleLabel(nfloat 18.)
    let bioLabel = new FGBodyLabel()
    
    let githubService = GitHubService() :> IGitHubService

    do
        avatarImageView.TranslatesAutoresizingMaskIntoConstraints <- false
        userNameLabel.TranslatesAutoresizingMaskIntoConstraints <- false
        nameLabel.TranslatesAutoresizingMaskIntoConstraints <- false
        locationImageView.TranslatesAutoresizingMaskIntoConstraints <- false
        locationLabel.TranslatesAutoresizingMaskIntoConstraints <- false
        bioLabel.TranslatesAutoresizingMaskIntoConstraints <- false

        self.View.AddSubview avatarImageView
        self.View.AddSubview userNameLabel
        self.View.AddSubview nameLabel
        self.View.AddSubview locationImageView
        self.View.AddSubview locationLabel
        self.View.AddSubview bioLabel

        userNameLabel.Text <- user.login
        nameLabel.Text <-
            match user.name with
            | Some value -> value
            | None -> "N/A"

        locationLabel.Text <-
            match user.location with
            | Some value -> value
            | None -> "N/A"

        bioLabel.TextAlignment <- UITextAlignment.Left
        bioLabel.Lines <- nint 3
        bioLabel.Text <-
            match user.bio with
            | Some value -> value
            | None -> "N/A"

        locationImageView.Image <- UIImage.GetSystemImage(ImageNames.location)
        locationImageView.TintColor <- UIColor.SecondaryLabelColor

        NSLayoutConstraint.ActivateConstraints
            ([| avatarImageView.TopAnchor.ConstraintEqualTo(self.View.TopAnchor)
                avatarImageView.LeadingAnchor.ConstraintEqualTo(self.View.LeadingAnchor)
                avatarImageView.HeightAnchor.ConstraintEqualTo(nfloat 90.)
                avatarImageView.WidthAnchor.ConstraintEqualTo(nfloat 90.)

                userNameLabel.TopAnchor.ConstraintEqualTo(avatarImageView.TopAnchor)
                userNameLabel.LeadingAnchor.ConstraintEqualTo(avatarImageView.TrailingAnchor, textImageViewPadding)
                userNameLabel.TrailingAnchor.ConstraintEqualTo(self.View.TrailingAnchor)
                userNameLabel.HeightAnchor.ConstraintEqualTo(nfloat 38.)

                nameLabel.CenterYAnchor.ConstraintEqualTo(avatarImageView.CenterYAnchor, nfloat 8.)
                nameLabel.LeadingAnchor.ConstraintEqualTo(avatarImageView.TrailingAnchor, textImageViewPadding)
                nameLabel.TrailingAnchor.ConstraintEqualTo(self.View.TrailingAnchor)
                nameLabel.HeightAnchor.ConstraintEqualTo(nfloat 38.)

                locationImageView.BottomAnchor.ConstraintEqualTo(avatarImageView.BottomAnchor)
                locationImageView.LeadingAnchor.ConstraintEqualTo(avatarImageView.TrailingAnchor, textImageViewPadding)
                locationImageView.WidthAnchor.ConstraintEqualTo(nfloat 20.)
                locationImageView.HeightAnchor.ConstraintEqualTo(nfloat 20.)

                locationLabel.CenterYAnchor.ConstraintEqualTo(locationImageView.CenterYAnchor)
                locationLabel.LeadingAnchor.ConstraintEqualTo(locationImageView.TrailingAnchor, nfloat 5.)
                locationLabel.TrailingAnchor.ConstraintEqualTo(self.View.TrailingAnchor)
                locationLabel.HeightAnchor.ConstraintEqualTo(nfloat 20.)

                bioLabel.TopAnchor.ConstraintEqualTo(avatarImageView.BottomAnchor)
                bioLabel.LeadingAnchor.ConstraintEqualTo(avatarImageView.LeadingAnchor)
                bioLabel.TrailingAnchor.ConstraintEqualTo(self.View.TrailingAnchor)
                bioLabel.HeightAnchor.ConstraintEqualTo(nfloat 90.) |])

        async {
            let! result = githubService.DownloadDataFromUrl(user.avatar_url) |> Async.AwaitTask

            match result with
            | Ok data ->
                DispatchQueue.MainQueue.DispatchAsync(fun _ ->
                    avatarImageView.Image <- data)
            | Error _ ->
                DispatchQueue.MainQueue.DispatchAsync(fun _ ->
                    avatarImageView.Image <- UIImage.FromBundle(ImageNames.ghLogo))
        }
        |> Async.Start

type ItemInfoVC(backgroundColor: UIColor,
                text: string,
                itemInfoOneType: ItemInfoType,
                itemInfoOneCount: int,
                itemInfoTwoType: ItemInfoType,
                itemInfoTwoCount: int) as self =
    inherit UIViewController()
    let padding = nfloat 20.
    let stackView = new UIStackView()

    let itemInfoViewOne =
        new FGItemInfoView(itemInfoOneType, itemInfoOneCount)

    let itemInfoViewTwo =
        new FGItemInfoView(itemInfoTwoType, itemInfoTwoCount)

    let actionButton = new FGButton(backgroundColor, text)

    do
        self.View.Layer.CornerRadius <- nfloat 18.
        self.View.BackgroundColor <- UIColor.SecondarySystemBackgroundColor

        self.View.AddSubview stackView
        self.View.AddSubview actionButton

        stackView.TranslatesAutoresizingMaskIntoConstraints <- false

        stackView.AddArrangedSubview itemInfoViewOne
        stackView.AddArrangedSubview itemInfoViewTwo
        stackView.Axis <- UILayoutConstraintAxis.Horizontal
        stackView.Distribution <- UIStackViewDistribution.EqualSpacing

        NSLayoutConstraint.ActivateConstraints
            ([| stackView.TopAnchor.ConstraintEqualTo(self.View.TopAnchor, padding)
                stackView.LeadingAnchor.ConstraintEqualTo(self.View.LeadingAnchor, padding)
                stackView.TrailingAnchor.ConstraintEqualTo(self.View.TrailingAnchor, -padding)
                stackView.HeightAnchor.ConstraintEqualTo(nfloat 50.)

                actionButton.BottomAnchor.ConstraintEqualTo(self.View.BottomAnchor, -padding)
                actionButton.LeadingAnchor.ConstraintEqualTo(self.View.LeadingAnchor, padding)
                actionButton.TrailingAnchor.ConstraintEqualTo(self.View.TrailingAnchor, -padding)
                actionButton.HeightAnchor.ConstraintEqualTo(nfloat 44.) |])

    member __.ActionButtonClicked(handler) =
        actionButton.TouchUpInside |> Event.add handler
