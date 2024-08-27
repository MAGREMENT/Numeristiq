using System.Collections.Generic;
using Model.Sudokus.Solver.Position;
using Model.Utility;
using Model.Utility.BitSets;

namespace Model.Sudokus.Solver.Utility;

public static class SharedSeenAlgorithms
{
    public static IEnumerable<Cell> FullGridSharedSeenCells(int row1, int col1, int row2, int col2)
    {
        for (int row = 0; row < 9; row++)
        {
            for (int col = 0; col < 9; col++)
            {
                if ((row == row1 && col == col1) || (row == row2 && col == col2)) continue;
                
                if (SudokuUtility.ShareAUnit(row, col, row1, col1)
                    && SudokuUtility.ShareAUnit(row, col, row2, col2))
                {
                    yield return new Cell(row, col); 
                }
            }
        }
    }

    public static IEnumerable<Cell> FullGridSharedSeenEmptyCells(ISudokuSolverData solverData, int row1, int col1,
        int row2, int col2)
    {
        for (int row = 0; row < 9; row++)
        {
            for (int col = 0; col < 9; col++)
            {
                if (solverData.Sudoku[row, col] != 0 ||
                    (row == row1 && col == col1) || (row == row2 && col == col2)) continue;
                
                if (SudokuUtility.ShareAUnit(row, col, row1, col1)
                    && SudokuUtility.ShareAUnit(row, col, row2, col2))
                {
                    yield return new Cell(row, col);  
                }
            }
        }
    }
    
    public static IEnumerable<Cell> GridPositionsSharedSeenCells(int row1, int col1, int row2, int col2)
    {
        var one = new GridPositions();
        var two = new GridPositions();

        one.FillRow(row1);
        one.FillColumn(col1);
        one.FillMiniGrid(row1 / 3, col1 / 3);

        two.FillRow(row2);
        two.FillColumn(col2);
        two.FillMiniGrid(row2 / 3, col2 / 3);

        var and = one.And(two);
        and.Remove(row1, col1);
        and.Remove(row2, col2);

        return and;
    }
    
    public static IEnumerable<Cell> InCommonSharedSeenCells(int row1, int col1, int row2, int col2)
    {
        int miniRow1 = row1 / 3;
        int miniCol1 = col1 / 3;
        int miniRow2 = row2 / 3;
        int miniCol2 = col2 / 3;

        if (miniRow1 == miniRow2 && miniCol1 == miniCol2)
        {
            var startRow = miniRow1 * 3;
            var startCol = miniCol1 * 3;

            for (int gridRow = 0; gridRow < 3; gridRow++)
            {
                for (int gridCol = 0; gridCol < 3; gridCol++)
                {
                    var row = startRow + gridRow;
                    var col = startCol + gridCol;
                    
                    if((row == row1 && col == col1) || (row == row2 && col == col2)) continue;
                    yield return new Cell(row, col);
                }
            }

            if (row1 == row2)
            {
                for (int miniCol = 0; miniCol < 3; miniCol++)
                {
                    if(miniCol == miniCol1) continue;

                    var sc = miniCol * 3;
                    for (int gridCol = 0; gridCol < 3; gridCol++)
                    {
                        yield return new Cell(row1, sc + gridCol);
                    }
                }
            }

            if (col1 == col2)
            {
                for (int miniRow = 0; miniRow < 3; miniRow++)
                {
                    if(miniRow == miniRow1) continue;

                    var sr = miniRow * 3;
                    for (int gridRow = 0; gridRow < 3; gridRow++)
                    {
                        yield return new Cell(sr + gridRow, col1);
                    }
                }
            }
            
            yield break;
        }

        if (row1 == row2)
        {
            for (int col = 0; col < 9; col++)
            {
                if(col == col1 || col == col2) continue;

                yield return new Cell(row1, col);
            }
            
            yield break;
        }

        if (col1 == col2)
        {
            for (int row = 0; row < 9; row++)
            {
                if(row == row1 || row == row2) continue;

                yield return new Cell(row, col1);
            }

            yield break;
        }

        if (miniRow1 == miniRow2)
        {
            var start1 = miniCol1 * 3;
            var start2 = miniCol2 * 3;

            for (int gridCol = 0; gridCol < 3; gridCol++)
            {
                yield return new Cell(row2, start1 + gridCol);
                yield return new Cell(row1, start2 + gridCol);
            }

            yield break;
        }

        if (miniCol1 == miniCol2)
        {
            var start1 = miniRow1 * 3;
            var start2 = miniRow2 * 3;

            for (int gridRow = 0; gridRow < 3; gridRow++)
            {
                yield return new Cell(start1 + gridRow, col2);
                yield return new Cell(start2 + gridRow, col1);
            }

            yield break;
        }

        yield return new Cell(row1, col2);
        yield return new Cell(row2, col1);
    }

    public static IEnumerable<Cell> InCommonSharedSeenEmptyCells(ISudokuSolverData solverData, int row1, int col1,
        int row2, int col2)
    {
        int miniRow1 = row1 / 3;
        int miniCol1 = col1 / 3;
        int miniRow2 = row2 / 3;
        int miniCol2 = col2 / 3;

        if (miniRow1 == miniRow2 && miniCol1 == miniCol2)
        {
            var startRow = miniRow1 * 3;
            var startCol = miniCol1 * 3;

            for (int gridRow = 0; gridRow < 3; gridRow++)
            {
                for (int gridCol = 0; gridCol < 3; gridCol++)
                {
                    var row = startRow + gridRow;
                    var col = startCol + gridCol;
                    
                    if((row == row1 && col == col1) || (row == row2 && col == col2)) continue;
                    if(solverData.Sudoku[row, col] == 0) yield return new Cell(row, col);
                }
            }

            if (row1 == row2)
            {
                for (int miniCol = 0; miniCol < 3; miniCol++)
                {
                    if(miniCol == miniCol1) continue;

                    var sc = miniCol * 3;
                    for (int gridCol = 0; gridCol < 3; gridCol++)
                    {
                        if(solverData.Sudoku[row1, sc + gridCol] == 0) yield return new Cell(row1, sc + gridCol);
                    }
                }
            }

            if (col1 == col2)
            {
                for (int miniRow = 0; miniRow < 3; miniRow++)
                {
                    if(miniRow == miniRow1) continue;

                    var sr = miniRow * 3;
                    for (int gridRow = 0; gridRow < 3; gridRow++)
                    {
                        if(solverData.Sudoku[sr + gridRow, col1] == 0) yield return new Cell(sr + gridRow, col1);
                    }
                }
            }
            
            yield break;
        }

        if (row1 == row2)
        {
            for (int col = 0; col < 9; col++)
            {
                if(col == col1 || col == col2) continue;

                if(solverData.Sudoku[row1, col] == 0) yield return new Cell(row1, col);
            }
            
            yield break;
        }

        if (col1 == col2)
        {
            for (int row = 0; row < 9; row++)
            {
                if(row == row1 || row == row2) continue;

                if(solverData.Sudoku[row, col1] == 0) yield return new Cell(row, col1);
            }

            yield break;
        }

        if (miniRow1 == miniRow2)
        {
            var start1 = miniCol1 * 3;
            var start2 = miniCol2 * 3;

            for (int gridCol = 0; gridCol < 3; gridCol++)
            {
                if(solverData.Sudoku[row2, start1 + gridCol] == 0) yield return new Cell(row2, start1 + gridCol);
                if(solverData.Sudoku[row1, start2 + gridCol] == 0) yield return new Cell(row1, start2 + gridCol);
            }

            yield break;
        }

        if (miniCol1 == miniCol2)
        {
            var start1 = miniRow1 * 3;
            var start2 = miniRow2 * 3;

            for (int gridRow = 0; gridRow < 3; gridRow++)
            {
                if(solverData.Sudoku[start1 + gridRow, col2] == 0) yield return new Cell(start1 + gridRow, col2);
                if(solverData.Sudoku[start2 + gridRow, col1] == 0) yield return new Cell(start2 + gridRow, col1);
            }

            yield break;
        }

        if(solverData.Sudoku[row1, col2] == 0) yield return new Cell(row1, col2);
        if(solverData.Sudoku[row2, col1] == 0) yield return new Cell(row2, col1);
    }

    public static IEnumerable<CellPossibility> InCommonSharedSeenExistingPossibilities(ISudokuSolverData solverData,
        int row1, int col1, int pos1, int row2,
        int col2, int pos2)
    {
        if (pos1 == pos2)
        {
            foreach (var cell in InCommonSharedSeenCells(row1, col1, row2, col2))
            {
                if(solverData.PossibilitiesAt(cell).Contains(pos1)) yield return new CellPossibility(cell, pos1);
            }
        }
        else
        {
            if (row1 == row2 && col1 == col2)
            {
                foreach (var p in solverData.PossibilitiesAt(row1, col1).EnumeratePossibilities())
                {
                    if (p == pos1 || p == pos2) continue;
                    yield return new CellPossibility(row1, col1, p);
                }
            }
            else if(SudokuUtility.ShareAUnit(row1, col1, row2, col2))
            {
                if(solverData.PossibilitiesAt(row1, col1).Contains(pos2)) yield return new CellPossibility(row1, col1, pos2);
                if(solverData.PossibilitiesAt(row2, col2).Contains(pos1)) yield return new CellPossibility(row2, col2, pos1);
            }
        }
    }
    
    public static IEnumerable<Cell> SharedUnitSharedSeenCells(int row1, int col1, int row2, int col2)
    {
        for (int unit = 0; unit < 9; unit++)
        {
            if (unit != row1)
            {
                if (SudokuUtility.ShareAUnit(unit, col1, row2, col2) &&
                    !(unit == row2 && col1 == col2)) yield return new Cell(unit, col1);
            }

            if (unit != col1)
            {
                if (SudokuUtility.ShareAUnit(row1, unit, row2, col2) &&
                    !(row1 == row2 && unit == col2)) yield return new Cell(row1, unit);
            }
        }

        int startRow = row1 / 3 * 3;
        int startCol = col1 / 3 * 3;
        for (int gridRow = 0; gridRow < 3; gridRow++)
        {
            for (int gridCol = 0; gridCol < 3; gridCol++)
            {
                int row = startRow + gridRow;
                int col = startCol + gridCol;
                
                if(row == row1 || col == col1 || (row == row2 && col == col2)) continue;

                if (SudokuUtility.ShareAUnit(row, col, row2, col2)) yield return new Cell(row, col);
            }
        }
    }
}