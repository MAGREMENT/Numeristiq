using System.Collections.Generic;
using Model.Core;
using Model.Core.Changes;
using Model.Core.Highlighting;
using Model.Core.Steps;
using Model.Utility.BitSets;

namespace DesktopApplication.Presenter.Sudokus.Solve.ChooseStep;

public class StepChooserPresenter : StepManagingPresenter<ISudokuHighlighter, IStep<ISudokuHighlighter, INumericSolvingState>,
    INumericSolvingState>
{
    private const int PageCount = 20;
    
    private readonly IReadOnlyList<IStep<ISudokuHighlighter, ISudokuSolvingState, NumericChange>> _steps;
    private readonly IStepChooserView _view;
    private readonly INumericSolvingState _currentState;
    private readonly ICommitApplier<ISudokuHighlighter, ISudokuSolvingState, NumericChange> _applier;

    private int _currentPage;
    private int _shownStep = -1;
    
    protected override IReadOnlyList<IStep<ISudokuHighlighter, INumericSolvingState>> Steps => _steps;
    protected override IStepManagingView View => _view;
    
    public StepChooserPresenter(IStepChooserView view, 
        INumericSolvingState currentState,
        IReadOnlyList<IStep<ISudokuHighlighter, ISudokuSolvingState, NumericChange>> steps, 
        ICommitApplier<ISudokuHighlighter, ISudokuSolvingState, NumericChange> applier, 
        Settings settings) 
        : base(new SudokuHighlighterTranslator(view.Drawer, settings))
    {
        _view = view;
        _currentState = currentState;
        _steps = steps;
        _applier = applier;
    }

    public void Initialize()
    {
        UpdateSteps();

        _view.SetTotalPage(_steps.Count / PageCount + (_steps.Count % PageCount > 0 ? 1 : 0));
        _view.SetCurrentPage(_currentPage + 1);
    }

    public override int GetViewIndex(int ind)
    {
        return ind - _currentPage * PageCount;
    }

    public override void UpdateSteps()
    {
        for (; _stepCount < PageCount; _stepCount++)
        {
            var ind = _currentPage * PageCount + _stepCount;
            if (ind >= _steps.Count) break;
            
            _view.AddStep(ind + 1, _steps[ind], _stepState);
        }
    }

    public override void RequestStepOpening(int id)
    {
        base.RequestStepOpening(id);
        _view.SetSelectionAvailability(_currentlyOpenedStep != -1);
    }

    public void ChangePage(int newPage)
    {
        var p = newPage - 1;
        if (p < 0 || p * PageCount > _steps.Count) return;

        _currentPage = p;
        _shownStep = -1;
        ClearSteps();
        UpdateSteps();
    }

    public void SelectCurrent()
    {
        if (_shownStep == -1) return;
        _applier.Apply(_steps[_currentPage * PageCount + _shownStep]);
    }
    
    public override IStepExplanationPresenterBuilder? RequestExplanation()
    {
        return null; //TODO
    }

    protected override void SetShownState(INumericSolvingState numericSolvingState, bool solutionAsClues, bool showPossibilities)
    {
        var drawer = _view.Drawer;
        drawer.ClearNumbers();
        drawer.ClearHighlights();
        
        for (int row = 0; row < 9; row++)
        {
            for (int col = 0; col < 9; col++)
            {
                var number = numericSolvingState[row, col];
                if (number == 0) drawer.ShowPossibilities(row, col,
                    numericSolvingState.PossibilitiesAt(row, col).EnumeratePossibilities());
                else drawer.ShowSolution(row, col, number);
            }
        }
        
        drawer.Refresh();
    }

    protected override INumericSolvingState GetCurrentState()
    {
        return _currentState;
    }
}