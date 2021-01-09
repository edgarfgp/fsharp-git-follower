namespace GitFollowers

open System
open Foundation
open GitFollowers
open SafariServices
open UIKit

type UserInfoController(user: User) as self =
    inherit UIViewController()

    let padding = nfloat 20.
    let contentView = new UIView()
    let headerView = new UIView()
    let itemViewOne = new UIView()
    let itemViewTwo = new UIView()
    let didRequestFollowers = Event<_>()

    let performDidRequestFollowers = fun _ ->
        if user.followers > 0 then
            didRequestFollowers.Trigger(self, user.login)
            self.DismissViewController(true, null)
        else
            presentFGAlertOnMainThread "Not followers" "This user does not have followers" self
    let performUserProfile = fun _ ->
        let safariVC =
            new SFSafariViewController(url = new NSUrl(user.html_url))

        safariVC.PreferredControlTintColor <- UIColor.SystemGreenColor
        self.PresentViewController(safariVC, true, null)

    [<CLIEvent>]
    member __.DidRequestFollowers = didRequestFollowers.Publish

    override __.ViewDidLoad() =
        base.ViewDidLoad()
        self.ConfigureElements user
        self.ConfigureViewController()
        self.ConfigureScrollView()
        self.ConfigureContentView()

    member private __.AddChildViewController(childVC: UIViewController, containerView: UIView) =
        self.AddChildViewController childVC
        containerView.AddSubview(childVC.View)
        childVC.View.Frame <- containerView.Bounds
        childVC.DidMoveToParentViewController(self)

    member private __.ConfigureViewController() =
        addRightNavigationItem self.NavigationItem UIBarButtonSystemItem.Done (fun _ -> self.DismissModalViewController(true))

    member private __.ConfigureContentView() =
        headerView.TranslatesAutoresizingMaskIntoConstraints <- false
        itemViewOne.TranslatesAutoresizingMaskIntoConstraints <- false
        itemViewTwo.TranslatesAutoresizingMaskIntoConstraints <- false

        contentView.AddSubviews headerView
        contentView.AddSubviews itemViewOne
        contentView.AddSubviews itemViewTwo

        NSLayoutConstraint.ActivateConstraints
            ([| headerView.TopAnchor.ConstraintEqualTo(contentView.SafeAreaLayoutGuide.TopAnchor, padding)
                headerView.LeadingAnchor.ConstraintEqualTo(contentView.LeadingAnchor, padding)
                headerView.TrailingAnchor.ConstraintEqualTo(contentView.TrailingAnchor, -padding)
                headerView.HeightAnchor.ConstraintEqualTo(nfloat 210.)

                itemViewOne.TopAnchor.ConstraintEqualTo(headerView.BottomAnchor, padding)
                itemViewOne.LeadingAnchor.ConstraintEqualTo(contentView.LeadingAnchor, padding)
                itemViewOne.TrailingAnchor.ConstraintEqualTo(contentView.TrailingAnchor, -padding)
                itemViewOne.HeightAnchor.ConstraintEqualTo(nfloat 140.)

                itemViewTwo.TopAnchor.ConstraintEqualTo(itemViewOne.BottomAnchor, padding)
                itemViewTwo.LeadingAnchor.ConstraintEqualTo(contentView.LeadingAnchor, padding)
                itemViewTwo.TrailingAnchor.ConstraintEqualTo(contentView.TrailingAnchor, -padding)
                itemViewTwo.HeightAnchor.ConstraintEqualTo(nfloat 140.)
                itemViewTwo.BottomAnchor.ConstraintEqualTo(contentView.BottomAnchor) |])

    member private __.ConfigureScrollView() =
        let scrollView =
            new UIScrollView(BackgroundColor = UIColor.SystemBackgroundColor)

        self.View.AddSubview scrollView
        scrollView.AddSubview contentView

        scrollView.TranslatesAutoresizingMaskIntoConstraints <- false
        contentView.TranslatesAutoresizingMaskIntoConstraints <- false

        NSLayoutConstraint.ActivateConstraints
            ([| scrollView.TopAnchor.ConstraintEqualTo(self.View.TopAnchor)
                scrollView.LeadingAnchor.ConstraintEqualTo(self.View.LeadingAnchor)
                scrollView.TrailingAnchor.ConstraintEqualTo(self.View.TrailingAnchor)
                scrollView.BottomAnchor.ConstraintEqualTo(self.View.BottomAnchor) |])

        NSLayoutConstraint.ActivateConstraints
            ([| contentView.TopAnchor.ConstraintEqualTo(scrollView.TopAnchor)
                contentView.LeadingAnchor.ConstraintEqualTo(scrollView.LeadingAnchor)
                contentView.TrailingAnchor.ConstraintEqualTo(scrollView.TrailingAnchor)
                contentView.BottomAnchor.ConstraintEqualTo(scrollView.BottomAnchor) |])

        NSLayoutConstraint.ActivateConstraints
            ([| contentView.WidthAnchor.ConstraintEqualTo(scrollView.WidthAnchor) |])

    member private __.ConfigureElements user =
        let itemInfoOne =
            new ItemInfoVC(UIColor.SystemPurpleColor,
                           "Github Profile",
                           ItemInfoType.Repo,
                           user.public_repos,
                           ItemInfoType.Gists,
                           user.public_gists)

        let itemInfoTwo =
            new ItemInfoVC(UIColor.SystemGreenColor, "Get Followers",
                           ItemInfoType.Followers,
                           user.followers,
                           ItemInfoType.Following,
                           user.following)

        self.AddChildViewController(new FGUserInfoHeaderVC(user), headerView)
        self.AddChildViewController(itemInfoOne, itemViewOne)
        self.AddChildViewController(itemInfoTwo, itemViewTwo)
        itemInfoOne.ActionButtonClicked(performUserProfile)
        itemInfoTwo.ActionButtonClicked(performDidRequestFollowers)
