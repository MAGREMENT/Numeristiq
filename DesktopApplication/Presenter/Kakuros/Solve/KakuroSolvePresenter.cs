using System.Collections.Generic;
using System.Threading.Tasks;
using Model.Core;
using Model.Core.Highlighting;
using Model.Core.Steps;
using Model.Kakuros;
using Model.Utility;
using Model.Utility.BitSets;
using Orientation = Model.Utility.Orientation;

namespace DesktopApplication.Presenter.Kakuros.Solve;

public class KakuroSolvePresenter : SolveWithStepsPresenter<INumericSolvingStateHighlighter, 
    INumericStep<INumericSolvingStateHighlighter>, INumericSolvingState>
{
    private readonly IKakuroSolveView _view;
    private readonly KakuroSolver _solver;

    private Cell? _selectedCell;
    private IKakuroSum? _selectedSum;
    private int _bufferedAmount = -1;
    private EditMode _mode = EditMode.Default;

    public KakuroSolvePresenter(IKakuroSolveView view, KakuroSolver solver, Settings settings)
        : base(new KakuroHighlightTranslator(view.Drawer))
    {
        _view = view;
        _solver = solver;

        _view.Drawer.FastPossibilityDisplay = settings.FastPossibilityDisplay.Get().ToBool();
        settings.FastPossibilityDisplay.ValueChanged += v =>
        {
            _view.Drawer.FastPossibilityDisplay = v.ToBool();
            _view.Drawer.Refresh();
        };
    }
    
    public void OnKakuroAsStringBoxShowed()
    {
        _view.SetKakuroAsString(KakuroTranslator.TranslateSumFormat(_solver.Kakuro));
    }

    public void SetNewKakuro(string s)
    {
        var k = KakuroTranslator.TranslateSumFormat(s);
        _solver.SetKakuro(k);
        ShowNewKakuro(k);
    }

    public async void Solve(bool stopAtProgress)
    {
        _solver.StrategyEnded += OnProgressMade;
        await Task.Run(() => _solver.Solve(stopAtProgress));
        _solver.StrategyEnded -= OnProgressMade;
    }

    public void SetEditMode(EditMode mode)
    {
        _mode = mode;
    }

    public void AddDefault()
    {
        if (_mode != EditMode.Edit || _solver.Kakuro is { RowCount: > 0, ColumnCount: > 0 }) return;

        var k = IKakuro.Default(Orientation.Horizontal);
        _solver.SetKakuro(k);
        ShowNewKakuro(k);
    }

    public void SelectCell(int row, int col)
    {
        switch (_mode)
        {
            case EditMode.Default :
                EnterAmount();
        
                var cell = new Cell(row, col);
                if (cell == _selectedCell)
                {
                    _selectedCell = null;
                    _view.Drawer.ClearCursor();
                }
                else
                {
                    _selectedCell = cell;
                    _view.Drawer.PutCursorOnNumberCell(row, col);
                }
        
                _view.Drawer.Refresh();
                break;
            case EditMode.Edit :
                var copy = _solver.Kakuro.Copy();
                if(!copy.RemoveCell(new Cell(row, col))) return;
        
                _solver.SetKakuro(copy);
                ShowNewKakuro(copy);
                break;
        }
    }

    public void SelectSum(int row, int col)
    {
        _selectedCell = null;

        var sum = _solver.Kakuro.FindSum(new Cell(row, col));

        switch (_mode)
        {
            case EditMode.Default :
                if (sum is null || sum.Equals(_selectedSum)) EnterAmount();
                else
                {
                    _selectedSum = sum;
                    _bufferedAmount = sum.Amount;
                    _view.Drawer.PutCursorOnAmountCell(row, col, sum.Orientation);
                }
            
                _view.Drawer.Refresh();
                break;
            case EditMode.Edit :
                if (sum is null) return;
            
                var copy = _solver.Kakuro.Copy();
                if (!copy.AddCellTo(sum)) return;
        
                _solver.SetKakuro(copy);
                ShowNewKakuro(copy);
                break;
        }
    }

    public void AddDigitToAmount(int n)
    {
        if (_selectedSum is null) return;
        
        _bufferedAmount *= 10;
        _bufferedAmount += n;

        var cell = _selectedSum.GetAmountCell();
        _view.Drawer.ReplaceAmount(cell.Row, cell.Column, _bufferedAmount, _selectedSum.Orientation);
        _view.Drawer.Refresh();
    }

    public void RemoveDigitFromAmount()
    {
        if (_selectedSum is null) return;
        
        _bufferedAmount /= 10;
        
        var cell = _selectedSum.GetAmountCell();
        _view.Drawer.ReplaceAmount(cell.Row, cell.Column, _bufferedAmount, _selectedSum.Orientation);
        _view.Drawer.Refresh();
    }

    public void EnterAmount()
    {
        if (_selectedSum is null) return;

        var copy = _solver.Kakuro.Copy();
        bool success = copy.ReplaceAmount(_selectedSum, _bufferedAmount);
        
        _selectedSum = null;
        _bufferedAmount = -1;
        _view.Drawer.ClearCursor();

        if (success)
        {
            _solver.SetKakuro(copy);
            ShowNewKakuro(copy);
        }
        else SetShownState(_solver, false, true);
    }

    private void ShowNewKakuro(IKakuro k)
    {
        ClearSteps();
        var drawer = _view.Drawer;

        drawer.RowCount = k.RowCount;
        drawer.ColumnCount = k.ColumnCount;

        drawer.ClearNumbers();
        drawer.ClearAmounts();
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
            drawer.AddAmount(c.Row, c.Column, sum.Amount, sum.Orientation);
        }
        
        drawer.Refresh();
    }

    private void OnProgressMade(Strategy strategy, int index, int solutionAdded, int possibilitiesRemoved)
    {
        if (solutionAdded + possibilitiesRemoved == 0) return;

        SetShownState(_solver, false, true);
        UpdateSteps();
    } 
    
    protected override IReadOnlyList<INumericStep<INumericSolvingStateHighlighter>> Steps => _solver.Steps;
    protected override ISolveWithStepsView View => _view;
    public override IStepExplanationPresenterBuilder? RequestExplanation()
    {
        return null;
    }

    protected override void SetShownState(INumericSolvingState state, bool solutionAsClues, bool showPossibilities)
    {
        var drawer = _view.Drawer;
        
        drawer.ClearNumbers();
        foreach (var cell in _solver.Kakuro.EnumerateCells())
        {
            var n = state[cell.Row, cell.Column];
            if (n == 0 && showPossibilities) drawer.SetPossibilities(cell.Row, cell.Column,
                state.PossibilitiesAt(cell).EnumeratePossibilities());
            else drawer.SetSolution(cell.Row, cell.Column, n);
        }
        
        drawer.Refresh();
    }

    protected override INumericSolvingState GetCurrentState() => _solver;
}

public enum EditMode
{
    Default, Edit
}