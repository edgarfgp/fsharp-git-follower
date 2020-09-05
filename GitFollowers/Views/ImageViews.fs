namespace GitFollowers

open System
open UIKit


type FGAvatarImageView() as self =
    inherit UIImageView()

    let placeHolderImage =
        UIImage.FromBundle(avatarPlaceHolder)

    do
        self.Layer.CornerRadius <- nfloat 10.
        self.ClipsToBounds <- true
        self.Image <- placeHolderImage
        self.TranslatesAutoresizingMaskIntoConstraints <- false
