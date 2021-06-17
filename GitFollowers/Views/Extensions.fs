namespace GitFollowers

open System
open System.Runtime.CompilerServices
open CoreFoundation
open CoreGraphics
open UIKit

[<AutoOpen>]
module Extensions =
    
    let emptyView = new FGEmptyView()
    let loadingView = new LoadingView()

    type UIViewController with
        member vc.AddRightNavigationItem  systemItem  action =
            vc.NavigationItem.RightBarButtonItem <- new UIBarButtonItem(systemItem = systemItem)
            vc.NavigationItem.RightBarButtonItem.Clicked
                |> Event.add action
               
        member vc.PresentAlert title message =
            DispatchQueue.MainQueue.DispatchAsync(fun _ ->
                let alertVC = new FGAlertVC(title, message, "Ok")
                alertVC.ModalPresentationStyle <- UIModalPresentationStyle.OverFullScreen
                alertVC.ModalTransitionStyle <- UIModalTransitionStyle.CrossDissolve
                alertVC.ActionButtonClicked(fun _ -> vc.DismissViewController(true, null))
                vc.PresentViewController(alertVC, true, null))
            
        member vc.ShowEmptyView(message : string) =
            emptyView.Frame <- vc.View.Bounds
            emptyView.SetMessage(message)
            vc.View.AddSubview emptyView
            NSLayoutConstraint.ActivateConstraints
                [| emptyView.TopAnchor.ConstraintEqualTo(vc.View.TopAnchor)
                   emptyView.LeadingAnchor.ConstraintEqualTo(vc.View.LeadingAnchor)
                   emptyView.TrailingAnchor.ConstraintEqualTo(vc.View.TrailingAnchor)
                   emptyView.BottomAnchor.ConstraintEqualTo(vc.View.BottomAnchor) |]

        member vc.DismissEmptyView() = emptyView.RemoveFromSuperview()
        
        member vc.ShowLoadingView() =
            loadingView.Frame <- vc.View.Frame
            loadingView.TranslatesAutoresizingMaskIntoConstraints <- false
            vc.View.AddSubview loadingView
            NSLayoutConstraint.ActivateConstraints
                ([| loadingView.TopAnchor.ConstraintEqualTo(vc.View.TopAnchor)
                    loadingView.LeadingAnchor.ConstraintEqualTo(vc.View.LeadingAnchor)
                    loadingView.TrailingAnchor.ConstraintEqualTo(vc.View.TrailingAnchor)
                    loadingView.BottomAnchor.ConstraintEqualTo(vc.View.BottomAnchor) |])

        member vc.DismissLoadingView() = loadingView.RemoveFromSuperview()

    type UICollectionView with
        member cv.CreateThreeColumnFlowLayout (view: UIView) =
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