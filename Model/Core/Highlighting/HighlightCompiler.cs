namespace Model.Core.Highlighting;

public interface IHighlightCompiler<THighlighter>
{
    public IHighlightable<THighlighter> Compile(IHighlightable<THighlighter> d);
}