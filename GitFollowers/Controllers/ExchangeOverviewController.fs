namespace GitFollowers.Controllers

open System
open System.Reactive.Disposables
open CoreGraphics
open GitFollowers
open GitFollowers.Entities
open GitFollowers.Persistence
open GitFollowers.Services
open GitFollowers.Views
open GitFollowers.DTOs
open FSharp.Control.Reactive
open UIKit

type ExchangeOverviewController() as self =
    inherit UIViewController()

    let addImage = new UIImageView()
    let messageLabel = new FGBodyLabel()
    let headerContainerView = new UIStackView()
    let headerView = new UIView()

    let exchanges = ResizeArray<Selection>()

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
                            exchanges
                            |> Seq.tryFind
                                (fun sel ->
                                    sel.first.code = data.first.code
                                    && sel.second.code = data.second.code)

                        if result.IsSome then
                            let index =
                                exchanges
                                |> Seq.findIndex
                                    (fun sel ->
                                        sel.first.code = data.first.code
                                        && sel.second.code = data.second.code)

                            exchanges.[index] <- data
                        else
                            exchanges.Add data

                        mainThread { collectionView.Value.ReloadData() }
                    | Error error ->
                        printfn $"ERROR ======> {error}"
                })

    let overViewDelegate =
        { new UICollectionViewDelegateFlowLayout() with

            override this.GetSizeForItem(collectionView, layout, indexPath) =
                CGSize(collectionView.Frame.Width - nfloat 16., nfloat 60.) }

    let dataSource =
        { new UICollectionViewDataSource() with

            override this.GetItemsCount(_, _) = nint exchanges.Count

            override this.GetCell(collectionView: UICollectionView, indexPath) =
                let cell =
                    collectionView.DequeueReusableCell(ExchangeCell.CellId, indexPath) :?> ExchangeCell

                let exchange = exchanges.[int indexPath.Item]
                cell.SetUp(exchange)
                upcast cell }

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
        addImage.AddGestureRecognizer(addImagesGesture)

        collectionView.Value.DataSource <- dataSource
        collectionView.Value.Delegate <- overViewDelegate
        
        mainThread {
            self.ShowLoadingView()
        }

        ExchangeLoader.loadCountriesInfo
        |> Observable.subscribe
            (fun currency ->
                let data = Currency.fromDomain currency
                ExchangeRepository.insertCurrency(data).AsTask()
                |> Async.AwaitTask
                |> ignore)
        |> disposables.Add
        
        mainThread {
            self.DismissLoadingView()
        }

        ExchangeLoader.didRequestFollowers
        |> Observable.subscribe
            (fun selection ->
                (getExchanges selection)
                    .Subscribe(fun _ -> printfn $"Loading data for {selection.first.code} and {selection.second.code}")
                |> disposables.Add)
        |> disposables.Add

    override self.Dispose _ = disposables.Dispose()
