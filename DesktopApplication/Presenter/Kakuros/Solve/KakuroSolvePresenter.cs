using Model.Kakuros;

namespace DesktopApplication.Presenter.Kakuros.Solve;

public class KakuroSolvePresenter
{
    private readonly IKakuroSolveView _view;
    private readonly KakuroSolver _solver;

    public KakuroSolvePresenter(IKakuroSolveView view)
    {
        _view = view;
        _solver = new KakuroSolver();
    }

    public void SetNewKakuro(string s)
    {
        var k = KakuroTranslator.TranslateSumFormat(s);
        _solver.SetKakuro(k);
        ShowNewKakuro(k);
    }

    private void ShowNewKakuro(IKakuro k)
    {
        var drawer = _view.Drawer;

        drawer.RowCount = k.RowCount;
        drawer.ColumnCount = k.ColumnCount;

        drawer.ClearPresence();
        foreach (var sum in k.Sums)
        {
            var c = sum.GetAmountCell();
            drawer.SetPresence(c.Row, c.Column, sum.Orientation, true);
            foreach (var cell in sum)
            {
                drawer.SetPresence(cell.Row, cell.Column, true);
            }
        }
        
        drawer.RedrawLines();
        drawer.Refresh();
    }
}