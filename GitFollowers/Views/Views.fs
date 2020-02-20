namespace GitFollowers.Views

open GitFollowers.Views.Labels
open System
open UIKit

type ItemInfoType =
    | Repo
    | Gists
    | Followers
    | Following

module Views =

    type FGItemInfoView(itemInfoType : ItemInfoType, withCount : int) as self =
        inherit UIView()

        let symbolImageView = new  UIImageView()
        let titleLabel =new  FGTitleLabel(UITextAlignment.Left, nfloat 14.)
        let countLabel = new  FGTitleLabel(UITextAlignment.Left, nfloat 14.)

        do
            self.AddSubview symbolImageView
            self.AddSubview titleLabel
            self.AddSubview countLabel

            symbolImageView.TranslatesAutoresizingMaskIntoConstraints <- false
            titleLabel.TranslatesAutoresizingMaskIntoConstraints <- false
            countLabel.TranslatesAutoresizingMaskIntoConstraints <- false
            symbolImageView.ContentMode <- UIViewContentMode.ScaleAspectFit
            symbolImageView.TintColor <- UIColor.LabelColor

            match itemInfoType with
            | Repo ->
                symbolImageView.Image <- UIImage.GetSystemImage("folder")
                titleLabel.Text <- "Public Repos"
            | Gists ->
                symbolImageView.Image <- UIImage.GetSystemImage("text.alignleft")
                titleLabel.Text <- "Public Gists"
            | Followers ->
                symbolImageView.Image <- UIImage.GetSystemImage("heart")
                titleLabel.Text <- "Followers"
            | Following ->
                symbolImageView.Image <- UIImage.GetSystemImage("person.2")
                titleLabel.Text <- "Following"

            countLabel.Text <- withCount.ToString()

            NSLayoutConstraint.ActivateConstraints([|
                symbolImageView.TopAnchor.ConstraintEqualTo(self.TopAnchor)
                symbolImageView.LeadingAnchor.ConstraintEqualTo(self.LeadingAnchor)
                symbolImageView.WidthAnchor.ConstraintEqualTo( nfloat 20.)
                symbolImageView.HeightAnchor.ConstraintEqualTo( nfloat 20.)

                titleLabel.CenterYAnchor.ConstraintEqualTo(symbolImageView.CenterYAnchor)
                titleLabel.LeadingAnchor.ConstraintEqualTo(symbolImageView.TrailingAnchor, nfloat 12.)
                titleLabel.TrailingAnchor.ConstraintEqualTo(self.TrailingAnchor)
                titleLabel.HeightAnchor.ConstraintEqualTo(nfloat 18.)

                countLabel.TopAnchor.ConstraintEqualTo(symbolImageView.BottomAnchor, nfloat 4.)
                countLabel.CenterXAnchor.ConstraintEqualTo(self.CenterXAnchor)
                countLabel.HeightAnchor.ConstraintEqualTo( nfloat 18.)
            |])


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

