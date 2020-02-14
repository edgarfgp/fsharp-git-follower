namespace GitFollowers.Controllers

open System
open GitFollowers.Models
open UIKit

type UserInfoController(follower: Follower) as self =
    inherit UIViewController()

    let padding = nfloat 20.
    let scrollView = new UIScrollView(BackgroundColor = UIColor.SystemBackgroundColor)
    let contentView = new UIView()
    let headerView = new UIView()
    let itemViewOne = new UIView()
    let itemViewTwo = new UIView()

    let configureScrollView () =
        self.View.AddSubview scrollView
        scrollView.AddSubview contentView

        scrollView.TranslatesAutoresizingMaskIntoConstraints <- false
        contentView.TranslatesAutoresizingMaskIntoConstraints <- false

        NSLayoutConstraint.ActivateConstraints([|
            scrollView.TopAnchor.ConstraintEqualTo(self.View.TopAnchor);
            scrollView.LeadingAnchor.ConstraintEqualTo(self.View.LeadingAnchor);
            scrollView.TrailingAnchor.ConstraintEqualTo(self.View.TrailingAnchor)
            scrollView.BottomAnchor.ConstraintEqualTo(self.View.BottomAnchor)
        |])

        NSLayoutConstraint.ActivateConstraints([|
            contentView.TopAnchor.ConstraintEqualTo(scrollView.TopAnchor)
            contentView.LeadingAnchor.ConstraintEqualTo(scrollView.LeadingAnchor)
            contentView.TrailingAnchor.ConstraintEqualTo(scrollView.TrailingAnchor)
            contentView.BottomAnchor.ConstraintEqualTo(scrollView.BottomAnchor)
        |])

        NSLayoutConstraint.ActivateConstraints([|
            contentView.WidthAnchor.ConstraintEqualTo(scrollView.WidthAnchor)
        |])

    let configureContentView () =

        headerView.TranslatesAutoresizingMaskIntoConstraints <- false
        itemViewOne.TranslatesAutoresizingMaskIntoConstraints <- false
        itemViewTwo.TranslatesAutoresizingMaskIntoConstraints <- false

        contentView.AddSubviews headerView
        contentView.AddSubviews itemViewOne
        contentView.AddSubviews itemViewTwo

        headerView.BackgroundColor <- UIColor.Yellow
        itemViewOne.BackgroundColor <- UIColor.Blue
        itemViewTwo.BackgroundColor <- UIColor.Red

        NSLayoutConstraint.ActivateConstraints([|

            headerView.TopAnchor.ConstraintEqualTo(contentView.SafeAreaLayoutGuide.TopAnchor, padding)
            headerView.LeadingAnchor.ConstraintEqualTo(contentView.LeadingAnchor, padding)
            headerView.TrailingAnchor.ConstraintEqualTo(contentView.TrailingAnchor, -padding)
            headerView.HeightAnchor.ConstraintEqualTo(nfloat 300.)

            itemViewOne.TopAnchor.ConstraintEqualTo(headerView.BottomAnchor, padding)
            itemViewOne.LeadingAnchor.ConstraintEqualTo(contentView.LeadingAnchor, padding)
            itemViewOne.TrailingAnchor.ConstraintEqualTo(contentView.TrailingAnchor, -padding)
            itemViewOne.HeightAnchor.ConstraintEqualTo(nfloat 300.)

            itemViewTwo.TopAnchor.ConstraintEqualTo(itemViewOne.BottomAnchor, padding)
            itemViewTwo.LeadingAnchor.ConstraintEqualTo(contentView.LeadingAnchor, padding)
            itemViewTwo.TrailingAnchor.ConstraintEqualTo(contentView.TrailingAnchor, -padding)
            itemViewTwo.HeightAnchor.ConstraintEqualTo(nfloat 300.)
            itemViewTwo.BottomAnchor.ConstraintEqualTo(contentView.BottomAnchor)
        |])

    let addChildViewController(childVC: UIViewController,containerView: UIView) =
        self.AddChildViewController childVC
        containerView.AddSubview(childVC.View)
        childVC.View.Frame <- containerView.Bounds
        childVC.DidMoveToParentViewController(self)

    let configureViewController () =
        let doneButton = new UIBarButtonItem(UIBarButtonSystemItem.Done)
        doneButton.Clicked.Add(fun _ -> self.DismissModalViewController(true))
        self.NavigationItem.RightBarButtonItem <- doneButton

    override v.ViewDidLoad() =
        base.ViewDidLoad()
        configureViewController()
        configureScrollView()
        configureContentView()




