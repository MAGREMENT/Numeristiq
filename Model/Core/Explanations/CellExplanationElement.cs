using Model.Core.Changes;
using Model.Core.Highlighting;
using Model.Utility;

namespace Model.Core.Explanations;

public class CellExplanationElement : IExplanationElement<ISudokuHighlighter>
{
    private readonly Cell _cell;

    public CellExplanationElement(Cell cell)
    {
        _cell = cell;
    }

    public override string ToString()
    {
        return _cell.ToString();
    }

    public bool ShouldBeBold => true;
    public ExplanationColor Color => ExplanationColor.Primary;

    public void Highlight(ISudokuHighlighter highlighter)
    {
        highlighter.HighlightCell(_cell, StepColor.Cause1);
    }

    public bool DoesShowSomething => true;
}