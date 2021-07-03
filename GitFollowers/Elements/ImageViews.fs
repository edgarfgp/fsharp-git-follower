namespace GitFollowers.Elements

open System
open GitFollowers
open UIKit

type FGAvatarImageView() as self =
    inherit UIImageView()

    do
        self.Layer.CornerRadius <- nfloat 10.
        self.ClipsToBounds <- true
        self.TranslatesAutoresizingMaskIntoConstraints <- false
