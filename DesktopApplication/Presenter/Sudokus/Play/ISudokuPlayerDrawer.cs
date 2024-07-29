using System.Collections.Generic;
using Model.Sudokus.Player;
using Model.Utility;
using Model.Utility.Collections;

namespace DesktopApplication.Presenter.Sudokus.Play;

public interface ISudokuPlayerDrawer : ISudokuDrawer
{
    double StartAngle { set; }
    int RotationFactor { set; }
    
    void PutCursorOn(IContainingEnumerable<Cell> cells);
    void ShowLinePossibilities(int row, int col, IEnumerable<int> possibilities, PossibilitiesLocation location,
        IEnumerable<(int, HighlightColor)> colors);
    void ShowLinePossibilities(int row, int col, IEnumerable<int> possibilities, PossibilitiesLocation location,
        IEnumerable<(int, HighlightColor)> colors, int outlinePossibility);
    void FillCell(int row, int col, params HighlightColor[] colors);
}