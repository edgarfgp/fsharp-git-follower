namespace GitFollowers.Views

open System
open UIKit

module Labels =

    type FGTitleLabel(textAligment: UITextAlignment, fontSize: nfloat) as self =
        inherit UILabel()
        do
            self.TextAlignment <- textAligment
            self.Font <- UIFont.SystemFontOfSize(fontSize, UIFontWeight.Bold)
            self.TextColor <- UIColor.LabelColor
            self.AdjustsFontSizeToFitWidth <- true
            self.MinimumScaleFactor <- nfloat 0.9
            self.LineBreakMode <- UILineBreakMode.TailTruncation
            self.TranslatesAutoresizingMaskIntoConstraints <- false

    type FGBodyLabel(text: string, textAligment: UITextAlignment, numberOfLines: nint) as self =
        inherit UILabel()

        do
            self.Text <- text
            self.TextAlignment <- textAligment
            self.Lines <- numberOfLines
            self.TextColor <- UIColor.SecondaryLabelColor
            self.Font <- UIFont.PreferredBody
            self.AdjustsFontSizeToFitWidth <- true
            self.MinimumScaleFactor <- nfloat 0.75
            self.LineBreakMode <- UILineBreakMode.WordWrap
            self.TranslatesAutoresizingMaskIntoConstraints <- false

    type FGSecondaryTitleLabel(fontSize: nfloat) as self =
        inherit UILabel()

        do
            self.Font <- UIFont.SystemFontOfSize(fontSize, UIFontWeight.Medium)
            self.TextColor <- UIColor.SecondaryLabelColor
            self.AdjustsFontSizeToFitWidth <- true
            self.MinimumScaleFactor <- nfloat 0.90
            self.LineBreakMode <- UILineBreakMode.TailTruncation
            self.TranslatesAutoresizingMaskIntoConstraints <- false

