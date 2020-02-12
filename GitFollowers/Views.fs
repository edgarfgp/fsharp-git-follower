namespace GitFollowers.Views

open CoreFoundation
open Foundation
open GitFollowers.Models
open System
open UIKit

module Labels =
    type FGTitleLabel(textAligment: UITextAlignment, fontSize: nfloat) as self =
        inherit UILabel()
        do
            self.TextAlignment <- textAligment
            self.Font <- UIFont.SystemFontOfSize(fontSize, UIFontWeight.Bold)
            self.TextColor <- UIColor.LabelColor
            self.AdjustsFontSizeToFitWidth <- true
            self.MinimumScaleFactor <- nfloat 0.9
            self.LineBreakMode <- UILineBreakMode.TailTruncation
            self.TranslatesAutoresizingMaskIntoConstraints <- false

    type FGBodyLabel(text: string, textAligment: UITextAlignment, numberOfLines: nint) as self =
        inherit UILabel()

        do
            self.Text <- text
            self.TextAlignment <- textAligment
            self.Lines <- numberOfLines
            self.TextColor <- UIColor.SecondaryLabelColor
            self.Font <- UIFont.PreferredBody
            self.AdjustsFontSizeToFitWidth <- true
            self.MinimumScaleFactor <- nfloat 0.75
            self.LineBreakMode <- UILineBreakMode.WordWrap
            self.TranslatesAutoresizingMaskIntoConstraints <- false


module Buttons =

    type FGButton(backgroundColor: UIColor, text: string) as self =
        inherit UIButton()

        do
            self.BackgroundColor <- backgroundColor
            self.TranslatesAutoresizingMaskIntoConstraints <- false
            self.SetTitle(text, UIControlState.Normal)
            self.Layer.CornerRadius <- nfloat 10.
            self.SetTitleColor(UIColor.White, UIControlState.Normal)
            self.TitleLabel.Font <- UIFont.PreferredHeadline

module TextFields =

    type FGTextField(placeholder: string) as self =
        inherit UITextField()
        do
            self.TranslatesAutoresizingMaskIntoConstraints <- false
            self.Layer.CornerRadius <- nfloat 10.
            self.Layer.BorderWidth <- nfloat 2.
            self.Layer.BorderColor <- UIColor.SystemGray4Color.CGColor

            self.TextColor <- UIColor.LabelColor
            self.TintColor <- UIColor.LabelColor
            self.TextAlignment <- UITextAlignment.Center
            self.Font <- UIFont.PreferredTitle2
            self.AdjustsFontSizeToFitWidth <- true
            self.MinimumFontSize <- nfloat 12.

            self.BackgroundColor <- UIColor.TertiarySystemBackgroundColor
            self.AutocorrectionType <- UITextAutocorrectionType.No

            self.ReturnKeyType <- UIReturnKeyType.Go

            self.Placeholder <- placeholder

module Views =
    open Labels

    type FGEmptyView(message: string) as self =
        inherit UIView()
        let messagelabel = new FGTitleLabel(UITextAlignment.Center, nfloat 28.)
        let logoImageView = new UIImageView(new UIImage("empty-state-logo"))

        do
            self.AddSubview(messagelabel)
            self.AddSubview(logoImageView)
            messagelabel.Text <- message
            messagelabel.Lines <- nint 3
            messagelabel.TextColor <- UIColor.SecondaryLabelColor
            logoImageView.Image <- new UIImage("empty-state-logo")
            logoImageView.TranslatesAutoresizingMaskIntoConstraints <- false

            NSLayoutConstraint.ActivateConstraints
                ([| messagelabel.CenterYAnchor.ConstraintEqualTo(self.CenterYAnchor, nfloat -150.0)
                    messagelabel.LeadingAnchor.ConstraintEqualTo(self.LeadingAnchor, nfloat 40.)
                    messagelabel.TrailingAnchor.ConstraintEqualTo(self.TrailingAnchor, nfloat -40.0)
                    messagelabel.HeightAnchor.ConstraintEqualTo(nfloat 200.)

                    logoImageView.WidthAnchor.ConstraintEqualTo(self.WidthAnchor, multiplier = nfloat 1.3)
                    logoImageView.HeightAnchor.ConstraintEqualTo(self.WidthAnchor, multiplier = nfloat 1.3)
                    logoImageView.TrailingAnchor.ConstraintEqualTo(self.TrailingAnchor, constant = nfloat 170.)
                    logoImageView.BottomAnchor.ConstraintEqualTo(self.BottomAnchor, constant = nfloat 40.) |])

module ViewControllers =
    open Labels
    open Buttons

    type FGAlertVC(title: string, message: string, buttonTitle: string) as self =
        inherit UIViewController()
        let containerView = new UIView()
        let titleLabel = new FGTitleLabel(UITextAlignment.Center, nfloat 20.)
        let messageLabel = new FGBodyLabel(message, UITextAlignment.Center, nint 1)
        let actionButton = new FGButton(UIColor.SystemPinkColor, "Ok")

        let padding = nfloat 20.

        do
            self.View.BackgroundColor <-
                new UIColor(red = nfloat 0., green = nfloat 0., blue = nfloat 0., alpha = nfloat 0.75)

            titleLabel.Text <- title
            self.View.AddSubview containerView
            containerView.AddSubview titleLabel
            containerView.AddSubview actionButton
            containerView.AddSubview messageLabel

            messageLabel.Lines <- nint 4
            actionButton.SetTitle(buttonTitle, UIControlState.Normal)
            actionButton.TouchUpInside.Add(fun _ -> self.DismissViewController(true, null))

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


module ImageViews =

    type FGAvatarImageView() as self =
        inherit UIImageView()

        let placeHolderImage = UIImage.FromBundle("avatar-placeholder")

        do
            self.Layer.CornerRadius <- nfloat 10.
            self.ClipsToBounds <- true
            self.Image <- placeHolderImage
            self.TranslatesAutoresizingMaskIntoConstraints <- false

module Cells =

    open ImageViews
    open Labels

    type FollowerCell(handle: IntPtr) as self =
        inherit UICollectionViewCell(handle)

        let mutable folllower = Follower.CreateFollower()

        let padding = nfloat 8.
        let avatarImageView = new FGAvatarImageView()
        let userNameLabel = new FGTitleLabel(UITextAlignment.Center, nfloat 16.)

        do
            self.AddSubview avatarImageView
            self.AddSubview userNameLabel

            NSLayoutConstraint.ActivateConstraints
                [| avatarImageView.TopAnchor.ConstraintEqualTo(self.ContentView.TopAnchor, padding)
                   avatarImageView.LeadingAnchor.ConstraintEqualTo(self.ContentView.LeadingAnchor, padding)
                   avatarImageView.TrailingAnchor.ConstraintEqualTo(self.ContentView.TrailingAnchor, -padding)
                   avatarImageView.HeightAnchor.ConstraintEqualTo(avatarImageView.WidthAnchor) |]

            NSLayoutConstraint.ActivateConstraints
                [| userNameLabel.TopAnchor.ConstraintEqualTo(avatarImageView.BottomAnchor, nfloat 12.)
                   userNameLabel.LeadingAnchor.ConstraintEqualTo(self.ContentView.LeadingAnchor, padding)
                   userNameLabel.TrailingAnchor.ConstraintEqualTo(self.ContentView.TrailingAnchor, -padding)
                   userNameLabel.HeightAnchor.ConstraintEqualTo(nfloat 20.) |]

        static member val CellId = "FollowerCell"

        member this.Follower
            with get () = folllower
            and set value =
                folllower <- value
                userNameLabel.Text <- folllower.login

                NSUrlSession.SharedSession.CreateDataTask(
                    new NSUrlRequest(new NSUrl(folllower.avatar_url)),
                      NSUrlSessionResponse(fun data response error ->
                          if data <> null then
                              DispatchQueue.MainQueue.DispatchAsync(fun _ ->
                                  let image = UIImage.LoadFromData(data)
                                  avatarImageView.Image <- image))).Resume()
