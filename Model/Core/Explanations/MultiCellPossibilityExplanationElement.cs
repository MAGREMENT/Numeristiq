using Model.Core.Changes;
using Model.Core.Highlighting;
using Model.Utility;
using Model.Utility.Collections;

namespace Model.Core.Explanations;

public class MultiCellPossibilityExplanationElement : IExplanationElement<ISudokuHighlighter>
{
    private readonly CellPossibility[] _cps;

    public MultiCellPossibilityExplanationElement(params CellPossibility[] cps)
    {
        _cps = cps;
    }

    public override string ToString()
    {
        return _cps.ToStringSequence(", ");
    }
    
    public bool ShouldBeBold => true;
    public ExplanationColor Color => ExplanationColor.Primary;

    public void Highlight(ISudokuHighlighter highlighter)
    {
        foreach (var cp in _cps)
        {
            highlighter.HighlightPossibility(cp, StepColor.Cause1);
        }
    }

    public bool DoesShowSomething => true;
}