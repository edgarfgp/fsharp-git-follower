namespace GitFollowers.Controllers

open System
open System.Reactive.Disposables
open CoreGraphics
open GitFollowers
open GitFollowers.Entities
open GitFollowers.Entities
open GitFollowers.Entities
open GitFollowers.Entities
open GitFollowers.Persistence
open GitFollowers.Services
open GitFollowers.Views
open GitFollowers.DTOs
open FSharp.Control.Reactive
open UIKit
open Xamarin.Essentials

type ExchangeOverviewController() as self =
    inherit UIViewController()

    let addImage = new UIImageView()
    let messageLabel = new FGBodyLabel()
    let headerContainerView = new UIStackView()
    let headerView = new UIView()

    let exchangesData = ResizeArray<Selection>()

    let collectionView =
        lazy (new UICollectionView(self.View.Frame, new UICollectionViewFlowLayout()))

    let disposables = new CompositeDisposable()

    let getExchanges (s: Selection) =
        Observable.interval (TimeSpan.FromSeconds 1.)
        |> Observable.flatmapAsync
            (fun _ ->
                async {
                    let! exchangeResult =
                        (ExchangeService.getExchanges s.first.code s.second.code)
                            .AsTask()
                        |> Async.AwaitTask

                    match exchangeResult with
                    | Ok exchange ->
                        let data =
                            { first = { s.first with value = exchange.first }
                              second =
                                  { s.second with
                                        value = exchange.second } }

                        let result =
                            exchangesData
                            |> Seq.tryFind
                                (fun sel ->
                                    sel.first.code = data.first.code
                                    && sel.second.code = data.second.code)

                        if result.IsSome then
                            let index =
                                exchangesData
                                |> Seq.findIndex
                                    (fun sel ->
                                        sel.first.code = data.first.code
                                        && sel.second.code = data.second.code)

                            exchangesData.[index] <- data

                            let exchange =
                                Exchange.fromDomain (
                                    exchange,
                                    data.first.code,
                                    data.first.name,
                                    data.second.code,
                                    data.second.name
                                )

                            ExchangeRepository.Instance
                                .InsertExchange(exchange)
                                .AsTask()
                            |> Async.AwaitTask
                            |> ignore
                        else
                            exchangesData.Add data

                            let exchange =
                                Exchange.fromDomain (
                                    exchange,
                                    data.first.code,
                                    data.first.name,
                                    data.second.code,
                                    data.second.name
                                )

                            ExchangeRepository.Instance
                                .InsertExchange(exchange)
                                .AsTask()
                            |> Async.AwaitTask
                            |> ignore

                        mainThread { collectionView.Value.ReloadData() }
                    | Error error -> printfn $"ERROR ======> {error}"
                })

    let overViewDelegate =
        { new UICollectionViewDelegateFlowLayout() with

            override this.GetSizeForItem(collectionView, layout, indexPath) =
                CGSize(collectionView.Frame.Width - nfloat 16., nfloat 60.) }

    let dataSource =
        { new UICollectionViewDataSource() with

            override this.GetItemsCount(_, _) = nint exchangesData.Count

            override this.GetCell(collectionView: UICollectionView, indexPath) =
                let cell =
                    collectionView.DequeueReusableCell(ExchangeCell.CellId, indexPath) :?> ExchangeCell

                let exchange = exchangesData.[int indexPath.Item]
                cell.SetUp(exchange)
                upcast cell }

    let getExchangeFor selection =
        (getExchanges selection).Subscribe(fun _ -> ())
        |> disposables.Add

    let initialize (state: ConnectivityChangedEventArgs) =
        if state.NetworkAccess = NetworkAccess.None
           || state.NetworkAccess = NetworkAccess.Unknown then
            self.PresentAlertOnMainThread "No internet" "Check your internet connection."
        else
            async {
                let! exchanges =
                    ExchangeRepository.Instance.GetAllExchanges.AsTask()
                    |> Async.AwaitTask

                let result =
                    exchanges
                    |> Seq.map Exchange.toDomain

                exchangesData.AddRange result

                result
                |> Observable.toObservable
                |> Observable.subscribe getExchangeFor
                |> disposables.Add

            }
            |> Async.Start
            
    let saveCurrenciesTRepository currency =
        let data = Currency.fromDomain currency
        ExchangeRepository.Instance.InsertCurrency(data).AsTask()
        |> Async.AwaitTask
        |> ignore

    override _.ViewDidLoad() =
        base.ViewDidLoad()

        self.ShowLoadingView()

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

        let addImagesGesture =
            new UITapGestureRecognizer(fun () ->
                self.NavigationController.PresentModalViewController(
                    new UINavigationController(new CurrencyFirstStepController()),
                    true
                ))

        addImage.UserInteractionEnabled <- true
        addImage.AddGestureRecognizer addImagesGesture

        collectionView.Value.DataSource <- dataSource
        collectionView.Value.Delegate <- overViewDelegate

        mainThread { self.ShowLoadingView() }

        mainThread { self.DismissLoadingView() }

        ExchangeLoader.didRequestFollowers
        |> Observable.subscribe getExchangeFor
        |> disposables.Add

        ExchangeLoader.connectionChecker
        |> Observable.subscribe initialize
        |> disposables.Add
        
        ExchangeLoader.loadCountriesInfo
        |> Observable.subscribe saveCurrenciesTRepository
        |> disposables.Add
        
//        async {
//                let! exchanges =
//                    ExchangeRepository.getAllExchanges().AsTask()
//                    |> Async.AwaitTask
//
//                exchanges
//                |> Seq.map Exchange.toDomain
//                |> Observable.toObservable
//                |> Observable.subscribe getExchangeFor
//                |> disposables.Add
//
//            }
//            |> Async.Start

    override self.Dispose _ = disposables.Dispose()
