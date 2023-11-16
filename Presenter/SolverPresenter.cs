using Global;
using Global.Enums;
using Model;
using Model.Solver;
using Presenter.Translator;

namespace Presenter;

//TODO : highlighting + by hand methods
public class SolverPresenter
{
    private readonly ISolver _solver;
    private readonly ISolverView _view;

    private bool _bound;
    private SolverState _shownState;
    private int _lastLogIndex;

    public SolverSettings Settings { get; }

    private SolverPresenter(ISolver solver, ISolverView view)
    {
        _solver = solver;
        _view = view;

        _shownState = _solver.CurrentState;

        Settings = new SolverSettings();
        Settings.TranslationTypeChanged += () =>
            _view.SetTranslation(SudokuTranslator.Translate(_shownState, Settings.TranslationType));
        Settings.UniquenessAllowedChanged += () =>
        {
            _solver.AllowUniqueness(Settings.UniquenessAllowed);
            _view.UpdateStrategies(ModelToViewTranslator.Translate(_solver.StrategyInfos));
        };
        Settings.OnInstanceFoundChanged += () => _solver.SetOnInstanceFound(Settings.OnInstanceFound);
        Settings.GivensNeedUpdate += UpdateGivens;
    }

    public static SolverPresenter FromView(ISolverView view)
    {
        return new SolverPresenter(new SudokuSolver
        {
            LogsManaged = true,
            StatisticsTracked = false
        }, view);
    }

    public void Bind()
    {
        _bound = true;

        _solver.LogsUpdated += UpdateLogs;
        _solver.CurrentStrategyChanged += i => _view.LightUpStrategy(i);

        ChangeShownState(_shownState);
        _view.InitializeStrategies(ModelToViewTranslator.Translate(_solver.StrategyInfos));
    }

    public void NewSudokuFromString(string s)
    {
        NewSudoku(SudokuTranslator.Translate(s));
    }

    public void ClearSudoku()
    {
        NewSudoku(new Sudoku());
    }

    public void Solve()
    {
        _solver.SolveAsync(Settings.StepByStep);
    }
    
    public void SelectLog(int number)
    {
        var logs = _solver.Logs;
        if (number < 0 || number >= logs.Count) return;

        var log = logs[number];
        _view.FocusLog(number);
        _view.ShowExplanation(log.Explanation);
        ChangeShownState(Settings.StateShown == StateShown.Before ? log.StateBefore : log.StateAfter);
    }

    public void ShowStartState()
    {
        ClearLogFocus();
        ChangeShownState(_solver.StartState);
    }

    public void ShowCurrentState()
    {
        ClearLogFocus();
        ChangeShownState(_solver.CurrentState);
    }

    public void UseStrategy(int number, bool yes)
    {
        if (yes) _solver.UseStrategy(number);
        else _solver.ExcludeStrategy(number);
    }
    
    //Private-----------------------------------------------------------------------------------------------------------

    private void ChangeShownState(SolverState state)
    {
        _shownState = state;
        if (!_bound) return;
        
        for (int row = 0; row < 9; row++)
        {
            for (int col = 0; col < 9; col++)
            {
                var current = state.At(row, col);
                if (current.IsPossibilities) _view.SetCellTo(row, col, current.AsPossibilities.ToArray());
                else _view.SetCellTo(row, col, current.AsNumber);
            }
        }
        
        _view.SetTranslation(SudokuTranslator.Translate(state, Settings.TranslationType));
    }

    private void NewSudoku(Sudoku sudoku)
    {
        _solver.SetSudoku(sudoku);
        if (_bound) return;
        
        ClearLogs();
        ClearLogFocus();
        ChangeShownState(_solver.CurrentState);
        UpdateGivens();
    }

    private void UpdateGivens()
    {
        HashSet<Cell> givens = new();
        for (int row = 0; row < 9; row++)
        {
            for (int col = 0; col < 9; col++)
            {
                if (_solver.Sudoku[row, col] != 0) givens.Add(new Cell(row, col));
            }
        }

        _view.UpdateGivens(givens);
    }

    private async void UpdateLogs()
    {
        var logs = _solver.Logs;
        _view.SetLogs(ModelToViewTranslator.Translate(logs));

        if (Settings.StepByStep) _lastLogIndex = logs.Count - 1;
        else
        {
            for (int i = _lastLogIndex + 1; i < logs.Count; i++)
            {
                _lastLogIndex = i;
                var current = logs[i];
                if (!current.FromSolving) continue;

                _view.ShowExplanation(current.Explanation);

                ChangeShownState(current.StateBefore);
                //TODO highligting

                await Task.Delay(TimeSpan.FromMilliseconds(Settings.DelayBeforeTransition));

                ChangeShownState(current.StateAfter);

                await Task.Delay(TimeSpan.FromMilliseconds(Settings.DelayAfterTransition));
            
                //TODO clear highlight
            }   
        }
        
        ChangeShownState(_solver.CurrentState);
    }

    private void ClearLogs()
    {
        _lastLogIndex = -1;
        _view.ClearLogs();
    }

    private void ClearLogFocus()
    {
        _view.UnFocusLog();
        _view.ShowExplanation("");
    }
}