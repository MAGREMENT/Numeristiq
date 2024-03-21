using System.Collections.Generic;
using Model.Helpers;
using Model.Helpers.Changes;
using Model.Helpers.Highlighting;

namespace DesktopApplication.Presenter.Sudoku.Solve;

public class ChooseStepPresenter
{
    private const int PageCount = 20;
    
    private readonly IReadOnlyList<BuiltChangeCommit<ISudokuHighlighter>> _commits;
    private readonly IChooseStepView _view;
    private readonly SudokuHighlighterTranslator _translator;
    private readonly ISolvingState _currentState;

    private int _currentPage;
    private int _shownStep = -1;
    
    public ChooseStepPresenter(IChooseStepView view, ISolvingState currentState,
        IReadOnlyList<BuiltChangeCommit<ISudokuHighlighter>> commits, Settings settings)
    {
        _view = view;
        _currentState = currentState;
        _commits = commits;
        _translator = new SudokuHighlighterTranslator(_view.Drawer, settings);
    }

    public void Initialize()
    {
        SetSteps();

        _view.SetTotalPage(_commits.Count / PageCount + (_commits.Count % PageCount > 0 ? 1 : 0));
        _view.SetCurrentPage(_currentPage + 1);
        _view.SetPreviousPageExistence(false);
        _view.SetNextPageExistence(_commits.Count > PageCount);

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

    public void ChangePage(int diff)
    {
        var newPage = _currentPage + diff;
        if (newPage < 0 || newPage * PageCount >= _commits.Count) return;

        _currentPage = newPage;
        _view.ClearCommits();
        SetSteps();

        _view.SetCurrentPage(_currentPage + 1);
        _view.SetPreviousPageExistence(_currentPage > 0);
        _view.SetNextPageExistence(newPage * PageCount < _commits.Count - 1);
    }

    public void ShowStep(int index)
    {
        var drawer = _view.Drawer;
        drawer.ClearHighlights();

        if (_shownStep != index)
        {
            _shownStep = index;
            _translator.Translate(_commits[index].Report.HighlightManager);
        }
        else _shownStep = -1;
        
        drawer.Refresh();
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