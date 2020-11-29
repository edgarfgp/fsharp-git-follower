namespace GitFollowers

open System
open CoreFoundation
open CoreGraphics
open UIKit

[<AutoOpen>]
module UIViewController =
    let addRightNavigationItem (navigationItem: UINavigationItem)  systemItem  action =
        navigationItem.RightBarButtonItem <- new UIBarButtonItem(systemItem = systemItem)
        navigationItem.RightBarButtonItem.Clicked
            |> Event.add action
            
    let presentFGAlertOnMainThread title message (self: UIViewController) =
        DispatchQueue.MainQueue.DispatchAsync(fun _ ->
            let alertVC = new FGAlertVC(title, message, "Ok")
            alertVC.ModalPresentationStyle <- UIModalPresentationStyle.OverFullScreen
            alertVC.ModalTransitionStyle <- UIModalTransitionStyle.CrossDissolve
            alertVC.ActionButtonClicked(fun _ -> self.DismissViewController(true, null))
            self.PresentViewController(alertVC, true, null))

//    let showEmptyView message (self: UIView) =
//        let emptyView = new FGEmptyView(message)
//        emptyView.Frame <- self.Bounds
//        self.AddSubview emptyView
//
//        NSLayoutConstraint.ActivateConstraints
//            ([| emptyView.TopAnchor.ConstraintEqualTo(self.TopAnchor)
//                emptyView.LeadingAnchor.ConstraintEqualTo(self.LeadingAnchor)
//                emptyView.TrailingAnchor.ConstraintEqualTo(self.TrailingAnchor)
//                emptyView.BottomAnchor.ConstraintEqualTo(self.BottomAnchor) |])

[<AutoOpen>]
module UICollectionView =

    let CreateThreeColumnFlowLayout (view: UIView) =
        let width = view.Bounds.Width
        let padding = nfloat 12.
        let minimumItemSpacing = nfloat 10.

        let availableWidth =
            width
            - (padding * nfloat 2.)
            - (minimumItemSpacing * nfloat 2.)

        let itemWidth = availableWidth / nfloat 3.
        let flowLayout = new UICollectionViewFlowLayout()
        flowLayout.SectionInset <- UIEdgeInsets(padding, padding, padding, padding)
        flowLayout.ItemSize <- CGSize(itemWidth, itemWidth + nfloat 40.)
        flowLayout