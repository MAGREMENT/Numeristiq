using Model.Core.Changes;
using Model.Core.Highlighting;
using Model.Utility;

namespace Model.Core.Explanations;

public class CellPossibilityExplanationElement : IExplanationElement<ISudokuHighlighter>
{
    private readonly CellPossibility _cellPossibility;

    public CellPossibilityExplanationElement(CellPossibility cellPossibility)
    {
        _cellPossibility = cellPossibility;
    }

    public override string ToString()
    {
        return _cellPossibility.ToString();
    }
    
    public bool ShouldBeBold => true;
    public ExplanationColor Color => ExplanationColor.Primary;

    public void Highlight(ISudokuHighlighter highlighter)
    {
        highlighter.HighlightPossibility(_cellPossibility, StepColor.Cause1);
    }

    public bool DoesShowSomething => true;
}