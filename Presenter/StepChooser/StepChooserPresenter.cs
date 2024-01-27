using Model.SudokuSolving.Solver;
using Model.SudokuSolving.Solver.Helpers.Changes;
using Presenter.Translators;

namespace Presenter.StepChooser;

public class StepChooserPresenter
{
    private readonly SolverState _state;
    private readonly BuiltChangeCommit[] _commits;
    private readonly IStepChooserView _view;
    private readonly IStepChooserCallback _callback;

    private readonly HighlighterTranslator _highlighterTranslator;

    private int _currentlySelectedIndex = -1;

    public StepChooserPresenter(SolverState state, BuiltChangeCommit[] commits, IStepChooserView view, IStepChooserCallback callback)
    {
        _commits = commits;
        _view = view;
        _callback = callback;
        _state = state;

        _highlighterTranslator = new HighlighterTranslator(_view, callback.Settings);
    }

    public void Closed()
    {
        _callback.EnableActionsBack();
    }

    public void Bind()
    {
        _view.ClearNumbers();
        for (int row = 0; row < 9; row++)
        {
            for (int col = 0; col < 9; col++)
            {
                var c = _state.At(row, col);
                if (c.IsPossibilities)
                    _view.ShowPossibilities(row, col, c.AsPossibilities.ToArray(), _callback.GetCellColor(row, col));
                else _view.ShowSolution(row, col, c.AsNumber, _callback.GetCellColor(row, col));
            }
        }
        _view.Refresh();
        _view.ShowCommits(ModelToViewTranslator.Translate(_commits));
    }

    public void SelectCommit(int index)
    {
        if (index < 0 || index >= _commits.Length) return;

        _view.ClearDrawings();
        if (_currentlySelectedIndex != -1) _view.UnShowSelection(_currentlySelectedIndex);
        
        if (_currentlySelectedIndex == -1 || _currentlySelectedIndex != index)
        {
            _currentlySelectedIndex = index;
            _highlighterTranslator.Translate(_commits[index].Report.HighlightManager); 
            _view.ShowCommitInformation(ModelToViewTranslator.Translate(_commits[index]));
            _view.ShowSelection(index);
            _view.AllowChoosing(true);
        }
        else
        {
            _currentlySelectedIndex = -1;
            _view.StopShowingCommitInformation();
            _view.AllowChoosing(false);
        }

        _view.Refresh();
    }

    public void ShiftHighlighting(int shift)
    {
        if (_currentlySelectedIndex == 0) return;

        if (shift > 0)
        {
            _commits[_currentlySelectedIndex].Report.HighlightManager.ShiftRight();
        }
        else if (shift < 0)
        {
            _commits[_currentlySelectedIndex].Report.HighlightManager.ShiftLeft();
        }
        
        _view.ClearDrawings();
        _highlighterTranslator.Translate(_commits[_currentlySelectedIndex].Report.HighlightManager);
        _view.Refresh();
        
        _view.ShowCommitInformation(ModelToViewTranslator.Translate(_commits[_currentlySelectedIndex]));
    }

    public void SelectCurrentCommit()
    {
        if (_currentlySelectedIndex == -1) return;
        
        _callback.ApplyCommit(_commits[_currentlySelectedIndex]);
    }
}