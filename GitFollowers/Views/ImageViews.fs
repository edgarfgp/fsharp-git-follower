namespace GitFollowers.Views

open System
open UIKit

module ImageViews =

    type FGAvatarImageView() as self =
        inherit UIImageView()

        let placeHolderImage = UIImage.FromBundle("avatar-placeholder")

        do
            self.Layer.CornerRadius <- nfloat 10.
            self.ClipsToBounds <- true
            self.Image <- placeHolderImage
            self.TranslatesAutoresizingMaskIntoConstraints <- false


