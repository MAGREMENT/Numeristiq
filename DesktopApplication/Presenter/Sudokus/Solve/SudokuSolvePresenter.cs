using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DesktopApplication.Presenter.Sudokus.Solve.ChooseStep;
using Model.Core;
using Model.Core.Changes;
using Model.Core.Highlighting;
using Model.Core.Settings;
using Model.Core.Steps;
using Model.Core.Trackers;
using Model.Repositories;
using Model.Sudokus;
using Model.Sudokus.Solver;
using Model.Utility;
using Model.Utility.BitSets;

namespace DesktopApplication.Presenter.Sudokus.Solve;

public class SudokuSolvePresenter : SolveWithStepsPresenter<ISudokuHighlighter, IStep<ISudokuHighlighter, INumericSolvingState>,
    INumericSolvingState>, ICommitApplier
{
    private readonly ISudokuSolveView _view;
    private readonly Disabler _disabler;
    private readonly Settings _settings;
    private readonly IStrategyRepository<SudokuStrategy> _repo;
    private readonly SudokuSolver _solver;
    
    private INumericSolvingState? _currentlyDisplayedState;
    private Cell? _selectedCell;
    private UIUpdaterTracker? _solveTracker;
    
    public SettingsPresenter SettingsPresenter { get; }
    protected override IReadOnlyList<IStep<ISudokuHighlighter, INumericSolvingState>> Steps => _solver.Steps;
    protected override ISolveWithStepsView View => _view;

    public SudokuSolvePresenter(ISudokuSolveView view, SudokuSolver solver, Settings settings,
        IStrategyRepository<SudokuStrategy> repo) : base(new SudokuHighlighterTranslator(view.Drawer, settings))
    {
        _view = view;
        _disabler = new Disabler(_view);
        _solver = solver;
        _settings = settings;
        _repo = repo;

        _view.Drawer.FastPossibilityDisplay = _settings.FastPossibilityDisplay.Get().ToBool();
        _view.Drawer.LinkOffsetSidePriority = (LinkOffsetSidePriority)_settings.LinkOffsetSidePriority.Get().ToInt();

        _settings.FastPossibilityDisplay.ValueChanged += v =>
        {
            _view.Drawer.FastPossibilityDisplay = v.ToBool();
            _view.Drawer.Refresh();
        };
        _settings.LinkOffsetSidePriority.ValueChanged += v =>
        {
            _view.Drawer.LinkOffsetSidePriority = (LinkOffsetSidePriority)v.ToInt();
            _view.Drawer.Refresh();
        };
        _settings.ShowSameCellLinks.ValueChanged += _ => _view.Drawer.Refresh();
        _settings.AllowUniqueness.ValueChanged += AllowUniqueness;
        
        SettingsPresenter = new SettingsPresenter(_settings, SettingCollections.SudokuSolvePage);

        App.Current.ThemeInformation.ThemeChanged += () =>
        {
            if (_currentlyOpenedStep == -1) return;
            
            _translator.Translate(_solver.Steps[_currentlyOpenedStep].HighlightManager, true);
            _view.Drawer.Refresh();
        };
    }

    public void OnSudokuAsStringBoxShowed()
    {
        _view.SetSudokuAsString(SudokuTranslator.TranslateLineFormat(_solver.Sudoku, 
            SudokuLineFormatEmptyCellRepresentation.Shortcuts));
    }

    public void SetNewSudoku(string s)
    {
        SetNewSudoku(SudokuTranslator.TranslateLineFormat(s));
    }

    public async void Solve(bool stopAtProgress)
    {
        _disabler.Disable(1);
        
        _solveTracker ??= new UIUpdaterTracker(this, true);
        _solveTracker.UpdateLogs = true;
        _solveTracker.AttachTo(_solver);
        await Task.Run(() => _solver.Solve(stopAtProgress));
        _solveTracker.Detach();

        _disabler.Enable(1);
    }

    public async Task<ChooseStepPresenterBuilder> ChooseStep()
    {
        _disabler.Disable(2);
        
        _solveTracker ??= new UIUpdaterTracker(this, false);
        _solveTracker.UpdateLogs = false;
        _solveTracker.AttachTo(_solver);
        var commits = await Task.Run(() => _solver.EveryPossibleNextStep());
        _solveTracker.Detach();

        return new ChooseStepPresenterBuilder(commits, _settings, _solver, this);
    }

    public void OnStoppedChoosingStep()
    {
        _disabler.Enable(2);
    }

    public void Clear()
    {
        _solver.SetSudoku(new Sudoku());
        SetShownState(_solver.StartState, true, false);
        ClearSteps();
    }

    public void ShowCurrentState()
    {
        SetShownState(_solver, false, true);
    }
    
    public override IStepExplanationPresenterBuilder? RequestExplanation()
    {
        if (_currentlyOpenedStep < 0 || _currentlyOpenedStep >= _solver.Steps.Count) return null;

        return new SudokuStepExplanationPresenterBuilder(_solver.Steps[_currentlyOpenedStep], _settings);
    }

    public void EnableStrategy(int index, bool enabled)
    {
        if (index < 0 || index >= _solver.StrategyManager.Strategies.Count) return;

        var strategy = _solver.StrategyManager.Strategies[index];
        strategy.Enabled = enabled;
        _repo.UpdateStrategy(strategy);
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
        UpdateSteps();
    }

    public void DeleteCurrentCell()
    {
        if (_selectedCell is null) return;
        
        var c = _selectedCell.Value;
        _solver.RemoveSolutionByHand(c.Row, c.Column);
        SetShownState(_solver, !_solver.StartedSolving, true);
        UpdateSteps();
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
        
        if(!_settings.OpenCopyDialog.Get().ToBool()) Copy(_currentlyDisplayedState, (SudokuStringFormat)
            _settings.DefaultCopyFormat.Get().ToInt());
        else _view.OpenOptionDialog("Copy", i =>
        {
            Copy(_currentlyDisplayedState, (SudokuStringFormat)i);
        }, EnumConverter.ToStringArray<SudokuStringFormat>(SpaceConverter.Instance));
    }

    public void Paste(string s)
    {
        if(!_settings.OpenPasteDialog.Get().ToBool()) Paste(s, (SudokuStringFormat)
            _settings.DefaultPasteFormat.Get().ToInt());
        else _view.OpenOptionDialog("Paste", i =>
        {
            Paste(s, (SudokuStringFormat)i);
        }, EnumConverter.ToStringArray<SudokuStringFormat>(SpaceConverter.Instance));
    }
    
    public void Apply(BuiltChangeCommit<NumericChange, ISudokuHighlighter> commit)
    {
        _solver.ApplyCommit(commit);
        ShowCurrentState();
        UpdateSteps();
    }

    public void OnShow()
    {
        _view.InitializeStrategies(_solver.StrategyManager.Strategies);
    }

    private void Copy(INumericSolvingState state, SudokuStringFormat format)
    {
        _view.CopyToClipBoard(format switch
        {
            SudokuStringFormat.Line => SudokuTranslator.TranslateLineFormat(state, (SudokuLineFormatEmptyCellRepresentation)
                _settings.EmptyCellRepresentation.Get().ToInt()),
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
                SetNewState(SudokuTranslator.TranslateGridFormat(s, _settings.SoloToGiven.Get().ToBool()));
                break;
            case SudokuStringFormat.Base32 :
                SetNewState(SudokuTranslator.TranslateBase32Format(s, new AlphabeticalBase32Translator()));
                break;
        }
    }

    protected override INumericSolvingState GetCurrentState() => _solver;

    protected override void SetShownState(INumericSolvingState numericSolvingState, bool solutionAsClues, bool showPossibilities)
    {
        _currentlyDisplayedState = numericSolvingState;
        var drawer = _view.Drawer;
        
        drawer.ClearNumbers();
        drawer.ClearHighlights();
        for (int row = 0; row < 9; row++)
        {
            for (int col = 0; col < 9; col++)
            {
                var number = numericSolvingState[row, col];
                if (number == 0)
                {
                    if(solutionAsClues) drawer.SetClue(row, col, false);
                    if(showPossibilities) drawer.ShowPossibilities(row, col, numericSolvingState.PossibilitiesAt(row, col).EnumeratePossibilities());
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
        ClearSteps();
    }

    private void SetNewState(INumericSolvingState numericSolvingState)
    {
        _solver.SetState(numericSolvingState);
        SetShownState(_solver, true, true);
        ClearSteps();
    }

    private void AllowUniqueness(SettingValue value)
    {
        var yes = value.ToBool();
        _solver.StrategyManager.AllowUniqueness(yes);
        
        for (int i = 0; i < _solver.StrategyManager.Strategies.Count; i++)
        {
            var s = _solver.StrategyManager.Strategies[i];
            _view.EnableStrategy(i, s.Enabled);
            if (s.Locked) _view.LockStrategy(i);
        }

        _repo.SetStrategies(_solver.StrategyManager.Strategies);
    }
}

public class UIUpdaterTracker : Tracker<object>
{
    private readonly SudokuSolvePresenter _presenter;
    
    public bool UpdateLogs { get; set; }

    public UIUpdaterTracker(SudokuSolvePresenter presenter, bool updateLogs)
    {
        _presenter = presenter;
        UpdateLogs = updateLogs;
    }
    
    protected override void OnAttach(ITrackerAttachable<object> attachable)
    {
        attachable.StrategyStarted += OnStrategyStart;
        attachable.StrategyEnded += OnStrategyEnd;
    }

    protected override void OnDetach(ITrackerAttachable<object> attachable)
    {
        attachable.StrategyStarted -= OnStrategyStart;
        attachable.StrategyEnded -= OnStrategyEnd;
    }

    private void OnStrategyStart(Strategy strategy, int index)
    {
        _presenter.HighlightStrategy(index);
    }

    private void OnStrategyEnd(Strategy strategy, int index, int solutionAdded, int possibilitiesRemoved)
    {
        _presenter.UnHighlightStrategy(index);
        if (UpdateLogs && solutionAdded + possibilitiesRemoved > 0)
        {
            _presenter.ShowCurrentState();
            _presenter.UpdateSteps();
        }
    }
}

public class ChooseStepPresenterBuilder
{
    private readonly IReadOnlyList<BuiltChangeCommit<NumericChange, ISudokuHighlighter>> _commits;
    private readonly INumericSolvingState _currentState;
    private readonly Settings _settings;
    private readonly ICommitApplier _applier;

    public ChooseStepPresenterBuilder(IReadOnlyList<BuiltChangeCommit<NumericChange, ISudokuHighlighter>> commits, Settings settings,
        INumericSolvingState currentState, ICommitApplier applier)
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

public interface ICommitApplier
{
    public void Apply(BuiltChangeCommit<NumericChange, ISudokuHighlighter> commit);
}