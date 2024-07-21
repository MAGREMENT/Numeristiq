namespace DesktopApplication.Presenter;

public abstract class SolveWithStepsPresenter<THighlight>
{
    private readonly IHighlighterTranslator<THighlight> _translator;

    protected SolveWithStepsPresenter(IHighlighterTranslator<THighlight> translator)
    {
        _translator = translator;
    }
    
    //TODO
}