using System.Text;
using Model.Utility;
using Model.Utility.Collections;

namespace Model.Core.Explanation;

public class MultiCellPossibilityExplanationElement : ExplanationElement
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
    
    public override bool ShouldBeBold => true;
    public override ExplanationColor Color => ExplanationColor.Primary;

    public override void Show(IExplanationHighlighter highlighter)
    {
        foreach (var cp in _cps)
        {
            highlighter.ShowCellPossibility(cp, Color);
        }
    }

    public override bool DoesShowSomething => true;
}