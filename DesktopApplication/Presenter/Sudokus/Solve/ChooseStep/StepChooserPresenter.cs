using System.Collections.Generic;
using Model.Core;
using Model.Core.Changes;
using Model.Core.Highlighting;
using Model.Utility.BitSets;

namespace DesktopApplication.Presenter.Sudokus.Solve.ChooseStep;

public class StepChooserPresenter
{
    private const int PageCount = 20;
    
    private readonly IReadOnlyList<BuiltChangeCommit<NumericChange, ISudokuHighlighter>> _commits;
    private readonly IStepChooserView _view;
    private readonly SudokuHighlighterTranslator _translator;
    private readonly INumericSolvingState _currentState;
    private readonly ICommitApplier _applier;

    private int _currentPage;
    private int _shownStep = -1;
    
    public StepChooserPresenter(IStepChooserView view, INumericSolvingState currentState,
        IReadOnlyList<BuiltChangeCommit<NumericChange, ISudokuHighlighter>> commits, ICommitApplier applier, Settings settings)
    {
        _view = view;
        _currentState = currentState;
        _commits = commits;
        _applier = applier;
        _translator = new SudokuHighlighterTranslator(_view.Drawer, settings);
    }

    public void Initialize()
    {
        SetSteps();

        _view.SetTotalPage(_commits.Count / PageCount + (_commits.Count % PageCount > 0 ? 1 : 0));
        _view.SetCurrentPage(_currentPage + 1);

        var drawer = _view.Drawer;
        for (int row = 0; row < 9; row++)
        {
            for (int col = 0; col < 9; col++)
            {
                var number = _currentState[row, col];
                if (number == 0) drawer.ShowPossibilities(row, col,
                        _currentState.PossibilitiesAt(row, col).EnumeratePossibilities());
                else drawer.ShowSolution(row, col, number);
            }
        }
        
        drawer.Refresh();
    }

    public void ChangePage(int newPage)
    {
        var p = newPage - 1;
        if (p < 0 || p * PageCount > _commits.Count) return;

        _currentPage = p;
        _view.ClearSteps();
        _shownStep = -1;
        SetSteps();
    }

    public void ShowStep(int index)
    {
        if(_shownStep != -1) _view.CloseStep(_shownStep);

        if (_shownStep == index)
        {
            _shownStep = -1;
            _view.SetSelectionAvailability(false);
            _view.Drawer.ClearHighlights();
            _view.Drawer.Refresh();
        }
        else
        {
            var actual = index - _currentPage * PageCount;
            _view.OpenStep(actual);
            _shownStep = actual;

            _view.SetSelectionAvailability(true);
            _translator.Translate(_commits[index].Report.HighlightCollection, true); 
        }
    }

    public void SelectCurrent()
    {
        if (_shownStep == -1) return;
        _applier.Apply(_commits[_shownStep]);
    }

    private void SetSteps()
    {
        var start = _currentPage * PageCount;
        for (int i = start; i < start + PageCount && i < _commits.Count; i++)
        {
            _view.AddStep(i, _commits[i]);
        }
    }
}