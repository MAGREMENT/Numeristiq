using System.Threading.Tasks;
using Model.Helpers;
using Model.Kakuros;
using Model.Utility.BitSets;

namespace DesktopApplication.Presenter.Kakuros.Solve;

public class KakuroSolvePresenter
{
    private readonly IKakuroSolveView _view;
    private readonly KakuroSolver _solver;

    public KakuroSolvePresenter(IKakuroSolveView view)
    {
        _view = view;
        _solver = new KakuroSolver(new RecursiveKakuroCombinationCalculator());
    }

    public void SetNewKakuro(string s)
    {
        var k = KakuroTranslator.TranslateSumFormat(s);
        _solver.SetKakuro(k);
        ShowNewKakuro(k);
    }

    public async void Solve(bool stopAtProgress)
    {
        _solver.ProgressMade += OnProgressMade;
        await Task.Run(() => _solver.Solve(stopAtProgress));
        _solver.ProgressMade -= OnProgressMade;
    }

    private void ShowNewKakuro(IKakuro k)
    {
        var drawer = _view.Drawer;

        drawer.RowCount = k.RowCount;
        drawer.ColumnCount = k.ColumnCount;

        drawer.ClearNumbers();
        drawer.ClearPresence();
        foreach (var sum in k.Sums)
        {
            foreach (var cell in sum)
            {
                drawer.SetPresence(cell.Row, cell.Column, true);
                var n = k[cell];
                if (n != 0) drawer.SetSolution(cell.Row, cell.Column, n);
                else drawer.SetPossibilities(cell.Row, cell.Column, 
                    _solver.PossibilitiesAt(cell.Row, cell.Column).EnumeratePossibilities());
            }

            var c = sum.GetAmountCell();
            drawer.SetPresence(c.Row, c.Column, sum.Orientation, true);
            drawer.SetAmount(c.Row, c.Column, sum.Amount, sum.Orientation);
        }
        
        drawer.RedrawLines();
        drawer.Refresh();
    }

    private void OnProgressMade() => ShowState(_solver);

    private void ShowState(ISolvingState state)
    {
        var drawer = _view.Drawer;
        
        drawer.ClearNumbers();
        foreach (var cell in _solver.Kakuro.EnumerateCells())
        {
            var n = state[cell.Row, cell.Column];
            if (n == 0)
                drawer.SetPossibilities(cell.Row, cell.Column,
                    state.PossibilitiesAt(cell).EnumeratePossibilities());
            else drawer.SetSolution(cell.Row, cell.Column, n);
        }
        
        drawer.Refresh();
    }
}