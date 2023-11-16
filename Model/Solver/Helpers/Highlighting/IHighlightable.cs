using Global;
using Global.Enums;
using Model.Solver.StrategiesUtil;
using Model.Solver.StrategiesUtil.LinkGraph;

namespace Model.Solver.Helpers.Highlighting;

public interface IHighlightable
{
    public void HighlightPossibility(int possibility, int row, int col, ChangeColoration coloration);

    public void HighlightPossibility(CellPossibility coord, ChangeColoration coloration)
    {
        HighlightPossibility(coord.Possibility, coord.Row, coord.Col, coloration);
    }

    public void CirclePossibility(int possibility, int row, int col);

    public void CirclePossibility(CellPossibility coord)
    {
        CirclePossibility(coord.Possibility, coord.Row, coord.Col);
    }

    public void HighlightCell(int row, int col, ChangeColoration coloration);

    public void HighlightCell(Cell coord, ChangeColoration coloration)
    {
        HighlightCell(coord.Row, coord.Col, coloration);
    }

    public void CircleCell(int row, int col);

    public void CircleCell(Cell coord)
    {
        CircleCell(coord.Row, coord.Col);
    }

    public void HighlightLinkGraphElement(ILinkGraphElement element, ChangeColoration coloration);

    public void CreateLink(CellPossibility from, CellPossibility to, LinkStrength linkStrength);

    public void CreateLink(ILinkGraphElement from, ILinkGraphElement to, LinkStrength linkStrength);
}
