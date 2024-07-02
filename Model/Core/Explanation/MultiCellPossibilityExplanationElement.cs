using System.Text;
using Model.Utility;

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
        if (_cps.Length == 0) return "";

        var builder = new StringBuilder(_cps[0].ToString());
        for (int i = 1; i < _cps.Length; i++)
        {
            builder.Append($", {_cps[i].ToString()}");
        }

        return builder.ToString();
    }
    
    public override bool ShouldBeBold => true;
    public override ExplanationColor Color => ExplanationColor.Primary;

    public override void Show(IExplanationHighlighter highlighter)
    {
        foreach (var cp in _cps)
        {
            highlighter.ShowCellPossibility(cp);
        }
    }

    public override bool DoesShowSomething => true;
}