using System.Threading.Tasks;
using Model.Core;
using Model.Nonograms;
using Model.Nonograms.Solver;
using Model.Nonograms.Solver.Strategies;

namespace DesktopApplication.Presenter.Nonograms.Solve;

public class NonogramSolvePresenter
{
    private readonly INonogramSolveView _view;
    private readonly NonogramSolver _solver;
    private readonly NonogramHighlightTranslator _translator;
    
    private int _stepCount;
    private StateShown _stateShown = StateShown.Before;
    private int _currentlyOpenedStep = -1;

    public NonogramSolvePresenter(INonogramSolveView view)
    {
        _view = view;
        _translator = new NonogramHighlightTranslator(_view.Drawer);
        _solver = new NonogramSolver();
        _solver.StrategyManager.AddStrategies(new PerfectSpaceStrategy(), new NotEnoughSpaceStrategy(),
            new BridgingStrategy(), new EdgeValueStrategy(), new ValueCompletionStrategy(), new ValueOverlayStrategy(),
            new UnreachableSquareStrategy());
    }

    public void SetNewNonogram(string s)
    {
        _solver.SetNonogram(NonogramTranslator.TranslateLineFormat(s));
        SetUpNewNonogram();
        ShowCurrentState();
        ClearLogs();
    }

    public void ShowNonogramAsString()
    {
        _view.ShowNonogramAsString(NonogramTranslator.TranslateLineFormat(_solver.Nonogram));
    }

    public async void Solve(bool stopAtProgress)
    {
        _solver.StrategyEnded += OnStrategyEnd;
        await Task.Run(() => _solver.Solve(stopAtProgress));
        _solver.StrategyEnded -= OnStrategyEnd;
    }
    
    public void RequestLogOpening(int id)
    {
        var index = id - 1;
        if (index < 0 || index > _solver.Steps.Count) return;
        
        _view.CloseLogs();

        if (_currentlyOpenedStep == index)
        {
            _currentlyOpenedStep = -1;
            ShowState(_solver);
        }
        else
        {
            _view.OpenLog(index);
            _currentlyOpenedStep = index;

            var log = _solver.Steps[index];
            ShowState(_stateShown == StateShown.Before ? log.From : log.To); 
            _translator.Translate(log.HighlightManager); 
        }
    }

    public void RequestStateShownChange(StateShown ss)
    {
        _stateShown = ss;
        _view.SetLogsStateShown(ss);
        if (_currentlyOpenedStep < 0 || _currentlyOpenedStep > _solver.Steps.Count) return;
        
        var log = _solver.Steps[_currentlyOpenedStep];
        ShowState(_stateShown == StateShown.Before ? log.From : log.To); 
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

    private void OnStrategyEnd(Strategy strategy, int index, int p, int s)
    {
        if (p + s == 0) return;
        
        ShowCurrentState();
        UpdateLogs();
    }
    
    private void ClearLogs()
    {
        _view.ClearLogs();
        _stepCount = 0;
    }
    
    private void UpdateLogs()
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

    private void SetUpNewNonogram()
    {
        var drawer = _view.Drawer;
        drawer.SetRows(_solver.Nonogram.HorizontalLineCollection);
        drawer.SetColumns(_solver.Nonogram.VerticalLineCollection);
        ShowCurrentState();
    }

    private void ShowCurrentState()
    {
        ShowState(_solver);
    }

    private void ShowState(IDichotomousSolvingState state)
    {
        var drawer = _view.Drawer;
        drawer.ClearSolutions();
        drawer.ClearUnavailable();
        drawer.ClearHighlights();
        
        for (int row = 0; row < state.RowCount; row++)
        {
            for (int col = 0; col < state.ColumnCount; col++)
            {
                if (state[row, col]) drawer.SetSolution(row, col);
                else if (!state.IsAvailable(row, col)) drawer.SetUnavailable(row, col);
            }
        }
        
        drawer.Refresh();
    }
}