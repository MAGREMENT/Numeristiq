using System.Threading.Tasks;
using Model;
using Model.Helpers;
using Model.Tectonic;

namespace DesktopApplication.Presenter.Tectonic.Solve;

public class TectonicSolvePresenter
{
    private readonly TectonicSolver _solver;
    private readonly ITectonicSolveView _view;

    private readonly TectonicHighlightTranslator _translator;
    
    private int _logCount;
    private int _currentlyOpenedLog = -1;
    private StateShown _stateShown = StateShown.Before;

    public TectonicSolvePresenter(TectonicSolver solver, ITectonicSolveView view)
    {
        _solver = solver;
        _view = view;

        _translator = new TectonicHighlightTranslator(_view.Drawer);
    }

    public void SetNewTectonic(string asString)
    {
        _solver.SetTectonic(TectonicTranslator.TranslateCodeFormat(asString));
        _view.Drawer.ClearHighlights();
        SetUpNewTectonic();
        SetShownState(_solver);
        ClearLogs();
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