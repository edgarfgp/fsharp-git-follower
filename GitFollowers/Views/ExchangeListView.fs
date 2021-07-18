namespace GitFollowers.Views

open System
open System.Reactive.Disposables
open CoreGraphics
open GitFollowers
open GitFollowers.Controllers
open GitFollowers.Elements
open FSharp.Control.Reactive
open GitFollowers.Persistence
open UIKit
open Xamarin.Essentials

type ExchangeListView() as self =
    inherit UIViewController()

    let addImage = new UIImageView()
    let messageLabel = new FGBodyLabel()
    let headerContainerView = new UIStackView()
    let headerView = new UIView()
    let disposables = new CompositeDisposable()

    let controller =
        ExchangesController(ExchangeRepository())

    let observableTimer =
        Observable.interval (TimeSpan.FromSeconds 1.)

    let mutable observableTimerSubscription: IDisposable = null

    let collectionView =
        lazy (new UICollectionView(self.View.Frame, new UICollectionViewFlowLayout()))

    let overViewDelegate =
        { new UICollectionViewDelegateFlowLayout() with
            override this.GetSizeForItem(collectionView, layout, indexPath) =
                CGSize(collectionView.Frame.Width - nfloat 16., nfloat 60.)

            member this.ItemSelected(collectionView, indexPath) =
                self.NavigationController.PushViewController(new ExchangeDetailView(), true) }

    let dataSource =
        { new UICollectionViewDataSource() with

            override this.GetItemsCount(_, _) = nint controller.ExchangesData.Count

            override this.GetCell(collectionView: UICollectionView, indexPath) =
                let cell =
                    collectionView.DequeueReusableCell(ExchangeCell.CellId, indexPath) :?> ExchangeCell

                let exchange =
                    controller.ExchangesData.[int indexPath.Item]

                cell.SetUp(exchange)
                upcast cell }

    let addImagesGestureRecognizer =
        new UITapGestureRecognizer(fun () ->
            if Connectivity.NetworkAccess = NetworkAccess.Local
               || Connectivity.NetworkAccess = NetworkAccess.Internet then

                self.NavigationController.PresentModalViewController(
                    new UINavigationController(new CurrencyFirstStepView()),
                    true
                )
            else
                self.PresentAlertOnMainThread "No internet" "Check your internet connection.")

    override _.ViewDidLoad() =
        base.ViewDidLoad()

        self.View.BackgroundColor <- UIColor.SystemBackgroundColor
        collectionView.Value.BackgroundColor <- UIColor.SystemBackgroundColor
        collectionView.Value.RegisterClassForCell(typeof<ExchangeCell>, ExchangeCell.CellId)

        self.View.BackgroundColor <- UIColor.SystemBackgroundColor
        headerContainerView.TranslatesAutoresizingMaskIntoConstraints <- false
        self.View.AddSubviewsX headerView
        headerView.AddSubviewsX headerContainerView
        headerContainerView.ConstraintToParent(headerView)

        self.View.AddSubviewsX collectionView.Value
        collectionView.Value.TopAnchor.ConstraintEqualTo(headerView.SafeAreaLayoutGuide.BottomAnchor).Active <- true
        collectionView.Value.LeadingAnchor.ConstraintEqualTo(self.View.SafeAreaLayoutGuide.LeadingAnchor, nfloat 16.).Active <- true
        collectionView.Value.TrailingAnchor.ConstraintEqualTo(self.View.SafeAreaLayoutGuide.TrailingAnchor, nfloat -16.).Active <- true
        collectionView.Value.BottomAnchor.ConstraintEqualTo(self.View.SafeAreaLayoutGuide.BottomAnchor, nfloat -16.).Active <- true
        headerView.TopAnchor.ConstraintEqualTo(self.View.SafeAreaLayoutGuide.TopAnchor, nfloat 16.).Active <- true

        headerView.LeadingAnchor.ConstraintEqualTo(self.View.SafeAreaLayoutGuide.LeadingAnchor, nfloat 16.).Active <- true

        headerView.TrailingAnchor.ConstraintEqualTo(self.View.SafeAreaLayoutGuide.TrailingAnchor, nfloat -16.).Active <- true

        headerContainerView.AddArrangedSubview messageLabel
        headerContainerView.AddArrangedSubview addImage
        headerContainerView.Axis <- UILayoutConstraintAxis.Vertical
        headerContainerView.Spacing <- nfloat 8.
        headerContainerView.Distribution <- UIStackViewDistribution.Fill

        addImage.Image <- UIImage.FromBundle(ImageNames.addImage)
        addImage.ContentMode <- UIViewContentMode.ScaleAspectFit
        messageLabel.TextAlignment <- UITextAlignment.Center
        messageLabel.Text <- "Choose a currency pair to compare their live rates"

        addImage.UserInteractionEnabled <- true
        addImage.AddGestureRecognizer addImagesGestureRecognizer

        collectionView.Value.DataSource <- dataSource
        collectionView.Value.Delegate <- overViewDelegate

        controller.ConnectionChecker
        |> Observable.subscribe
            (fun state ->
                if state = InternetConnectionEvent.NotConnected then
                    self.PresentAlertOnMainThread "No internet" "Check your internet connection."
                    observableTimerSubscription.Dispose()
                else
                    observableTimerSubscription <-
                        observableTimer
                        |> Observable.subscribe
                            (fun _ ->
                                controller.FetchExchangeForCurrencies
                                |> Observable.subscribe
                                    (fun state ->
                                        match state with
                                        | ExchangesUpdate.Updated ->
                                            mainThread {
                                                self.DismissLoadingView()
                                                collectionView.Value.ReloadData()
                                            }
                                        | ExchangesUpdate.NotUpdated -> ())
                                |> disposables.Add))
        |> disposables.Add


        controller.LoadCountriesInfo
        |> disposables.Add
        

        (*controller.HandleRequestExchangeUpdate
        |> Observable.subscribe
            (fun _ ->
                mainThread {
                    self.DismissLoadingView()
                    collectionView.Value.ReloadData()
                })
        |> disposables.Add*)

        async {
            mainThread { self.ShowLoadingView() }

            let! state =
                controller.LoadExchanges.AsTask()
                |> Async.AwaitTask

            match state with
            | ExchangesState.Loaded ->
                mainThread {
                    self.DismissLoadingView()
                    collectionView.Value.ReloadData()
                }
            | ExchangesState.NotLoaded -> self.PresentAlertOnMainThread "Exchanges" "Error loading exchanges."
        }
        |> Async.Start

        if controller.IsConnected then
            observableTimerSubscription <-
                observableTimer
                |> Observable.subscribe
                    (fun _ ->
                        controller.FetchExchangeForCurrencies
                        |> Observable.subscribe
                            (fun state ->
                                if state = ExchangesUpdate.Updated then
                                    mainThread {
                                        self.DismissLoadingView()
                                        collectionView.Value.ReloadData()
                                    })
                        |> disposables.Add)
    //        controller.DidRequestExchangesSubject
//        |> disposables.Add
//        |> Observable.subscribe(fun _ ->
//            if controller.IsConnected then
//                    Observable.interval (TimeSpan.FromSeconds 1.)
//                    |> Observable.flatmap
//                        (fun _ ->
//                            controller.FetchExchangeForCurrencies
//                            Observable.Return(Unit))
//                    |> Observable.subscribe (fun _ -> ())
//                    |> disposables.Add
//
//                    mainThread {
//                        self.DismissLoadingView()
//                        collectionView.Value.ReloadData()
//                    }
//                else
//                    self.PresentAlertOnMainThread "No internet" "Check your internet connection.")

    (*controller.HandleLoadExchanges
        |> Observable.subscribe
            (fun _ ->
                if controller.IsConnected then
                    Observable.interval (TimeSpan.FromSeconds 1.)
                    |> Observable.flatmap
                        (fun _ ->
                            controller.FetchExchangeForCurrencies
                            Observable.Return(Unit))
                    |> Observable.subscribe (fun _ -> ())
                    |> disposables.Add

                    mainThread {
                        self.DismissLoadingView()
                        collectionView.Value.ReloadData()
                    }
                else
                    self.PresentAlertOnMainThread "No internet" "Check your internet connection.")

        |> disposables.Add*)


    override self.Dispose _ = disposables.Dispose()
