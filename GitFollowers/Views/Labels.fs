namespace GitFollowers

open System
open UIKit

type FGTitleLabel(textAlignment: UITextAlignment, fontSize: nfloat) as self =
    inherit UILabel()

    do
        self.TextAlignment <- textAlignment
        self.Font <- UIFont.SystemFontOfSize(fontSize, UIFontWeight.Bold)
        self.TextColor <- UIColor.LabelColor
        self.AdjustsFontSizeToFitWidth <- true
        self.MinimumScaleFactor <- nfloat 0.9
        self.LineBreakMode <- UILineBreakMode.TailTruncation
        self.TranslatesAutoresizingMaskIntoConstraints <- false

type FGBodyLabel() as self =
    inherit UILabel()

    do
        self.TextColor <- UIColor.SecondaryLabelColor
        self.Font <- UIFont.PreferredBody
        self.AdjustsFontSizeToFitWidth <- true
        self.MinimumScaleFactor <- nfloat 0.75
        self.LineBreakMode <- UILineBreakMode.WordWrap
        self.TranslatesAutoresizingMaskIntoConstraints <- false

    override self.Text
        with set (value) = base.Text <- value

    override self.TextAlignment
        with set (value) = base.TextAlignment <- value

    override self.Lines
        with set (value) = base.Lines <- value

type FGSecondaryTitleLabel(fontSize: nfloat) as self =
    inherit UILabel()

    do
        self.Font <- UIFont.SystemFontOfSize(fontSize, UIFontWeight.Medium)
        self.TextColor <- UIColor.SecondaryLabelColor
        self.AdjustsFontSizeToFitWidth <- true
        self.MinimumScaleFactor <- nfloat 0.90
        self.LineBreakMode <- UILineBreakMode.TailTruncation
        self.TranslatesAutoresizingMaskIntoConstraints <- false
