using System.Threading.Tasks;
using Model.Nonograms;
using Model.Nonograms.Strategies;

namespace DesktopApplication.Presenter.Nonograms.Solve;

public class NonogramSolvePresenter
{
    private readonly INonogramSolveView _view;
    private readonly NonogramSolver _solver;

    public NonogramSolvePresenter(INonogramSolveView view)
    {
        _view = view;
        _solver = new NonogramSolver();
        _solver.StrategyManager.AddStrategies(new PerfectSpaceStrategy(), new NotEnoughSpaceStrategy());
    }

    public void SetNewNonogram(string s)
    {
        _solver.SetNonogram(NonogramTranslator.TranslateLineFormat(s));
        SetUpNewNonogram();
        ShowCurrentState();
    }

    public void ShowNonogramAsString()
    {
        _view.ShowNonogramAsString(NonogramTranslator.TranslateLineFormat(_solver.Nonogram));
    }

    public async void Solve(bool stopAtProgress)
    {
        await Task.Run(() => _solver.Solve(stopAtProgress));
        ShowCurrentState();
    }

    private void SetUpNewNonogram()
    {
        var drawer = _view.Drawer;
        drawer.SetRows(_solver.Nonogram.HorizontalLineCollection);
        drawer.SetColumns(_solver.Nonogram.VerticalLineCollection);
        ShowCurrentState();
    }

    private void ShowCurrentState()
    {
        var drawer = _view.Drawer;
        drawer.ClearSolutions();
        drawer.ClearUnavailable();
        
        for (int row = 0; row < _solver.Nonogram.RowCount; row++)
        {
            for (int col = 0; col < _solver.Nonogram.ColumnCount; col++)
            {
                if (_solver.Nonogram[row, col]) drawer.SetSolution(row, col);
                else if (!_solver.IsAvailable(row, col)) drawer.SetUnavailable(row, col);
            }
        }
        
        drawer.Refresh();
    }
}