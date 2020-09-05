namespace GitFollowers

open System
open UIKit


type ItemInfoType =
| Repo
| Gists
| Followers
| Following

[<AutoOpen>]
type FGItemInfoView(itemInfoType: ItemInfoType, withCount: int) as self =
    inherit UIView()

    let symbolImageView = new UIImageView()

    let titleLabel =
        new FGTitleLabel(UITextAlignment.Left, nfloat 14.)

    let countLabel =
        new FGTitleLabel(UITextAlignment.Left, nfloat 14.)

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
            symbolImageView.Image <- UIImage.GetSystemImage(ImageNames.folder)
            titleLabel.Text <- "Public Repos"
        | Gists ->
            symbolImageView.Image <- UIImage.GetSystemImage(ImageNames.textAlignLeft)
            titleLabel.Text <- "Public Gists"
        | Followers ->
            symbolImageView.Image <- UIImage.GetSystemImage(ImageNames.heart)
            titleLabel.Text <- "Followers"
        | Following ->
            symbolImageView.Image <- UIImage.GetSystemImage(ImageNames.person2)
            titleLabel.Text <- "Following"

        countLabel.Text <- withCount.ToString()

        NSLayoutConstraint.ActivateConstraints
            ([| symbolImageView.TopAnchor.ConstraintEqualTo(self.TopAnchor)
                symbolImageView.LeadingAnchor.ConstraintEqualTo(self.LeadingAnchor)
                symbolImageView.WidthAnchor.ConstraintEqualTo(nfloat 20.)
                symbolImageView.HeightAnchor.ConstraintEqualTo(nfloat 20.)

                titleLabel.CenterYAnchor.ConstraintEqualTo(symbolImageView.CenterYAnchor)
                titleLabel.LeadingAnchor.ConstraintEqualTo(symbolImageView.TrailingAnchor, nfloat 12.)
                titleLabel.TrailingAnchor.ConstraintEqualTo(self.TrailingAnchor)
                titleLabel.HeightAnchor.ConstraintEqualTo(nfloat 18.)

                countLabel.TopAnchor.ConstraintEqualTo(symbolImageView.BottomAnchor, nfloat 4.)
                countLabel.CenterXAnchor.ConstraintEqualTo(self.CenterXAnchor)
                countLabel.HeightAnchor.ConstraintEqualTo(nfloat 18.) |])


type FGEmptyView(message: string) as self =
    inherit UIView()

    let messageLabel =
        new FGTitleLabel(UITextAlignment.Center, nfloat 28.)

    let logoImageView =
        new UIImageView(UIImage.FromBundle(ImageNames.emptyStateLogo))

    do
        messageLabel.TranslatesAutoresizingMaskIntoConstraints <- false
        self.AddSubview(messageLabel)
        self.AddSubview(logoImageView)
        messageLabel.Text <- message
        messageLabel.Lines <- nint 3
        messageLabel.TextColor <- UIColor.SecondaryLabelColor
        logoImageView.TranslatesAutoresizingMaskIntoConstraints <- false

        NSLayoutConstraint.ActivateConstraints
            ([| messageLabel.CenterYAnchor.ConstraintEqualTo(self.CenterYAnchor, nfloat -150.0)
                messageLabel.LeadingAnchor.ConstraintEqualTo(self.LeadingAnchor, nfloat 40.)
                messageLabel.TrailingAnchor.ConstraintEqualTo(self.TrailingAnchor, nfloat -40.0)
                messageLabel.HeightAnchor.ConstraintEqualTo(nfloat 200.)

                logoImageView.WidthAnchor.ConstraintEqualTo(self.WidthAnchor, multiplier = nfloat 1.3)
                logoImageView.HeightAnchor.ConstraintEqualTo(self.WidthAnchor, multiplier = nfloat 1.3)
                logoImageView.TrailingAnchor.ConstraintEqualTo(self.TrailingAnchor, constant = nfloat 170.)
                logoImageView.BottomAnchor.ConstraintEqualTo(self.BottomAnchor, constant = nfloat 40.) |])

type LoadingView private () as view =
    inherit UIView()

    let activityIndicator =
        new UIActivityIndicatorView(UIActivityIndicatorViewStyle.Large)

    do
        view.BackgroundColor <- UIColor.SystemBackgroundColor
        view.Alpha <- nfloat 0.
        UIView.Animate(float 0.25, (fun _ -> view.Alpha <- nfloat 0.8))
        view.AddSubview activityIndicator
        activityIndicator.TranslatesAutoresizingMaskIntoConstraints <- false
        NSLayoutConstraint.ActivateConstraints
            ([| activityIndicator.CenterXAnchor.ConstraintEqualTo(view.CenterXAnchor)
                activityIndicator.CenterYAnchor.ConstraintEqualTo(view.CenterYAnchor) |])

        activityIndicator.StartAnimating()

    member __.Show(parentView: UIView) =
        view.Frame <- parentView.Frame
        view.TranslatesAutoresizingMaskIntoConstraints <- false
        parentView.AddSubview view
        NSLayoutConstraint.ActivateConstraints
            ([| view.TopAnchor.ConstraintEqualTo(parentView.TopAnchor)
                view.LeadingAnchor.ConstraintEqualTo(parentView.LeadingAnchor)
                view.TrailingAnchor.ConstraintEqualTo(parentView.TrailingAnchor)
                view.BottomAnchor.ConstraintEqualTo(parentView.BottomAnchor) |])

    member __.Dismiss() = view.RemoveFromSuperview()

    static member Instance = new LoadingView()
