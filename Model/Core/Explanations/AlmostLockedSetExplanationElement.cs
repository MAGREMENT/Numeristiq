using Model.Core.Changes;
using Model.Core.Highlighting;
using Model.Sudokus.Solver.PossibilitySets;

namespace Model.Core.Explanations;

public class AlmostLockedSetExplanationElement : IExplanationElement<ISudokuHighlighter>
{
    private readonly IPossibilitySet _als;

    public AlmostLockedSetExplanationElement(IPossibilitySet als)
    {
        _als = als;
    }

    public bool ShouldBeBold => true;
    public ExplanationColor Color => ExplanationColor.Primary;
    public bool DoesShowSomething => true;
    
    public void Highlight(ISudokuHighlighter highlighter)
    {
        foreach (var cp in _als.EnumeratePossibilities())
        {
            highlighter.HighlightPossibility(cp, StepColor.Cause1); 
        }
    }
    
    public override string ToString()
    {
        return _als.ToString()!;
    }
}