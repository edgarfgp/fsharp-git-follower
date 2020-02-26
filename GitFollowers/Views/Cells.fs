namespace GitFollowers.Views

open CoreFoundation
open Foundation
open GitFollowers.Models
open System
open UIKit

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

        member __.Follower
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

