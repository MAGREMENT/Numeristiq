using Model.Helpers.Highlighting;
using Model.Helpers.Logs;
using Model.Sudokus.Solver.Explanation;

namespace DesktopApplication.Presenter.Sudokus.Solve.Explanation;

public class StepExplanationPresenter
{
    private readonly IStepExplanationView _view;
    private readonly ISolverLog<ISudokuHighlighter> _log;
    private readonly SudokuHighlighterTranslator _translator;

    private bool _showHighlight = true;
    private ExplanationElement? _currentlyShown;

    public StepExplanationPresenter(IStepExplanationView view, ISolverLog<ISudokuHighlighter> log, Settings _settings)
    {
        _view = view;
        _log = log;
        _translator = new SudokuHighlighterTranslator(view.Drawer, _settings);
    }

    public void Initialize()
    {
        var state = _log.StateBefore;
        
        var drawer = _view.Drawer;
        for (int row = 0; row < 9; row++)
        {
            for (int col = 0; col < 9; col++)
            {
                var number = state[row, col];
                if (number == 0) drawer.ShowPossibilities(row, col, state.PossibilitiesAt(row, col).EnumeratePossibilities());
                else drawer.ShowSolution(row, col, number);
            }
        }

        _translator.Translate(_log.HighlightManager);
        _view.ShowExplanation(_log.Explanation);
    }

    public void ShowExplanationElement(ExplanationElement element)
    {
        _currentlyShown = element;
        
        _view.Drawer.ClearHighlights();
        _currentlyShown.Show(_view.ExplanationHighlighter);
        if(_showHighlight) _translator.Translate(_log.HighlightManager);
        else _view.Drawer.Refresh();
    }

    public void StopShowingExplanationElement()
    {
        _currentlyShown = null;
        
        _view.Drawer.ClearHighlights();
        if(_showHighlight) _translator.Translate(_log.HighlightManager);
        else _view.Drawer.Refresh();
    }

    public void TurnOffHighlight()
    {
        _showHighlight = false;
        
        _view.Drawer.ClearHighlights();
        _currentlyShown?.Show(_view.ExplanationHighlighter);
        _view.Drawer.Refresh();
    }

    public void TurnOnHighlight()
    {
        _showHighlight = true;
        
        _currentlyShown?.Show(_view.ExplanationHighlighter);
        _translator.Translate(_log.HighlightManager);
    }
}