namespace GitFollowers.Views

open System
open System.Reactive.Disposables
open CoreGraphics
open GitFollowers
open GitFollowers.Controllers
open GitFollowers.Elements
open GitFollowers.DTOs
open FSharp.Control.Reactive
open UIKit
open Xamarin.Essentials

type ExchangeListView() as self =
    inherit UIViewController()

    let addImage = new UIImageView()
    let messageLabel = new FGBodyLabel()
    let headerContainerView = new UIStackView()
    let headerView = new UIView()
    let exchangeArray = ResizeArray<Selection>()
    let mutable exchangeObservable : IObservable<unit> = null
    let mutable didRequestObservable : IObservable<unit> = null
    let mutable exchangeSubscription : IDisposable = null
    let mutable loadCountriesSubscription : IDisposable = null
    let mutable didRequestExchangeSubscription : IDisposable = null
    let disposables = new CompositeDisposable()

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

            override this.GetItemsCount(_, _) = nint exchangeArray.Count

            override this.GetCell(collectionView: UICollectionView, indexPath) =
                let cell =
                    collectionView.DequeueReusableCell(ExchangeCell.CellId, indexPath) :?> ExchangeCell

                let exchange = exchangeArray.[int indexPath.Item]
                cell.SetUp(exchange)
                upcast cell }

    let checkConnection (state: ConnectivityChangedEventArgs) =
        if state.NetworkAccess = NetworkAccess.None
           || state.NetworkAccess = NetworkAccess.Unknown then
            self.PresentAlertOnMainThread "No internet" "Check your internet connection."
            exchangeSubscription.Dispose()
        else
            exchangeObservable
            |> Observable.subscribe (fun _ -> ())
            |> disposables.Add

    let addImagesGestureRecognizer =
        new UITapGestureRecognizer(fun () ->
            if Connectivity.NetworkAccess = NetworkAccess.Local
               || Connectivity.NetworkAccess = NetworkAccess.Internet then
                loadCountriesSubscription.Dispose()

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

        mainThread { self.ShowLoadingView() }

        didRequestObservable <-
            ExchangesController.requestExchangesSubject
            |> Observable.flatmapAsync
                (fun selection ->
                    async {
                        let! result = ExchangesController.getExchangeFor selection

                        match result with
                        | Some exchange ->
                            exchangeArray
                            |> ExchangesController.updateExchangeData selection exchange

                            mainThread { collectionView.Value.ReloadData() }
                        | None ->
                            self.PresentAlertOnMainThread
                                "Info"
                                $"Exchange not available for {selection.first.name}. Please select a different pair"
                    })

        didRequestExchangeSubscription <-
            didRequestObservable
            |> Observable.subscribeWithCallbacks
                (fun _ -> mainThread { self.ShowLoadingView() })
                (fun error -> printfn $"{error.Message}")
                (fun _ -> mainThread { self.DismissLoadingView() })

        ExchangesController.connectionChecker
        |> Observable.subscribe checkConnection
        |> disposables.Add

        loadCountriesSubscription <-
            ExchangesController.loadCountriesInfo
            |> Observable.subscribe ExchangesController.saveCurrenciesToRepository

        exchangeObservable <-
            Observable.interval (TimeSpan.FromSeconds 1.)
            |> Observable.flatmapAsync
                (fun _ ->
                    async {
                        let! exchanges = ExchangesController.loadExchangesFromRepo

                        exchanges
                        |> Seq.iter
                            (fun selection ->
                                async {
                                    let! result = ExchangesController.getExchangeFor selection

                                    match result with
                                    | Some exchange ->
                                        exchangeArray
                                        |> ExchangesController.updateExchangeData selection exchange

                                        mainThread {
                                            self.DismissLoadingView()
                                            collectionView.Value.ReloadData()
                                        }
                                    | None -> printfn $"No exchange found for ======> {selection}"
                                }
                                |> Async.Start)

                    })

        if ExchangesController.isConnected then
            exchangeSubscription <-
                exchangeObservable
                |> Observable.subscribe (fun _ -> ())

        else
            self.PresentAlertOnMainThread "No internet" "Check your internet connection."

            async {
                let! exchanges = ExchangesController.loadExchangesFromRepo
                exchanges |> exchangeArray.AddRange

                mainThread {
                    self.DismissLoadingView()
                    collectionView.Value.ReloadData()
                }
            }
            |> Async.Start


    override self.Dispose _ = disposables.Dispose()
