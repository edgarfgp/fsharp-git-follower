namespace GitFollowers.Elements

open System
open UIKit

type FGButton(backgroundColor: UIColor, text: string) as self =
    inherit UIButton()

    do
        self.BackgroundColor <- backgroundColor
        self.TranslatesAutoresizingMaskIntoConstraints <- false
        self.SetTitle(text, UIControlState.Normal)
        self.Layer.CornerRadius <- nfloat 10.
        self.SetTitleColor(UIColor.White, UIControlState.Normal)
        self.TitleLabel.Font <- UIFont.PreferredHeadline
