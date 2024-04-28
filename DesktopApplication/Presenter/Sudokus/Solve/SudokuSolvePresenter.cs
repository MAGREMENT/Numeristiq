using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DesktopApplication.Presenter.Sudokus.Solve.ChooseStep;
using DesktopApplication.Presenter.Sudokus.Solve.Explanation;
using Model.Helpers;
using Model.Helpers.Changes;
using Model.Helpers.Highlighting;
using Model.Helpers.Settings;
using Model.Helpers.Steps;
using Model.Sudokus;
using Model.Sudokus.Solver;
using Model.Sudokus.Solver.Trackers;
using Model.Utility;
using Model.Utility.BitSets;

namespace DesktopApplication.Presenter.Sudokus.Solve;

public class SudokuSolvePresenter : ICommitApplier
{
    private readonly ISudokuSolveView _view;
    private readonly SolveActionEnabler _enabler;
    private readonly SudokuHighlighterTranslator _translator;
    private readonly Settings _settings;
    private readonly IStrategyRepositoryUpdater _updater;
    private readonly SudokuSolver _solver;
    
    private ISolvingState? _currentlyDisplayedState;
    private int _currentlyOpenedStep = -1;
    private Cell? _selectedCell;
    private UIUpdaterTracker? _solveTracker;

    private int _stepCount;
    private StateShown _stateShown = StateShown.Before;
    
    public SettingsPresenter SettingsPresenter { get; }

    public SudokuSolvePresenter(ISudokuSolveView view, SudokuSolver solver, Settings settings, IStrategyRepositoryUpdater updater)
    {
        _view = view;
        _enabler = new SolveActionEnabler(_view);
        _translator = new SudokuHighlighterTranslator(_view.Drawer, settings);
        _solver = solver;
        _settings = settings;
        _updater = updater;

        _settings.AddEvent(SpecificSettings.LinkOffsetSidePriority, _ => RedrawBoard());
        _settings.AddEvent(SpecificSettings.ShowSameCellLinks, _ => RedrawBoard());
        _settings.AddEvent(SpecificSettings.AllowUniqueness, AllowUniqueness);
        
        SettingsPresenter = new SettingsPresenter(_settings, SettingCollections.SudokuSolvePage);

        App.Current.ThemeInformation.ThemeChanged += () =>
        {
            if (_currentlyOpenedStep == -1) return;
            
            _view.Drawer.ClearHighlights();
            _translator.Translate(_solver.StepHistory.Steps[_currentlyOpenedStep].HighlightManager);
            _view.Drawer.Refresh();
        };
    }

    public void OnSudokuAsStringBoxShowed()
    {
        _view.SetSudokuAsString(SudokuTranslator.TranslateLineFormat(_solver.Sudoku, SudokuLineFormatEmptyCellRepresentation.Shortcuts));
    }

    public void SetNewSudoku(string s)
    {
        SetNewSudoku(SudokuTranslator.TranslateLineFormat(s));
    }

    public async void Solve(bool stopAtProgress)
    {
        _enabler.DisableActions(1);
        
        _solveTracker ??= new UIUpdaterTracker(this, true);
        _solveTracker.UpdateLogs = true;
        _solver.AddTracker(_solveTracker);
        await Task.Run(() => _solver.Solve(stopAtProgress));
        _solver.RemoveTracker(_solveTracker);

        _enabler.EnableActions(1);
    }

    public async Task<ChooseStepPresenterBuilder> ChooseStep()
    {
        _enabler.DisableActions(2);
        
        _solveTracker ??= new UIUpdaterTracker(this, false);
        _solveTracker.UpdateLogs = false;
        var commits = await Task.Run(() => _solver.EveryPossibleNextStep());
        _solver.RemoveTracker(_solveTracker);

        return new ChooseStepPresenterBuilder(commits, _settings, _solver, this);
    }

    public void OnStoppedChoosingStep()
    {
        _enabler.EnableActions(2);
    }

    public void Clear()
    {
        _solver.SetSudoku(new Sudoku());
        SetShownState(_solver.StartState, true, false);
        ClearLogs();
    }

    public void ShowCurrentState()
    {
        SetShownState(_solver, false, true);
    }

    public void UpdateLogs()
    {
        if (_solver.StepHistory.Steps.Count < _stepCount)
        {
            ClearLogs();
            return;
        }

        for (;_stepCount < _solver.StepHistory.Steps.Count; _stepCount++)
        {
            _view.AddLog(_solver.StepHistory.Steps[_stepCount], _stateShown);
        }
    }

    public void RequestLogOpening(int id)
    {
        var index = id - 1;
        if (index < 0 || index >= _solver.StepHistory.Steps.Count) return;
        
        if(_currentlyOpenedStep != -1) _view.CloseLog(_currentlyOpenedStep);

        if (_currentlyOpenedStep == index)
        {
            _currentlyOpenedStep = -1;
            SetShownState(_solver, false, true);
        }
        else
        {
            _view.OpenLog(index);
            _currentlyOpenedStep = index;

            var log = _solver.StepHistory.Steps[index];
            SetShownState(_stateShown == StateShown.Before ? log.From : log.To, false, true); 
            _translator.Translate(log.HighlightManager); 
        }
    }

    public void RequestStateShownChange(StateShown ss)
    {
        _stateShown = ss;
        _view.SetLogsStateShown(ss);
        if (_currentlyOpenedStep < 0 || _currentlyOpenedStep >= _solver.StepHistory.Steps.Count) return;
        
        var log = _solver.StepHistory.Steps[_currentlyOpenedStep];
        SetShownState(_stateShown == StateShown.Before ? log.From : log.To, false, true); 
        _translator.Translate(log.HighlightManager);
    }

    public void RequestHighlightShift(bool isLeft)
    {
        if (_currentlyOpenedStep < 0 || _currentlyOpenedStep >= _solver.StepHistory.Steps.Count) return;
        
        var log = _solver.StepHistory.Steps[_currentlyOpenedStep];
        if(isLeft) log.HighlightManager.ShiftLeft();
        else log.HighlightManager.ShiftRight();
        
        _view.Drawer.ClearHighlights();
        _translator.Translate(log.HighlightManager);
        _view.SetCursorPosition(_currentlyOpenedStep, log.HighlightManager.CursorPosition());
    }
    
    public StepExplanationPresenterBuilder? RequestExplanation()
    {
        if (_currentlyOpenedStep < 0 || _currentlyOpenedStep >= _solver.StepHistory.Steps.Count) return null;

        return new StepExplanationPresenterBuilder(_solver.StepHistory.Steps[_currentlyOpenedStep], _settings);
    }

    public void EnableStrategy(int index, bool enabled)
    {
        if (index < 0 || index >= _solver.StrategyManager.Strategies.Count) return;

        _solver.StrategyManager.Strategies[index].Enabled = enabled;
        _updater.Update();
    }

    public void SelectCell(int row, int col)
    {
        var c = new Cell(row, col);
        if (_selectedCell is null || _selectedCell.Value != c)
        {
            _selectedCell = c;
            _view.Drawer.PutCursorOn(c);
            _view.Drawer.Refresh();
        }
        else
        {
            _selectedCell = null;
            _view.Drawer.ClearCursor();
            _view.Drawer.Refresh();
        }
    }

    public void SetCurrentCell(int n)
    {
        if (_selectedCell is null) return;
        
        var c = _selectedCell.Value;
        _solver.SetSolutionByHand(n, c.Row, c.Column);
        SetShownState(_solver, !_solver.StartedSolving, true);
        UpdateLogs();
    }

    public void DeleteCurrentCell()
    {
        if (_selectedCell is null) return;
        
        var c = _selectedCell.Value;
        _solver.RemoveSolutionByHand(c.Row, c.Column);
        SetShownState(_solver, !_solver.StartedSolving, true);
        UpdateLogs();
    }

    public void HighlightStrategy(int index)
    {
        _view.HighlightStrategy(index);
    }

    public void UnHighlightStrategy(int index)
    {
        _view.UnHighlightStrategy(index);
    }

    public void Copy()
    {
        if (_currentlyDisplayedState is null) return;
        
        if(!_settings.OpenCopyDialog) Copy(_currentlyDisplayedState, _settings.DefaultCopyFormat);
        else _view.OpenOptionDialog("Copy", i =>
        {
            Copy(_currentlyDisplayedState, (SudokuStringFormat)i);
        }, EnumConverter.ToStringArray<SudokuStringFormat>(SpaceConverter.Instance));
    }

    public void Paste(string s)
    {
        if(!_settings.OpenPasteDialog) Paste(s, _settings.DefaultPasteFormat);
        else _view.OpenOptionDialog("Paste", i =>
        {
            Paste(s, (SudokuStringFormat)i);
        }, EnumConverter.ToStringArray<SudokuStringFormat>(SpaceConverter.Instance));
    }
    
    public void Apply(BuiltChangeCommit<ISudokuHighlighter> commit)
    {
        _solver.ApplyCommit(commit);
        UpdateLogs();
    }

    public void OnShow()
    {
        _view.InitializeStrategies(_solver.StrategyManager.Strategies);
    }

    private void Copy(ISolvingState state, SudokuStringFormat format)
    {
        _view.CopyToClipBoard(format switch
        {
            SudokuStringFormat.Line => SudokuTranslator.TranslateLineFormat(state, _settings.EmptyCellRepresentation),
            SudokuStringFormat.Grid => SudokuTranslator.TranslateGridFormat(state),
            SudokuStringFormat.Base32 => SudokuTranslator.TranslateBase32Format(state, new AlphabeticalBase32Translator()),
            _ => throw new Exception()
        });
    }

    private void Paste(string s, SudokuStringFormat format)
    {
        switch (format)
        {
            case SudokuStringFormat.Line :
                SetNewSudoku(SudokuTranslator.TranslateLineFormat(s));
                break;
            case SudokuStringFormat.Grid :
                SetNewState(SudokuTranslator.TranslateGridFormat(s, _settings.SoloToGiven));
                break;
            case SudokuStringFormat.Base32 :
                SetNewState(SudokuTranslator.TranslateBase32Format(s, new AlphabeticalBase32Translator()));
                break;
        }
    }

    private void ClearLogs()
    {
        _view.ClearLogs();
        _stepCount = 0;
    }

    private void SetShownState(ISolvingState solvingState, bool solutionAsClues, bool showPossibilities)
    {
        _currentlyDisplayedState = solvingState;
        var drawer = _view.Drawer;
        
        drawer.ClearNumbers();
        drawer.ClearHighlights();
        for (int row = 0; row < 9; row++)
        {
            for (int col = 0; col < 9; col++)
            {
                var number = solvingState[row, col];
                if (number == 0)
                {
                    if(solutionAsClues) drawer.SetClue(row, col, false);
                    if(showPossibilities) drawer.ShowPossibilities(row, col, solvingState.PossibilitiesAt(row, col).EnumeratePossibilities());
                }
                else
                {
                    if (solutionAsClues) drawer.SetClue(row, col, true);
                    drawer.ShowSolution(row, col, number);
                }
            }
        }
        
        drawer.Refresh();
    }

    private void SetNewSudoku(Sudoku sudoku)
    {
        _solver.SetSudoku(sudoku);
        SetShownState(_solver, true, true);
        ClearLogs();
    }

    private void SetNewState(ISolvingState solvingState)
    {
        _solver.SetState(solvingState);
        SetShownState(_solver, true, true);
        ClearLogs();
    }

    private void RedrawBoard()
    {
        if (_currentlyDisplayedState is null) return;
        
        var drawer = _view.Drawer;
        drawer.ClearNumbers();
        drawer.ClearHighlights();
        
        for (int row = 0; row < 9; row++)
        {
            for (int col = 0; col < 9; col++)
            {
                var number = _currentlyDisplayedState[row, col];
                if (number == 0) drawer.ShowPossibilities(row, col, _currentlyDisplayedState
                    .PossibilitiesAt(row, col).EnumeratePossibilities());
                else drawer.ShowSolution(row, col, number);
            }
        }

        if (_currentlyOpenedStep != -1)
        {
            _translator.Translate(_solver.StepHistory.Steps[_currentlyOpenedStep].HighlightManager);
        }
        
        drawer.Refresh();
    }

    private void AllowUniqueness(SettingValue value)
    {
        var yes = value.ToBool();

        for (int i = 0; i < _solver.StrategyManager.Strategies.Count; i++)
        {
            var s = _solver.StrategyManager.Strategies[i];
            if (s.UniquenessDependency != UniquenessDependency.FullyDependent) continue;
            
            if (yes)
            {
                s.Locked = false;
                s.Enabled = true;
                _view.EnableStrategy(i, s.Enabled);
            }
            else
            {
                s.Enabled = false;
                s.Locked = true;
                _view.EnableStrategy(i, s.Enabled);
                _view.LockStrategy(i);
            }
        }
        
        _updater.Update();
    }
}

public class UIUpdaterTracker : Tracker
{
    private readonly SudokuSolvePresenter _presenter;
    
    public bool UpdateLogs { get; set; }

    public UIUpdaterTracker(SudokuSolvePresenter presenter, bool updateLogs)
    {
        _presenter = presenter;
        UpdateLogs = updateLogs;
    }

    public override void OnStrategyStart(SudokuStrategy strategy, int index)
    {
        _presenter.HighlightStrategy(index);
    }

    public override void OnStrategyEnd(SudokuStrategy strategy, int index, int solutionAdded, int possibilitiesRemoved)
    {
        _presenter.UnHighlightStrategy(index);
        if (UpdateLogs && solutionAdded + possibilitiesRemoved > 0)
        {
            _presenter.ShowCurrentState();
            _presenter.UpdateLogs();
        }
    }
}

public class ChooseStepPresenterBuilder
{
    private readonly IReadOnlyList<BuiltChangeCommit<ISudokuHighlighter>> _commits;
    private readonly ISolvingState _currentState;
    private readonly Settings _settings;
    private readonly ICommitApplier _applier;

    public ChooseStepPresenterBuilder(IReadOnlyList<BuiltChangeCommit<ISudokuHighlighter>> commits, Settings settings,
        ISolvingState currentState, ICommitApplier applier)
    {
        _commits = commits;
        _settings = settings;
        _currentState = currentState;
        _applier = applier;
    }

    public ChooseStepPresenter Build(IChooseStepView view)
    {
        return new ChooseStepPresenter(view, _currentState, _commits, _applier, _settings);
    }
}

public class StepExplanationPresenterBuilder
{
    private readonly ISolverStep<ISudokuHighlighter> _step;
    private readonly Settings _settings;

    public StepExplanationPresenterBuilder(ISolverStep<ISudokuHighlighter> step, Settings settings)
    {
        _step = step;
        _settings = settings;
    }

    public StepExplanationPresenter Build(IStepExplanationView view)
    {
        return new StepExplanationPresenter(view, _step, _settings);
    }
}

public interface ICommitApplier
{
    public void Apply(BuiltChangeCommit<ISudokuHighlighter> commit);
}