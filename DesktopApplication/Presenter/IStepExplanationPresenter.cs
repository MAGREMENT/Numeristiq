using Model.Core.Explanation;
using Model.Core.Steps;

namespace DesktopApplication.Presenter;

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

public interface IStepExplanationView
{
    T GetDrawer<T>() where T : IDrawer;
    public IExplanationHighlighter ExplanationHighlighter { get; }
    public void ShowExplanation(ExplanationElement? start);
}

public interface IDrawer
{
    void ClearHighlights();
    void Refresh();
}

public interface IStepExplanationPresenterBuilder
{
    public IStepExplanationPresenter Build(IStepExplanationView view);
}