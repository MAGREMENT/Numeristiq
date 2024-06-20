using System.Threading.Tasks;
using Model.Core;
using Model.Core.Trackers;
using Model.Tectonics;
using Model.Tectonics.Solver;
using Model.Utility;
using Model.Utility.Collections;

namespace DesktopApplication.Presenter.Tectonics.Solve;

public class TectonicSolvePresenter
{
    private readonly TectonicSolver _solver;
    private readonly ITectonicSolveView _view;

    private readonly TectonicHighlightTranslator _translator;
    private readonly UIUpdateTracker _tracker;
    
    private int _stepCount;
    private int _currentlyOpenedStep = -1;
    private StateShown _stateShown = StateShown.Before;
    private readonly ContainingList<Cell> _selectedCells = new();
    private IZone? _selectedZone;
    private SelectionMode _selectionMode = SelectionMode.Default;

    public TectonicSolvePresenter(TectonicSolver solver, ITectonicSolveView view)
    {
        _solver = solver;
        _view = view;

        _translator = new TectonicHighlightTranslator(_view.Drawer);
        _tracker = new UIUpdateTracker(this);
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
        _tracker.AttachTo(_solver);

        await Task.Run(() => _solver.Solve(stopAtProgress));

        _tracker.Detach();
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
    
    public void RequestLogOpening(int id)
    {
        var index = id - 1;
        if (index < 0 || index > _solver.Steps.Count) return;
        
        _view.CloseLogs();

        if (_currentlyOpenedStep == index)
        {
            _currentlyOpenedStep = -1;
            SetShownState(_solver, false, true);
        }
        else
        {
            _view.OpenLog(index);
            _currentlyOpenedStep = index;

            var log = _solver.Steps[index];
            SetShownState(_stateShown == StateShown.Before ? log.From : log.To, false, true); 
            _translator.Translate(log.HighlightManager); 
        }
    }

    public void RequestStateShownChange(StateShown ss)
    {
        _stateShown = ss;
        _view.SetLogsStateShown(ss);
        if (_currentlyOpenedStep < 0 || _currentlyOpenedStep > _solver.Steps.Count) return;
        
        var log = _solver.Steps[_currentlyOpenedStep];
        SetShownState(_stateShown == StateShown.Before ? log.From : log.To, false, true); 
        _translator.Translate(log.HighlightManager);
    }

    public void RequestHighlightShift(bool isLeft)
    {
        if (_currentlyOpenedStep < 0 || _currentlyOpenedStep > _solver.Steps.Count) return;
        
        var log = _solver.Steps[_currentlyOpenedStep];
        if(isLeft) log.HighlightManager.ShiftLeft();
        else log.HighlightManager.ShiftRight();
        
        _view.Drawer.ClearHighlights();
        _translator.Translate(log.HighlightManager);
        _view.SetCursorPosition(_currentlyOpenedStep, log.HighlightManager.CursorPosition());
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
            case SelectionMode.Split :
                if (!_selectedCells.Contains(c)) _selectedCells.Add(c);
                _view.Drawer.PutCursorOn(_selectedCells);
                
                break;
        }
        
        _view.Drawer.Refresh();
    }

    public void EndCellSelection()
    {
        if (_selectionMode == SelectionMode.Split)
        {
            var tectonic = _solver.Tectonic.Copy();
            if (tectonic.SplitZone(_selectedCells.ToArray())) SetNewTectonic(tectonic, true);
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
        UpdateLogs();
    }

    public void DeleteCurrentCell()
    {
        if (_selectedCells.Count != 1) return;
        
        var c = _selectedCells[0];
        _solver.RemoveSolutionByHand(c.Row, c.Column);
        SetShownState(_solver, !_solver.StartedSolving, true);
        UpdateLogs();
    }

    #region Private

    private void ClearLogs()
    {
        _view.ClearLogs();
        _stepCount = 0;
    }
    
    private void SetNewTectonic(ITectonic tectonic, bool showPossibilities)
    {
        _solver.SetTectonic(tectonic);
        _view.Drawer.ClearHighlights();
        SetUpNewTectonic();
        SetShownState(_solver, true, showPossibilities);
        ClearLogs();
    }

    private void SetShownState(ISolvingState state, bool asClue, bool showPossibilities)
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

    public void ShowCurrentState()
    {
        SetShownState(_solver, false, true);
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
    
    public void UpdateLogs()
    {
        if (_solver.Steps.Count < _stepCount)
        {
            ClearLogs();
            return;
        }

        for (;_stepCount < _solver.Steps.Count; _stepCount++)
        {
            _view.AddLog(_solver.Steps[_stepCount], _stateShown);
        }
    }

    #endregion
}

public class UIUpdateTracker : Tracker<Strategy<ITectonicSolverData>, object>
{
    private readonly TectonicSolvePresenter _presenter;

    public UIUpdateTracker(TectonicSolvePresenter presenter)
    {
        _presenter = presenter;
    }

    protected override void OnAttach(ITrackerAttachable<Strategy<ITectonicSolverData>, object> attachable)
    {
        attachable.StrategyEnded += OnStrategyEnd;
    }

    protected override void OnDetach(ITrackerAttachable<Strategy<ITectonicSolverData>, object> attachable)
    {
        attachable.StrategyEnded -= OnStrategyEnd;
    }
    
    private void OnStrategyEnd(Strategy<ITectonicSolverData> strategy, int index, int solutionAdded, int possibilitiesRemoved)
    {
        if (possibilitiesRemoved == 0 && solutionAdded == 0) return;
        
        _presenter.ShowCurrentState();
        _presenter.UpdateLogs();
    }
}

public enum SelectionMode
{
    Default, Merge, Split
}