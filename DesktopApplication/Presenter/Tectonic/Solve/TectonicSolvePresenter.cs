using System.Collections.Generic;
using System.Threading.Tasks;
using Model;
using Model.Helpers;
using Model.Tectonic;
using Model.Utility;

namespace DesktopApplication.Presenter.Tectonic.Solve;

public class TectonicSolvePresenter
{
    private readonly TectonicSolver _solver;
    private readonly ITectonicSolveView _view;

    private readonly TectonicHighlightTranslator _translator;
    
    private int _logCount;
    private int _currentlyOpenedLog = -1;
    private StateShown _stateShown = StateShown.Before;
    private readonly List<Cell> _selectedCells = new();
    private IZone? _selectedZone;
    private SelectionMode _selectionMode = SelectionMode.Default;

    public TectonicSolvePresenter(TectonicSolver solver, ITectonicSolveView view)
    {
        _solver = solver;
        _view = view;

        _translator = new TectonicHighlightTranslator(_view.Drawer);
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

        SetNewTectonic(tectonic);
    }

    public void SetNewRowCount(int diff)
    {
        SetNewTectonic(new ArrayTectonic(_solver.Tectonic.RowCount + diff, _solver.Tectonic.ColumnCount));
    }

    public void SetNewColumnCount(int diff)
    {
        SetNewTectonic(new ArrayTectonic(_solver.Tectonic.RowCount, _solver.Tectonic.ColumnCount + diff));
    }

    public async void Solve(bool stopAtProgress)
    {
        _solver.ProgressMade += OnProgressMade;

        await Task.Run(() => _solver.Solve(stopAtProgress));

        _solver.ProgressMade -= OnProgressMade;
    }

    private void OnProgressMade()
    {
        SetShownState(_solver);
        UpdateLogs();
    }
    
    private void UpdateLogs()
    {
        if (_solver.Logs.Count < _logCount)
        {
            ClearLogs();
            return;
        }

        for (;_logCount < _solver.Logs.Count; _logCount++)
        {
            _view.AddLog(_solver.Logs[_logCount], _stateShown);
        }
    }
    
    public void RequestLogOpening(int id)
    {
        var index = id - 1;
        if (index < 0 || index > _solver.Logs.Count) return;
        
        _view.CloseLogs();

        if (_currentlyOpenedLog == index)
        {
            _currentlyOpenedLog = -1;
            SetShownState(_solver);
        }
        else
        {
            _view.OpenLog(index);
            _currentlyOpenedLog = index;

            var log = _solver.Logs[index];
            SetShownState(_stateShown == StateShown.Before ? log.StateBefore : log.StateAfter); 
            _translator.Translate(log.HighlightManager); 
        }
    }

    public void RequestStateShownChange(StateShown ss)
    {
        _stateShown = ss;
        _view.SetLogsStateShown(ss);
        if (_currentlyOpenedLog < 0 || _currentlyOpenedLog > _solver.Logs.Count) return;
        
        var log = _solver.Logs[_currentlyOpenedLog];
        SetShownState(_stateShown == StateShown.Before ? log.StateBefore : log.StateAfter); 
        _translator.Translate(log.HighlightManager);
    }

    public void RequestHighlightShift(bool isLeft)
    {
        if (_currentlyOpenedLog < 0 || _currentlyOpenedLog > _solver.Logs.Count) return;
        
        var log = _solver.Logs[_currentlyOpenedLog];
        if(isLeft) log.HighlightManager.ShiftLeft();
        else log.HighlightManager.ShiftRight();
        
        _view.Drawer.ClearHighlights();
        _translator.Translate(log.HighlightManager);
        _view.SetCursorPosition(_currentlyOpenedLog, log.HighlightManager.CursorPosition());
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
                    if (tectonic.MergeZones(_selectedZone, _solver.Tectonic.GetZone(c))) SetNewTectonic(tectonic);
                    
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
            if (tectonic.SplitZone(_selectedCells.ToArray())) SetNewTectonic(tectonic);
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

    #region Private

    private void ClearLogs()
    {
        _view.ClearLogs();
        _logCount = 0;
    }

    private void SetShownState(ISolvingState state)
    {
        var drawer = _view.Drawer;

        drawer.ClearNumbers();
        drawer.ClearHighlights();
        for (int row = 0; row < _solver.Tectonic.RowCount; row++)
        {
            for (int col = 0; col < _solver.Tectonic.ColumnCount; col++)
            {
                var number = state[row, col];
                if (number == 0)
                {
                    var zoneSize = _solver.Tectonic.GetZone(row, col).Count;
                    drawer.ShowPossibilities(row, col, state.PossibilitiesAt(row, col).Enumerate(1, zoneSize));
                }
                else drawer.ShowSolution(row, col, number);
            }
        }
        
        drawer.Refresh();
    }

    private void SetNewTectonic(ITectonic tectonic)
    {
        _solver.SetTectonic(tectonic);
        _view.Drawer.ClearHighlights();
        SetUpNewTectonic();
        SetShownState(_solver);
        ClearLogs();
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
    Default, Merge, Split
}