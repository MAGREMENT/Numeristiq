using Model.Core.Highlighting;

namespace Model.Core.Explanations;

public class StringExplanationElement : IExplanationElement<ISudokuHighlighter>
{
    private readonly string _value;
    
    public StringExplanationElement(string value)
    {
        _value = value;
    }

    public void Highlight(ISudokuHighlighter highlighter)
    {
        
    }

    public override string ToString()
    {
        return _value;
    }
    
    public bool ShouldBeBold => false;
    public ExplanationColor Color => ExplanationColor.Text;
    public bool DoesShowSomething => false;
}