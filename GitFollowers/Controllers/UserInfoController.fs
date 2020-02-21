namespace GitFollowers.Controllers

open Foundation
open GitFollowers
open System
open GitFollowers.Models
open GitFollowers.Views
open GitFollowers.Views.ViewControllers
open SafariServices
open UIKit

type UserInfoController(userName : string) as self =
    inherit UIViewController()

    let padding = nfloat 20.
    let scrollView = new UIScrollView(BackgroundColor = UIColor.SystemBackgroundColor)
    let contentView = new UIView()
    let headerView = new UIView()
    let itemViewOne = new UIView()
    let itemViewTwo = new UIView()

    let addChildViewController(childVC: UIViewController,containerView: UIView) =
        self.AddChildViewController childVC
        containerView.AddSubview(childVC.View)
        childVC.View.Frame <- containerView.Bounds
        childVC.DidMoveToParentViewController(self)

    let configureElements user =
        let itemInfoOne =
            new ItemInfoVC(UIColor.SystemPurpleColor, "Github Profile", ItemInfoType.Repo, user.public_repos, ItemInfoType.Gists, user.public_gists)
        let itemInfoTwo =
            new ItemInfoVC(UIColor.SystemGreenColor, "Get Followers", ItemInfoType.Followers, user.followers, ItemInfoType.Following, user.following)
        addChildViewController(new FGUserInfoHeaderVC(user), headerView)
        addChildViewController(itemInfoOne, itemViewOne)
        addChildViewController(itemInfoTwo, itemViewTwo)

        itemInfoOne.ActionButtonClicked(fun _ ->
            let safariVC = new SFSafariViewController(url = new NSUrl(user.html_url))
            safariVC.PreferredControlTintColor <- UIColor.SystemGreenColor
            self.PresentViewController(safariVC, true, null))

        itemInfoTwo.ActionButtonClicked(fun _ ->
            // TODO implement proper navigation to FollowerList
            let searchViewController = new UIViewController()
            searchViewController.View.BackgroundColor <- UIColor.SystemPinkColor
            self.NavigationController.PushViewController(searchViewController, animated = true))

    let getUserInfo  =
        match GitHubService.getUserInfo userName with
        | Ok value ->
            configureElements value
            value
        | Error _-> User.CreateUser()

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

        NSLayoutConstraint.ActivateConstraints([|

            headerView.TopAnchor.ConstraintEqualTo(contentView.SafeAreaLayoutGuide.TopAnchor, padding)
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
            itemViewTwo.BottomAnchor.ConstraintEqualTo(contentView.BottomAnchor)
        |])

    let configureViewController () =
        let doneButton = new UIBarButtonItem(UIBarButtonSystemItem.Done)
        doneButton.Clicked.Add(fun _ -> self.DismissModalViewController(true))
        self.NavigationItem.RightBarButtonItem <- doneButton

    override v.ViewDidLoad() =
        base.ViewDidLoad()
        configureViewController()
        configureScrollView()
        configureContentView()

        getUserInfo |> ignore