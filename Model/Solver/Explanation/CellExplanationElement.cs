using Global;

namespace Model.Solver.Explanation;

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

    public override void Show(IExplanationShower shower)
    {
        shower.ShowCell(_cell);
    }
}