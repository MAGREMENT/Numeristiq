using System.Collections.Generic;

namespace DesktopApplication.Presenter.Tectonic.Solve;

public interface ITectonicDrawer
{
    int RowCount { set; }
    int ColumnCount { set; }

    void Refresh();
    void ClearNumbers();
    void ShowSolution(int row, int column, int number);
    void ShowPossibilities(int row, int column, IEnumerable<int> possibilities, int zoneSize);
    void ClearBorderDefinitions();
    void AddBorderDefinition(int insideRow, int insideColumn, BorderDirection direction, bool isThin);
}

public enum BorderDirection
{
    Horizontal, Vertical
}