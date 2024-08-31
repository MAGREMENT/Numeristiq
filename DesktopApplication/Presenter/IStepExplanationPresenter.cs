using Model.Core.Explanations;
using Model.Core.Steps;

namespace DesktopApplication.Presenter;

public interface IStepExplanationPresenter
{
    public void LoadStep();
    public void ShowExplanationElement(object element);
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
    private IExplanationElement<THighlight>? _currentlyShown;

    protected AbstractStepExplanationPresenter(IStepExplanationView view, TStep numericStep,
        IHighlighterTranslator<THighlight> translator)
    {
        _view = view;
        _numericStep = numericStep;
        _translator = translator;
    }

    public abstract void LoadStep();

    public void ShowExplanationElement(object element)
    {
        if (element is not IExplanationElement<THighlight> e) return;
        
        var drawer = _view.GetDrawer<IDrawer>();
        _currentlyShown = e;
        
        drawer.ClearHighlights();
        _translator.Translate(_currentlyShown, false);
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
        _showHighlight = false;
        if(_currentlyShown is not null) _translator.Translate(_currentlyShown, true);
    }

    public void TurnOnHighlight()
    {
        _showHighlight = true;
        
        if(_currentlyShown is not null) _translator.Translate(_currentlyShown, true);
        _translator.Translate(_numericStep.HighlightManager, false);
    }
}

public interface IStepExplanationView
{
    T GetDrawer<T>() where T : IDrawer;
    public void ShowExplanation<T>(Explanation<T> e);
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