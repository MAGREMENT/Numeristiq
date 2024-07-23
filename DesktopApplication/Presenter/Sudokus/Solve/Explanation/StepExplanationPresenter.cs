using Model.Core.Explanation;
using Model.Core.Highlighting;
using Model.Core.Steps;
using Model.Utility.BitSets;

namespace DesktopApplication.Presenter.Sudokus.Solve.Explanation;

public class StepExplanationPresenter
{
    private readonly IStepExplanationView<ISudokuSolverDrawer> _view;
    private readonly INumericStep<ISudokuHighlighter> _numericStep;
    private readonly SudokuHighlighterTranslator _translator;

    private bool _showHighlight = true;
    private ExplanationElement? _currentlyShown;

    public StepExplanationPresenter(IStepExplanationView<ISudokuSolverDrawer> view, INumericStep<ISudokuHighlighter> numericStep, Settings _settings)
    {
        _view = view;
        _numericStep = numericStep;
        _translator = new SudokuHighlighterTranslator(view.Drawer, _settings);
    }

    public void LoadStep()
    {
        var state = _numericStep.From;
        
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

        _translator.Translate(_numericStep.HighlightManager, false);
        _view.ShowExplanation(_numericStep.Explanation);
    }

    public void ShowExplanationElement(ExplanationElement element)
    {
        _currentlyShown = element;
        
        _view.Drawer.ClearHighlights();
        _currentlyShown.Show(_view.ExplanationHighlighter);
        if(_showHighlight) _translator.Translate(_numericStep.HighlightManager, false);
        else _view.Drawer.Refresh();
    }

    public void StopShowingExplanationElement()
    {
        _currentlyShown = null;
        
        _view.Drawer.ClearHighlights();
        if(_showHighlight) _translator.Translate(_numericStep.HighlightManager, false);
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
        _translator.Translate(_numericStep.HighlightManager, false);
    }
}

//TODO
public abstract class AbstractStepExplanationPresenter<THighlight, TStep, TState, TDrawer> 
    where TStep : IStep<THighlight, TState> where TDrawer : IDrawer
{
    private readonly IStepExplanationView<TDrawer> _view;
    private readonly TStep _numericStep;
    private readonly IHighlighterTranslator<THighlight> _translator;

    private bool _showHighlight = true;
    private ExplanationElement? _currentlyShown;

    protected AbstractStepExplanationPresenter(IStepExplanationView<TDrawer> view, TStep numericStep, IHighlighterTranslator<THighlight> translator)
    {
        _view = view;
        _numericStep = numericStep;
        _translator = translator;
    }

    protected abstract void LoadStep();

    public void ShowExplanationElement(ExplanationElement element)
    {
        _currentlyShown = element;
        
        _view.Drawer.ClearHighlights();
        _currentlyShown.Show(_view.ExplanationHighlighter);
        if(_showHighlight) _translator.Translate(_numericStep.HighlightManager, false);
        else _view.Drawer.Refresh();
    }

    public void StopShowingExplanationElement()
    {
        _currentlyShown = null;
        
        _view.Drawer.ClearHighlights();
        if(_showHighlight) _translator.Translate(_numericStep.HighlightManager, false);
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
        _translator.Translate(_numericStep.HighlightManager, false);
    }
}

public interface IDrawer
{
    void ClearHighlights();
    void Refresh();
}