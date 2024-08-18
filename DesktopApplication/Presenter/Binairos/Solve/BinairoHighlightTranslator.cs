using Model.Core.Highlighting;

namespace DesktopApplication.Presenter.Binairos.Solve;

public class BinairoHighlightTranslator : IHighlighterTranslator<IBinairoHighlighter>
{
    private readonly IBinairoDrawer _drawer;

    public BinairoHighlightTranslator(IBinairoDrawer drawer)
    {
        _drawer = drawer;
    }

    public void Translate(IHighlightable<IBinairoHighlighter> highlightable, bool clear)
    {
        if(clear) _drawer.ClearHighlights();
        highlightable.Highlight(_drawer);
        _drawer.Refresh();
    }
}