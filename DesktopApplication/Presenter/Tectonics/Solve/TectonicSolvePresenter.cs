using System.Collections.Generic;
using System.Threading.Tasks;
using Model.Core;
using Model.Core.Highlighting;
using Model.Core.Steps;
using Model.Tectonics;
using Model.Tectonics.Solver;
using Model.Utility;
using Model.Utility.Collections;

namespace DesktopApplication.Presenter.Tectonics.Solve;

public class TectonicSolvePresenter : SolveWithStepsPresenter<ITectonicHighlighter, IStep<ITectonicHighlighter,
    INumericSolvingState>, INumericSolvingState>
{
    private readonly TectonicSolver _solver;
    private readonly ITectonicSolveView _view;
    
    private readonly ContainingList<Cell> _selectedCells = new();
    private IZone? _selectedZone;
    private SelectionMode _selectionMode = SelectionMode.Default;

    public TectonicSolvePresenter(TectonicSolver solver, ITectonicSolveView view, Settings settings) 
        : base(new TectonicHighlightTranslator(view.Drawer))
    {
        _solver = solver;
        _view = view;

        _view.Drawer.LinkOffsetSidePriority = (LinkOffsetSidePriority)settings.LinkOffsetSidePriority.Get().ToInt();
        settings.LinkOffsetSidePriority.ValueChanged += v =>
        {
            _view.Drawer.LinkOffsetSidePriority = (LinkOffsetSidePriority)v.ToInt();
            _view.Drawer.Refresh();
        };
    }

    public void SetTectonicString()
    {
        _view.SetTectonicString(TectonicTranslator.TranslateRdFormat(_solver.Tectonic));
    }

    public void SetNewTectonic(string asString)
    {
        ITectonic tectonic;
        switch (TectonicTranslator.GuessFormat(asString))
        {
            case TectonicStringFormat.Code : tectonic = TectonicTranslator.TranslateCodeFormat(asString);
                break;
            case TectonicStringFormat.Rd : tectonic = TectonicTranslator.TranslateRdFormat(asString);
                break;
            case TectonicStringFormat.None:
            default: return;
        }

        SetNewTectonic(tectonic, true);
    }

    public void SetNewRowCount(int diff)
    {
        var r = _solver.Tectonic.RowCount + diff;
        if (r is < 0 or > 25) return;
        
        SetNewTectonic(_solver.Tectonic.Transfer(r, _solver.Tectonic.ColumnCount), true);
    }

    public void SetNewColumnCount(int diff)
    {
        var c = _solver.Tectonic.ColumnCount + diff;
        if (c is < 0 or > 25) return;
        
        SetNewTectonic(_solver.Tectonic.Transfer(_solver.Tectonic.RowCount, c), true);
    }

    public async void Solve(bool stopAtProgress)
    {
        _solver.StrategyEnded += OnStrategyEnd;

        await Task.Run(() => _solver.Solve(stopAtProgress));

        _solver.StrategyEnded -= OnStrategyEnd;
    }

    public void Clear()
    {
        SetNewTectonic(new ArrayTectonic(_solver.Tectonic.RowCount, _solver.Tectonic.ColumnCount),
            false);
    }

    public void Reset()
    {
        SetNewTectonic(_solver.Tectonic.CopyWithoutDigits(), false);
    }

    public void SelectCell(Cell c)
    {
        switch (_selectionMode)
        {
            case SelectionMode.Default :
                if (_selectedCells.Count == 1 && c == _selectedCells[0])
                {
                    _selectedCells.Clear();
                    _view.Drawer.ClearCursor();
                }
                else
                {
                    _selectedCells.Clear();
                    _selectedCells.Add(c);
                    _view.Drawer.PutCursorOn(c);
                }

                break;
            case SelectionMode.Merge :
                if (_selectedZone is null)
                {
                    _selectedZone = _solver.Tectonic.GetZone(c);
                    _view.Drawer.PutCursorOn(_selectedZone);
                }
                else if(_selectedZone.Contains(c))
                {
                    _selectedZone = null;
                    _view.Drawer.ClearCursor();
                }
                else
                {
                    var tectonic = _solver.Tectonic.Copy();
                    if (tectonic.MergeZones(_selectedZone, _solver.Tectonic.GetZone(c))) SetNewTectonic(tectonic, true);
                    
                    _view.Drawer.ClearCursor();
                    _selectedZone = null;
                }
                break;
            case SelectionMode.Edit :
                if (!_selectedCells.Contains(c)) _selectedCells.Add(c);
                _view.Drawer.PutCursorOn(_selectedCells);
                
                break;
        }
        
        _view.Drawer.Refresh();
    }

    public void EndCellSelection()
    {
        if (_selectionMode == SelectionMode.Edit)
        {
            var tectonic = _solver.Tectonic.Copy();
            if (tectonic.CreateZone(_selectedCells.ToArray())) SetNewTectonic(tectonic, true);
            _selectedCells.Clear();
            
            _view.Drawer.ClearCursor();
            _view.Drawer.Refresh();
        }
    }

    public void SetSelectionMode(SelectionMode mode)
    {
        _selectionMode = mode;
        _selectedCells.Clear();
        _selectedZone = null;
        _view.Drawer.ClearCursor();
        _view.Drawer.Refresh();
    }
    
    public void SetCurrentCell(int n)
    {
        if (_selectedCells.Count != 1) return;

        var c = _selectedCells[0];
        _solver.SetSolutionByHand(n, c.Row, c.Column);
        SetShownState(_solver, !_solver.StartedSolving, true);
        UpdateSteps();
    }

    public void DeleteCurrentCell()
    {
        if (_selectedCells.Count != 1) return;
        
        var c = _selectedCells[0];
        _solver.RemoveSolutionByHand(c.Row, c.Column);
        SetShownState(_solver, !_solver.StartedSolving, true);
        UpdateSteps();
    }

    #region Private
    
    private void OnStrategyEnd(Strategy strategy, int index, int solutionAdded, int possibilitiesRemoved)
    {
        if (possibilitiesRemoved == 0 && solutionAdded == 0) return;
        
        SetShownState(_solver, false, true);
        UpdateSteps();
    }
    
    private void SetNewTectonic(ITectonic tectonic, bool showPossibilities)
    {
        _solver.SetTectonic(tectonic);
        _view.Drawer.ClearHighlights();
        SetUpNewTectonic();
        SetShownState(_solver, true, showPossibilities);
        ClearSteps();
    }

    protected override IReadOnlyList<IStep<ITectonicHighlighter, INumericSolvingState>> Steps => _solver.Steps;
    protected override ISolveWithStepsView View => _view;

    public override IStepExplanationPresenterBuilder? RequestExplanation()
    {
        if (_currentlyOpenedStep >= 0 && _currentlyOpenedStep < _solver.Steps.Count)
            return new TectonicStepExplanationPresenterBuilder(_solver.Steps[_currentlyOpenedStep], _solver.Tectonic);
        return null;
    }

    protected override void SetShownState(INumericSolvingState state, bool asClue, bool showPossibilities)
    {
        var drawer = _view.Drawer;

        drawer.ClearNumbers();
        drawer.ClearHighlights();
        for (int row = 0; row < _solver.Tectonic.RowCount; row++)
        {
            for (int col = 0; col < _solver.Tectonic.ColumnCount; col++)
            {
                var number = state[row, col];
                if (asClue) drawer.SetClue(row, col, number != 0);
                if (number == 0)
                {
                    if (!showPossibilities) continue;
                    
                    var zoneSize = _solver.Tectonic.GetZone(row, col).Count;
                    drawer.ShowPossibilities(row, col, state.PossibilitiesAt(row, col).Enumerate(1, zoneSize));
                }
                else drawer.ShowSolution(row, col, number);
            }
        }
        
        drawer.Refresh();
    }

    protected override INumericSolvingState GetCurrentState()
    {
        return _solver;
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

    #endregion
}

public enum SelectionMode
{
    Default, Merge, Edit
}