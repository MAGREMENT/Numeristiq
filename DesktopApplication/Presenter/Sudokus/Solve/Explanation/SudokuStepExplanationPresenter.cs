using Model.Core;
using Model.Core.Explanation;
using Model.Core.Highlighting;
using Model.Core.Steps;
using Model.Utility.BitSets;

namespace DesktopApplication.Presenter.Sudokus.Solve.Explanation;

public class SudokuStepExplanationPresenter : AbstractStepExplanationPresenter<ISudokuHighlighter, 
    INumericStep<ISudokuHighlighter>, INumericSolvingState>
{ 
    public SudokuStepExplanationPresenter(IStepExplanationView view,
        INumericStep<ISudokuHighlighter> numericStep, Settings settings) : base(view, numericStep,
        new SudokuHighlighterTranslator(view.GetDrawer<ISudokuSolverDrawer>(), settings))
    {
    }

    public override void LoadStep()
    {
        var state = _numericStep.From;
        var drawer = _view.GetDrawer<ISudokuSolverDrawer>();
        
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
}

public interface IStepExplanationPresenter
{
    public void LoadStep();
    public void ShowExplanationElement(ExplanationElement element);
    public void StopShowingExplanationElement();
    public void TurnOffHighlight();
    public void TurnOnHighlight();
}

public abstract class AbstractStepExplanationPresenter<THighlight, TStep, TState> : IStepExplanationPresenter
    where TStep : IStep<THighlight, TState> 
{
    protected readonly IStepExplanationView _view;
    protected readonly TStep _numericStep;
    protected readonly IHighlighterTranslator<THighlight> _translator;

    private bool _showHighlight = true;
    private ExplanationElement? _currentlyShown;

    protected AbstractStepExplanationPresenter(IStepExplanationView view, TStep numericStep,
        IHighlighterTranslator<THighlight> translator)
    {
        _view = view;
        _numericStep = numericStep;
        _translator = translator;
    }

    public abstract void LoadStep();

    public void ShowExplanationElement(ExplanationElement element)
    {
        var drawer = _view.GetDrawer<IDrawer>();
        _currentlyShown = element;
        
        drawer.ClearHighlights();
        _currentlyShown.Show(_view.ExplanationHighlighter);
        if(_showHighlight) _translator.Translate(_numericStep.HighlightManager, false);
        else drawer.Refresh();
    }

    public void StopShowingExplanationElement()
    {
        var drawer = _view.GetDrawer<IDrawer>();
        _currentlyShown = null;
        
        drawer.ClearHighlights();
        if(_showHighlight) _translator.Translate(_numericStep.HighlightManager, false);
        else drawer.Refresh();
    }

    public void TurnOffHighlight()
    {
        var drawer = _view.GetDrawer<IDrawer>();
        _showHighlight = false;
        
        drawer.ClearHighlights();
        _currentlyShown?.Show(_view.ExplanationHighlighter);
        drawer.Refresh();
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