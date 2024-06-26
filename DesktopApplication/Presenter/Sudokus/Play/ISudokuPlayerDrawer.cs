﻿using System.Collections.Generic;
using Model.Sudokus.Player;
using Model.Utility;

namespace DesktopApplication.Presenter.Sudokus.Play;

public interface ISudokuPlayerDrawer : ISudokuDrawer
{
    void PutCursorOn(HashSet<Cell> cells);
    void ShowLinePossibilities(int row, int col, IEnumerable<int> possibilities, PossibilitiesLocation location,
        IEnumerable<(int, HighlightColor)> colors);
    void ShowLinePossibilities(int row, int col, IEnumerable<int> possibilities, PossibilitiesLocation location,
        IEnumerable<(int, HighlightColor)> colors, int outlinePossibility);
    void FillCell(int row, int col, double startAngle, int rotationFactor, params HighlightColor[] colors);
}