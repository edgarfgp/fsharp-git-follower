namespace GitFollowers

open System
open System.Reactive.Disposables
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
    
    let disposables = new CompositeDisposable()

    let itemInfoOne =
        new ItemInfoVC(
            UIColor.SystemPurpleColor,
            "Github Profile",
            ItemInfoType.Repo,
            user.public_repos,
            ItemInfoType.Gists,
            user.public_gists)

    let itemInfoTwo =
        new ItemInfoVC(
            UIColor.SystemGreenColor,
            "Get Followers",
            ItemInfoType.Followers,
            user.followers,
            ItemInfoType.Following,
            user.following)

    let performDidRequestFollowers() =
        if user.followers > 0 then
            didRequestFollowers.Trigger(self, user.login)
            self.DismissViewController(true, null)
        else
            self.PresentAlert "Not followers" "This user does not have followers"

    let performUserProfile() =
        let safariVC =
            new SFSafariViewController(url = new NSUrl(user.html_url))
        safariVC.PreferredControlTintColor <- UIColor.SystemGreenColor
        self.PresentViewController(safariVC, true, null)

    let addChildViewController (childVC: UIViewController, containerView: UIView) =
        self.AddChildViewController childVC
        containerView.AddSubview(childVC.View)
        childVC.View.Frame <- containerView.Bounds
        childVC.DidMoveToParentViewController(self)

    let configureViewController() =
        self.AddRightNavigationItem UIBarButtonSystemItem.Done
        |> Observable.subscribe(fun _ -> self.DismissModalViewController(true))
        |> disposables.Add

    let configureContentView() =
        contentView.AddSubviewsX(headerView , itemViewOne ,itemViewTwo)

        NSLayoutConstraint.ActivateConstraints
            [| headerView.TopAnchor.ConstraintEqualTo(contentView.SafeAreaLayoutGuide.TopAnchor, padding)
               headerView.LeadingAnchor.ConstraintEqualTo(contentView.LeadingAnchor, padding)
               headerView.TrailingAnchor.ConstraintEqualTo(contentView.TrailingAnchor, -padding)
               headerView.HeightAnchor.ConstraintEqualTo(nfloat 210.)

               itemViewOne.TopAnchor.ConstraintEqualTo(headerView.BottomAnchor, padding)
               itemViewOne.LeadingAnchor.ConstraintEqualTo(contentView.LeadingAnchor, padding)
               itemViewOne.TrailingAnchor.ConstraintEqualTo(contentView.TrailingAnchor, -padding)
               itemViewOne.HeightAnchor.ConstraintEqualTo(nfloat 140.)

               itemViewTwo.TopAnchor.ConstraintEqualTo(itemViewOne.BottomAnchor, padding)
               itemViewTwo.LeadingAnchor.ConstraintEqualTo(contentView.LeadingAnchor,padding)
               itemViewTwo.TrailingAnchor.ConstraintEqualTo(contentView.TrailingAnchor, -padding)
               itemViewTwo.HeightAnchor.ConstraintEqualTo(nfloat 140.)
               itemViewTwo.BottomAnchor.ConstraintEqualTo(contentView.BottomAnchor) |]

    let configureScrollView() =
        let scrollView =
            new UIScrollView(BackgroundColor = UIColor.SystemBackgroundColor)

        self.View.AddSubview scrollView
        scrollView.AddSubview contentView

        scrollView.TranslatesAutoresizingMaskIntoConstraints <- false
        contentView.TranslatesAutoresizingMaskIntoConstraints <- false
        scrollView.ConstraintToParent(self.View)
        contentView.ConstraintToParent(scrollView)

        contentView.WidthAnchor.ConstraintEqualTo(scrollView.WidthAnchor).Active <- true

    let configureElements user =
        addChildViewController (new FGUserInfoHeaderVC(user), headerView)
        addChildViewController (itemInfoOne, itemViewOne)
        addChildViewController (itemInfoTwo, itemViewTwo)

    [<CLIEvent>]
    member _.DidRequestFollowers = didRequestFollowers.Publish

    override _.ViewDidLoad() =
        base.ViewDidLoad()

        configureElements user
        configureViewController()
        configureScrollView()
        configureContentView()
        
        itemInfoOne.ActionButtonClicked
        |>Observable.subscribe(fun _ -> performUserProfile())
        |> disposables.Add
        
        itemInfoTwo.ActionButtonClicked
        |>Observable.subscribe(fun _ -> performDidRequestFollowers())
        |> disposables.Add
        
    override self.Dispose _ =
        disposables.Dispose()
