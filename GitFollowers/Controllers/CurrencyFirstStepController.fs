namespace GitFollowers.Controllers

open System
open System.Reactive.Disposables
open Foundation
open GitFollowers
open GitFollowers.DTOs
open GitFollowers.Entities
open GitFollowers.Persistence
open GitFollowers.Views
open UIKit

type CurrencyFirstStepController() as self =
    inherit UIViewController()

    let tableView = lazy (new UITableView(self.View.Frame))

    let countriesData = ResizeArray<CurrencyData>()

    let disposables = new CompositeDisposable()

    let dataSource =
        { new UITableViewDataSource() with
            member this.GetCell(tableView: UITableView, indexPath) =
                let cell =
                    tableView.DequeueReusableCell(CurrencyCell.CellId, indexPath) :?> CurrencyCell

                let country = countriesData.[int indexPath.Item]
                cell.SetUp(country)
                upcast cell

            member this.RowsInSection(_, _) = nint countriesData.Count }

    let tableViewDelegate =
        { new UITableViewDelegate() with
            member this.RowSelected(tableView, indexPath: NSIndexPath) =
                let country = countriesData.[int indexPath.Item]
                self.NavigationController.PushViewController(new CurrencySecondStepController(country), true) }

    let configureTableView () =
        tableView.Value.TranslatesAutoresizingMaskIntoConstraints <- false
        tableView.Value.ConstraintToParent(self.View, nfloat 16.)
        tableView.Value.RegisterClassForCellReuse(typeof<CurrencyCell>, CurrencyCell.CellId)
        tableView.Value.SeparatorStyle <- UITableViewCellSeparatorStyle.None
        tableView.Value.RowHeight <- nfloat 50.
        tableView.Value.DataSource <- dataSource
        tableView.Value.Delegate <- tableViewDelegate

    override self.ViewDidLoad() =
        base.ViewDidLoad()

        self.View.BackgroundColor <- UIColor.SystemBackgroundColor
        self.View.AddSubviewsX tableView.Value
        configureTableView ()

        self.NavigationController.SetNavigationBarHidden(true, true)

        mainThread { self.ShowLoadingView() }

        async {
            let! result =
                ExchangeRepository.Instance.getAllCurrencies.AsTask()
                |> Async.AwaitTask

            let currencies =
                result
                |> Seq.map Currency.toDomain |> Seq.toArray

            countriesData.AddRange currencies
            
            mainThread {
                self.DismissLoadingView()
                tableView.Value.ReloadData()
            }
        }
        |> Async.Start

    override self.ViewWillDisappear _ = disposables.Dispose()

    override self.Dispose _ = disposables.Dispose()
