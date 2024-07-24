using Model.Sudokus.Solver.PossibilitySets;

namespace Model.Core.Explanation;

public class AlmostLockedSetExplanationElement : ExplanationElement
{
    private readonly IPossibilitySet _als;

    public AlmostLockedSetExplanationElement(IPossibilitySet als)
    {
        _als = als;
    }

    public override bool ShouldBeBold => true;
    public override ExplanationColor Color => ExplanationColor.Primary;
    public override void Show(IExplanationHighlighter highlighter)
    {
        foreach (var cp in _als.EnumeratePossibilities())
        {
            highlighter.ShowCellPossibility(cp, Color); 
        }
    }

    public override bool DoesShowSomething => true;
    public override string ToString()
    {
        return _als.ToString()!;
    }
}