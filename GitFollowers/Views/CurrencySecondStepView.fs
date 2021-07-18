namespace GitFollowers.Views

open System
open System.Reactive.Disposables
open Foundation
open GitFollowers
open GitFollowers.Controllers
open GitFollowers.Elements
open GitFollowers.DTOs
open GitFollowers.Persistence
open UIKit

type CurrencySecondStepView(currencyData: CurrencyData) as self =
    inherit UIViewController()

    let tableView = lazy (new UITableView(self.View.Frame))
    
    let disposables = new CompositeDisposable()
    
    let controller = ExchangesController(ExchangeRepository())
    
    let dataSource =
        { new UITableViewDataSource() with
            member this.GetCell(tableView: UITableView, indexPath) =
                let cell =
                    tableView.DequeueReusableCell(CurrencyCell.CellId, indexPath) :?> CurrencyCell

                let country = controller.CurrenciesData.[int indexPath.Item]
                cell.SetUp(country)
                upcast cell

            member this.RowsInSection(_, _) = nint controller.CurrenciesData.Count }
        
    let tableViewDelegate =
        { new UITableViewDelegate() with
            member this.RowSelected(tableView, indexPath: NSIndexPath) =
                let country = controller.CurrenciesData.[int indexPath.Item]
                let selection: Selection = { first = currencyData ; second = country }
                controller.PublishSelection selection
                self.DismissViewController(true, null)
        }

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
        configureTableView()

        mainThread {
            self.ShowLoadingView()
        }

        controller.LoadCurrencies.AsTask()
        |> Async.AwaitTask
        |> ignore
        
        controller.HandleLoadCurrenciesSubject
        |> Observable.subscribe(fun _ ->
             mainThread {
                self.DismissLoadingView()
                tableView.Value.ReloadData()
            })
        |> disposables.Add

    override self.ViewWillDisappear _ =
        disposables.Dispose()

    override self.Dispose _ =
        disposables.Dispose()
