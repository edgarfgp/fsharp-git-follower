namespace GitFollowers

open System
open CoreGraphics
open UIKit

[<AutoOpen>]
module Common =
    let emptyView = new FGEmptyView()
    let loadingView = new LoadingView()

    type UIView with
        member uv.AddSubviewsX([<ParamArray>]views:UIView array) =
            views
            |> Array.map(fun view ->
                    view.TranslatesAutoresizingMaskIntoConstraints <- false
                    uv.AddSubview view)
            |> ignore

        member uv.ConstraintToParent(parentView: UIView, ?leading: float, ?top: float,
                                     ?trailing: float, ?bottom: float) =
            let leading = defaultArg leading (float 0)
            let top = defaultArg top (float 0)
            let trailing = defaultArg trailing (float 0)
            let bottom = defaultArg bottom (float 0)

            NSLayoutConstraint.ActivateConstraints(
                [| uv.TopAnchor.ConstraintEqualTo(parentView.TopAnchor, nfloat top)
                   uv.LeadingAnchor.ConstraintEqualTo(parentView.LeadingAnchor, nfloat leading)
                   uv.TrailingAnchor.ConstraintEqualTo(parentView.TrailingAnchor, nfloat trailing)
                   uv.BottomAnchor.ConstraintEqualTo(parentView.BottomAnchor, nfloat bottom) |]
            )


    type UIViewController with
    
        member vc.AddRightNavigationItem systemItem =
            vc.NavigationItem.RightBarButtonItem <- new UIBarButtonItem(systemItem = systemItem)
            vc.NavigationItem.RightBarButtonItem.Clicked

        member vc.PresentAlert title message =
            mainThread {
                let alertVC = new FGAlertVC(title, message, "Ok")
                alertVC.ModalPresentationStyle <- UIModalPresentationStyle.OverFullScreen
                alertVC.ModalTransitionStyle <- UIModalTransitionStyle.CrossDissolve
                alertVC.ActionButtonClicked
                |> Observable.subscribe(fun _ -> vc.DismissViewController(true, null))
                |> ignore
                vc.PresentViewController(alertVC, true, null)
            }

        member vc.ShowEmptyView(message: string) =
            emptyView.Frame <- vc.View.Bounds
            emptyView.SetMessage(message)
            vc.View.AddSubview emptyView
            emptyView.ConstraintToParent(vc.View)

        member vc.DismissEmptyView() = emptyView.RemoveFromSuperview()

        member vc.ShowLoadingView() =
            loadingView.Frame <- vc.View.Frame
            loadingView.TranslatesAutoresizingMaskIntoConstraints <- false
            vc.View.AddSubview loadingView
            loadingView.ConstraintToParent(vc.View)

        member vc.DismissLoadingView() = loadingView.RemoveFromSuperview()

    type UICollectionView with
        member cv.CreateThreeColumnFlowLayout(view: UIView) =
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