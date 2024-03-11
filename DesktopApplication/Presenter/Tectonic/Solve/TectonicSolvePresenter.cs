using System.Threading.Tasks;
using Model.Tectonic;

namespace DesktopApplication.Presenter.Tectonic.Solve;

public class TectonicSolvePresenter
{
    private readonly TectonicSolver _solver;
    private readonly ITectonicSolveView _view;

    public TectonicSolvePresenter(TectonicSolver solver, ITectonicSolveView view)
    {
        _solver = solver;
        _view = view;
    }

    public void SetNewTectonic(string asString)
    {
        _solver.SetTectonic(TectonicTranslator.TranslateLineFormat(asString));
        SetUpNewTectonic();
        ShowTectonic();
    }

    public async void Solve(bool stopAtProgress)
    {
        _solver.ProgressMade += ShowTectonic;

        await Task.Run(() => _solver.Solve(stopAtProgress));

        _solver.ProgressMade -= ShowTectonic;
    }

    private void ShowTectonic()
    {
        var drawer = _view.Drawer;

        drawer.ClearNumbers();
        for (int row = 0; row < _solver.Tectonic.RowCount; row++)
        {
            for (int col = 0; col < _solver.Tectonic.ColumnCount; col++)
            {
                var number = _solver[row, col];
                if (number == 0)
                {
                    var zoneSize = _solver.Tectonic.GetZone(row, col).Count;
                    drawer.ShowPossibilities(row, col, _solver.PossibilitiesAt(row, col).Enumerate(1, zoneSize));
                }
                else drawer.ShowSolution(row, col, number);
            }
        }
        
        drawer.Refresh();
    }

    private void SetUpNewTectonic()
    {
        var drawer = _view.Drawer;

        drawer.RowCount = _solver.Tectonic.RowCount;
        drawer.ColumnCount = _solver.Tectonic.ColumnCount;

        drawer.ClearBorderDefinitions();
        for (int row = 0; row < _solver.Tectonic.RowCount; row++)
        {
            for (int col = 0; col < _solver.Tectonic.ColumnCount - 1; col++)
            {
                drawer.AddBorderDefinition(row, col, BorderDirection.Vertical,
                    _solver.Tectonic.GetZone(row, col).Equals(_solver.Tectonic.GetZone(row, col + 1)));
            }
        }

        for (int col = 0; col < _solver.Tectonic.ColumnCount; col++)
        {
            for (int row = 0; row < _solver.Tectonic.RowCount - 1; row++)
            {
                drawer.AddBorderDefinition(row, col, BorderDirection.Horizontal,
                    _solver.Tectonic.GetZone(row, col).Equals(_solver.Tectonic.GetZone(row + 1, col)));
            }
        }
        
        drawer.Refresh();
    }
}