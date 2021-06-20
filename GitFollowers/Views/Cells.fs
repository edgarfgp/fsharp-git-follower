namespace GitFollowers

open System
open UIKit

type FollowerCell(handle: IntPtr) as self =
    inherit UICollectionViewCell(handle)
    let padding = nfloat 8.
    let avatarImageView = new FGAvatarImageView()

    let userNameLabel =
        new FGTitleLabel(UITextAlignment.Center, nfloat 16.)

    do
        self.AddSubview avatarImageView
        self.AddSubview userNameLabel

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

    member _.SetUp(follower: FollowerObject) =
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

    member _.SetUp(follower: Follower) =
        async {
            mainThread {
                userNameLabel.Text <- follower.login
                avatarImageView.LoadImageFromUrl(follower.avatar_url).AsTask()
                    |> Async.AwaitTask
                    |> ignore
            }
        }
        |> Async.Start
