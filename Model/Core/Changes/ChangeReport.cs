using Model.Core.Explanations;
using Model.Core.Highlighting;

namespace Model.Core.Changes;

public class ChangeReport<THighlighter>
{
    public string Description { get; }
    public Explanation<THighlighter> Explanation { get; }
    public HighlightCollection<THighlighter> HighlightCollection { get; }
    
    public ChangeReport(string description, Highlight<THighlighter> highlighter)
    {
        Description = description;
        HighlightCollection = new HighlightCollection<THighlighter>(highlighter);
        Explanation = new Explanation<THighlighter>();
    }
    
    public ChangeReport(string description, Highlight<THighlighter> highlighter, Explanation<THighlighter> explanation)
    {
        Description = description;
        HighlightCollection = new HighlightCollection<THighlighter>(highlighter);
        Explanation = explanation;
    }
    
    public ChangeReport(string description, params Highlight<THighlighter>[] highlighters)
    {
        Description = description;
        HighlightCollection = new HighlightCollection<THighlighter>(highlighters);
        Explanation = new Explanation<THighlighter>();
    }
    
    public ChangeReport(string description, Highlight<THighlighter> first, params Highlight<THighlighter>[] highlighters)
    {
        Description = description;

        var all = new Highlight<THighlighter>[highlighters.Length + 1];
        all[0] = first;

        for (int i = 0; i < highlighters.Length; i++)
        {
            all[i + 1] = highlighters[i];
        }
        
        HighlightCollection = new HighlightCollection<THighlighter>(all);
        Explanation = new Explanation<THighlighter>();
    }
}