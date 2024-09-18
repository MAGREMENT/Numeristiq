using System.Collections.Generic;
using Model.Core;
using Model.Core.Changes;
using Model.Core.Highlighting;
using Model.Utility.BitSets;

namespace DesktopApplication.Presenter.Sudokus.Solve.ChooseStep;

public class ChooseStepPresenter
{
    private const int PageCount = 20;
    
    private readonly IReadOnlyList<BuiltChangeCommit<NumericChange, ISudokuHighlighter>> _commits;
    private readonly IChooseStepView _view;
    private readonly SudokuHighlighterTranslator _translator;
    private readonly INumericSolvingState _currentState;
    private readonly ICommitApplier _applier;

    private int _currentPage;
    private int _shownStep = -1;
    
    public ChooseStepPresenter(IChooseStepView view, INumericSolvingState currentState,
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
        _view.ClearCommits();
        SetSteps();
    }

    public void ShowStep(int index) //TODO improve (with StepControl)
    {
        var drawer = _view.Drawer;
        drawer.ClearHighlights();
        if(_shownStep != -1) _view.UnselectStep(_shownStep % PageCount);

        if (_shownStep != index)
        {
            _shownStep = index;
            _translator.Translate(_commits[index].Report.HighlightCollection, false);
            _view.SelectStep(index % PageCount);
        }
        else _shownStep = -1;

        _view.EnableSelection(_shownStep != -1);
        drawer.Refresh();
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
            _view.AddCommit(_commits[i].Maker, i);
        }
    }
}