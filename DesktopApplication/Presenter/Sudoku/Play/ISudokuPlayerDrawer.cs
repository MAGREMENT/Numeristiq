using System.Collections.Generic;
using Model.Sudoku.Player;
using Model.Utility;

namespace DesktopApplication.Presenter.Sudoku.Play;

public interface ISudokuPlayerDrawer : ISudokuDrawer
{
    void PutCursorOn(HashSet<Cell> cells);
    void ShowLinePossibilities(int row, int col, IEnumerable<int> possibilities, PossibilitiesLocation location);
    void FillCell(int row, int col, double startAngle, int rotationFactor, params HighlightColor[] colors);
}