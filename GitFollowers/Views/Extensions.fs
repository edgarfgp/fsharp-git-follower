namespace GitFollowers.Views

open CoreFoundation
open GitFollowers.Views.ViewControllers
open GitFollowers.Views.Views
open UIKit

module Extensions =
    let presentFGAlertOnMainThread(title , message, self: UIViewController) =
            DispatchQueue.MainQueue.DispatchAsync(fun _ ->
                let alertVC =
                    new FGAlertVC(title, message, "Ok")
                alertVC.ModalPresentationStyle <- UIModalPresentationStyle.OverFullScreen
                alertVC.ModalTransitionStyle <- UIModalTransitionStyle.CrossDissolve
                self.PresentViewController(alertVC, true, null))

    let showEmptyView(message: string, self: UIViewController) =
        let emptyView = new FGEmptyView(message)
        emptyView.Frame <- self.View.Bounds
        emptyView.TranslatesAutoresizingMaskIntoConstraints <- false
        self.View.AddSubview emptyView

        NSLayoutConstraint.ActivateConstraints([|
            emptyView.TopAnchor.ConstraintEqualTo(self.View.TopAnchor)
            emptyView.LeadingAnchor.ConstraintEqualTo(self.View.LeadingAnchor)
            emptyView.TrailingAnchor.ConstraintEqualTo(self.View.TrailingAnchor)
            emptyView.BottomAnchor.ConstraintEqualTo(self.View.BottomAnchor)
        |])

