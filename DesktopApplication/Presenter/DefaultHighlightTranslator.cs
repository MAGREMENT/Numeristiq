using Model.Core.Highlighting;

namespace DesktopApplication.Presenter;

public class DefaultHighlightTranslator<TDrawer, THighlighter> : IHighlighterTranslator<THighlighter>
    where TDrawer : IDrawer, THighlighter
{
    private readonly TDrawer _drawer;

    public DefaultHighlightTranslator(TDrawer drawer)
    {
        _drawer = drawer;
    }

    public void Translate(IHighlightable<THighlighter> highlightable, bool clear)
    {
        if(clear) _drawer.ClearHighlights();
        highlightable.Highlight(_drawer);
        _drawer.Refresh();
    }
}