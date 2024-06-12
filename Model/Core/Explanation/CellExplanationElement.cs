using Model.Utility;

namespace Model.Core.Explanation;

public class CellExplanationElement : ExplanationElement
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

    public override bool ShouldBeBold => true;
    public override ExplanationColor Color => ExplanationColor.Primary;

    public override void Show(IExplanationHighlighter highlighter)
    {
        highlighter.ShowCell(_cell);
    }

    public override bool DoesShowSomething => true;
}