using Model.Core.Highlighting;

namespace DesktopApplication.Presenter;

public interface IHighlighterTranslator<T>
{
    void Translate(IHighlightable<T> highlightable);
}