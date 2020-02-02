namespace GitFollowers

open System
open UIKit


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

