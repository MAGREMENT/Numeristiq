using Global;
using Global.Enums;

namespace Presenter.Translators;

public interface ISudokuDrawer
{
    void ClearNumbers();
    void SetCellTo(int row, int col, int number);
    void SetCellTo(int row, int col, int[] possibilities);
    public void UpdateBackground();
    public void ClearDrawings();
    public void FillPossibility(int row, int col, int possibility, ChangeColoration coloration);
    public void FillCell(int row, int col, ChangeColoration coloration);
    public void EncirclePossibility(int row, int col, int possibility);
    public void EncircleCell(int row, int col);
    public void EncircleRectangle(int rowFrom, int colFrom, int possibilityFrom, int rowTo, int colTo,
        int possibilityTo, ChangeColoration coloration);
    public void EncircleRectangle(int rowFrom, int colFrom, int rowTo, int colTo, ChangeColoration coloration);
    public void EncircleCellPatch(Cell[] cells, ChangeColoration coloration);
    public void CreateLink(int rowFrom, int colFrom, int possibilityFrom, int rowTo, int colTo, int possibilityTo,
        LinkStrength strength, LinkOffsetSidePriority priority);
}