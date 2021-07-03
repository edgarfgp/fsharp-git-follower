namespace GitFollowers.Elements

open System
open GitFollowers
open GitFollowers.DTOs
open GitFollowers.Entities
open UIKit
type CurrencyCell(handle: IntPtr) as self =
    inherit UITableViewCell(handle)
    
    let avatarImageView = new FGAvatarImageView()
    let labelTitle = new FGTitleLabel(UITextAlignment.Left, nfloat 14.)
    let labelBody = new FGBodyLabel()
    
    let containerView = new UIStackView()
    do
        self.AddSubviewsX containerView
                
        containerView.Axis <- UILayoutConstraintAxis.Horizontal
        containerView.Distribution <- UIStackViewDistribution.FillProportionally
        containerView.Alignment <- UIStackViewAlignment.Center
        containerView.Spacing <- nfloat 8.

        containerView.ConstraintToParent(self, nfloat 8.)
        avatarImageView.TranslatesAutoresizingMaskIntoConstraints <- false
        labelTitle.TranslatesAutoresizingMaskIntoConstraints <- false
        labelBody.TranslatesAutoresizingMaskIntoConstraints <- false
        
        labelBody.TextAlignment <- UITextAlignment.Left
        
        containerView.AddArrangedSubview(avatarImageView)
        containerView.AddArrangedSubview(labelTitle)
        containerView.AddArrangedSubview(labelBody)
        
        avatarImageView.WidthAnchor.ConstraintEqualTo(nfloat 40.).Active <- true
        avatarImageView.HeightAnchor.ConstraintEqualTo(nfloat 40.).Active <- true
        
        avatarImageView.Image <- UIImage.GetSystemImage(ImageNames.currencies)
        avatarImageView.ContentMode <- UIViewContentMode.ScaleAspectFit
        
    member _.SetUp(country: CurrencyData) =
        labelTitle.Text <- country.code
        labelBody.Text <- country.name

    static member val CellId = "CurrencyCell"
        

type ExchangeCell(handle: IntPtr) as self =
    inherit UICollectionViewCell(handle)

    let overallContainer = new UIStackView()
    let firstCurrencyContainer = new UIStackView()
    let secondCurrencyContainer = new UIStackView()
    
    let firstCurrencyValue = new UILabel()
    let firstCurrencyName  = new UILabel()
    
    let secondCurrencyValue = new UILabel()
    let secondCurrencyName  = new UILabel()

    do
        self.AddSubviewsX(overallContainer)
        overallContainer.ConstraintToParent self.ContentView
        
        firstCurrencyContainer.TranslatesAutoresizingMaskIntoConstraints <- false
        overallContainer.AddArrangedSubview firstCurrencyContainer
        overallContainer.AddArrangedSubview secondCurrencyContainer
        overallContainer.Axis <- UILayoutConstraintAxis.Horizontal
        overallContainer.Alignment <- UIStackViewAlignment.Center
        overallContainer.Distribution <- UIStackViewDistribution.FillProportionally
       
        firstCurrencyValue.TranslatesAutoresizingMaskIntoConstraints <- false
        firstCurrencyName.TranslatesAutoresizingMaskIntoConstraints <- false
        secondCurrencyContainer.TranslatesAutoresizingMaskIntoConstraints <- false
        secondCurrencyValue.TranslatesAutoresizingMaskIntoConstraints <- false
        secondCurrencyName.TranslatesAutoresizingMaskIntoConstraints <- false
        
        firstCurrencyContainer.Distribution <- UIStackViewDistribution.Fill
        firstCurrencyContainer.Axis <- UILayoutConstraintAxis.Vertical
        firstCurrencyContainer.Alignment <- UIStackViewAlignment.Leading
        firstCurrencyContainer.AddArrangedSubview firstCurrencyValue
        firstCurrencyContainer.AddArrangedSubview firstCurrencyName
        
        secondCurrencyContainer.Distribution <- UIStackViewDistribution.Fill
        secondCurrencyContainer.Axis <- UILayoutConstraintAxis.Vertical
        secondCurrencyContainer.Alignment <- UIStackViewAlignment.Trailing
        secondCurrencyContainer.AddArrangedSubview secondCurrencyValue
        secondCurrencyContainer.AddArrangedSubview secondCurrencyName
        
        firstCurrencyValue.Lines <- nint 0
        firstCurrencyName.Lines <- nint 0
        secondCurrencyValue.Lines <- nint 0
        secondCurrencyName.Lines <- nint 0
        
        firstCurrencyValue.Font <- UIFont.BoldSystemFontOfSize(nfloat 18.)
        secondCurrencyValue.Font <- UIFont.BoldSystemFontOfSize(nfloat 18.)
        firstCurrencyName.Font <- UIFont.PreferredBody
        secondCurrencyName.Font <- UIFont.PreferredBody
        
    member self.SetUp(exchange: Selection) =
        firstCurrencyValue.Text <- $"{Math.Round(exchange.first.value, 3)} {exchange.first.code}"
        firstCurrencyName.Text <- exchange.first.name
        
        secondCurrencyValue.Text <- $"{Math.Round(exchange.second.value, 3)} {exchange.second.code}"
        secondCurrencyName.Text <- exchange.second.name
    static member val CellId = "ExchangeCell"

type FollowerCell(handle: IntPtr) as self =
    inherit UICollectionViewCell(handle)
    let padding = nfloat 8.
    let avatarImageView = new FGAvatarImageView()

    let userNameLabel =
        new FGTitleLabel(UITextAlignment.Center, nfloat 16.)

    do
        self.AddSubviewsX(avatarImageView , userNameLabel)

        NSLayoutConstraint.ActivateConstraints
            [| avatarImageView.TopAnchor.ConstraintEqualTo(self.ContentView.TopAnchor, padding)
               avatarImageView.LeadingAnchor.ConstraintEqualTo(self.ContentView.LeadingAnchor, padding)
               avatarImageView.TrailingAnchor.ConstraintEqualTo(self.ContentView.TrailingAnchor, -padding)
               avatarImageView.HeightAnchor.ConstraintEqualTo(avatarImageView.WidthAnchor) |]

        NSLayoutConstraint.ActivateConstraints
            [| userNameLabel.TopAnchor.ConstraintEqualTo(avatarImageView.BottomAnchor,nfloat 12.)
               userNameLabel.LeadingAnchor.ConstraintEqualTo(self.ContentView.LeadingAnchor, padding)
               userNameLabel.TrailingAnchor.ConstraintEqualTo(self.ContentView.TrailingAnchor,-padding)
               userNameLabel.HeightAnchor.ConstraintEqualTo(nfloat 20.) |]

    static member val CellId = "FollowerCell"

    member _.SetUp(follower: Follower) =
        userNameLabel.Text <- follower.Login
        async {
            mainThread{
                userNameLabel.Text <- follower.Login
                avatarImageView
                    .LoadImageFromUrl(follower.AvatarUrl)
                    |> ignore
            }
        }
        |> Async.Start

type FavoriteCell(handle: IntPtr) as self =
    inherit UITableViewCell(handle)

    let padding = nfloat 12.

    let avatarImageView = new FGAvatarImageView()

    let userNameLabel =
        new FGTitleLabel(UITextAlignment.Left, nfloat 16.)

    do
        self.Accessory <- UITableViewCellAccessory.DisclosureIndicator
        self.AddSubview avatarImageView
        self.AddSubview userNameLabel

        NSLayoutConstraint.ActivateConstraints
            [| avatarImageView.CenterYAnchor.ConstraintEqualTo(self.CenterYAnchor)
               avatarImageView.LeadingAnchor.ConstraintEqualTo(self.LeadingAnchor, padding)
               avatarImageView.HeightAnchor.ConstraintEqualTo(constant = nfloat 60.)
               avatarImageView.WidthAnchor.ConstraintEqualTo(constant = nfloat 60.) |]

        NSLayoutConstraint.ActivateConstraints
            [| userNameLabel.CenterYAnchor.ConstraintEqualTo(self.CenterYAnchor)
               userNameLabel.LeadingAnchor.ConstraintEqualTo(avatarImageView.TrailingAnchor, padding * nfloat 2.)
               userNameLabel.TrailingAnchor.ConstraintEqualTo(self.ContentView.TrailingAnchor, -padding)
               userNameLabel.HeightAnchor.ConstraintEqualTo(nfloat 40.) |]

    static member val CellId = "FollowerTableCell"

    member _.SetUp(follower: DTOs.Follower) =
        async {
            mainThread {
                userNameLabel.Text <- follower.login
                avatarImageView.LoadImageFromUrl(follower.avatar_url).AsTask()
                    |> Async.AwaitTask
                    |> ignore
            }
        }
        |> Async.Start
