using System.Collections.Generic;
using Model.Sudokus.Solver.PossibilitySets;
using Model.Utility;
using Model.Utility.BitSets;

namespace Model.Sudokus.Solver.Utility.AlmostLockedSets;

public static class AlmostNakedSetSearcher
{
    public static List<IPossibilitySet> InCells(ISudokuSolverData solverData, int maxSize, int difference, IReadOnlyList<Cell> coords)
    {
        List<IPossibilitySet> result = new();
        InCells(solverData, maxSize, difference, coords, new List<Cell>(), new ReadOnlyBitSet16(), 0, result);
        
        return result;
    }
    
    public static List<IPossibilitySet> FullGrid(ISudokuSolverData solverData, int maxSize, int difference)
    {
        var result = new List<IPossibilitySet>();
        var possibilities = new ReadOnlyBitSet16();
        var cells = new List<Cell>();

        for (int row = 0; row < 9; row++)
        {
            InRow(solverData, maxSize, difference, row, 0, possibilities, cells, result);
            possibilities = new ReadOnlyBitSet16();
            cells.Clear();
        }
        
        for (int col = 0; col < 9; col++)
        {
            InColumn(solverData, maxSize, difference, col, 0, possibilities, cells, result, true);
            possibilities = new ReadOnlyBitSet16();
            cells.Clear();
        }

        for (int miniRow = 0; miniRow < 3; miniRow++)
        {
            for (int miniCol = 0; miniCol < 3; miniCol++)
            {
                InMiniGrid(solverData, maxSize, difference, miniRow, miniCol, 0, possibilities, cells, result, true);
                possibilities = new ReadOnlyBitSet16();
                cells.Clear();
            }
        }

        return result;
    }

    public static List<IPossibilitySet> InRow(ISudokuSolverData solverData, int maxSize, int difference, int row)
    {
        var result = new List<IPossibilitySet>();
        InRow(solverData, maxSize, difference, row, 0, new ReadOnlyBitSet16(), new List<Cell>(), result);

        return result;
    }

    public static List<IPossibilitySet> InColumn(ISudokuSolverData solverData, int maxSize, int difference, int col)
    {
        var result = new List<IPossibilitySet>();
        InColumn(solverData, maxSize, difference, col, 0, new ReadOnlyBitSet16(), new List<Cell>(), result, false);

        return result;
    }

    public static List<IPossibilitySet> InMiniGrid(ISudokuSolverData solverData, int maxSize, int difference, int miniRow, int miniCol)
    {
        var result = new List<IPossibilitySet>();
        InMiniGrid(solverData, maxSize, difference, miniRow, miniCol, 0, new ReadOnlyBitSet16(), new List<Cell>(), result, false);

        return result;
    }
    
    private static void InCells(ISudokuSolverData solverData, int maxSize, int difference, IReadOnlyList<Cell> coords, List<Cell> visited,
        ReadOnlyBitSet16 current, int start, List<IPossibilitySet> result)
    {
        for (int i = start; i < coords.Count; i++)
        {
            if (!SudokuUtility.ShareAUnitWithAll(coords[i], visited)) continue;

            var inspected = solverData.PossibilitiesAt(coords[i].Row, coords[i].Column);
            if(inspected.Count == 0 || (current.Count != 0 && !current.ContainsAny(inspected))) continue;

            var or = current | inspected;
            visited.Add(coords[i]);

            if (or.Count == visited.Count + difference)
            {
                result.Add(new SnapshotPossibilitySet(visited.ToArray(), or, solverData.CurrentState));
            }

            if (maxSize > visited.Count) InCells(solverData, maxSize, difference, coords, visited, or, i + 1, result);
            
            visited.RemoveAt(visited.Count - 1);
        }
    }
    
    private static void InRow(ISudokuSolverData solverData, int maxSize, int difference, int row, int start, ReadOnlyBitSet16 current,
        List<Cell> visited, List<IPossibilitySet> result)
    {
        for (int col = start; col < 9; col++)
        {
            var inspected = solverData.PossibilitiesAt(row, col);
            if (inspected.Count == 0 || (current.Count != 0 && !current.ContainsAny(inspected))) continue;

            var mashed = current | inspected;
            visited.Add(new Cell(row, col));

            if (mashed.Count == visited.Count + difference)
            {
                result.Add(new SnapshotPossibilitySet(visited.ToArray(), mashed, solverData.CurrentState));
            }

            if(maxSize > visited.Count) InRow(solverData, maxSize, difference, row, col + 1, mashed, visited, result);
            
            visited.RemoveAt(visited.Count - 1);
        }
    }
    
    private static void InColumn(ISudokuSolverData solverData, int maxSize, int difference, int col, int start, ReadOnlyBitSet16 current,
        List<Cell> visited, List<IPossibilitySet> result, bool excludeSingles)
    {
        for (int row = start; row < 9; row++)
        {
            var inspected = solverData.PossibilitiesAt(row, col);
            if (inspected.Count == 0 || (current.Count != 0 && !current.ContainsAny(inspected))) continue;

            var mashed = current | inspected;
            visited.Add(new Cell(row, col));

            if (mashed.Count == visited.Count + difference && (!excludeSingles || visited.Count > 1))
            {
                result.Add(new SnapshotPossibilitySet(visited.ToArray(), mashed, solverData.CurrentState));
            }

            if(maxSize > visited.Count) InColumn(solverData, maxSize, difference, col, row + 1, mashed, visited, result, excludeSingles);
            
            visited.RemoveAt(visited.Count - 1);
        }
    }

    private static void InMiniGrid(ISudokuSolverData solverData, int maxSize, int difference, int miniRow, int miniCol, int start,
        ReadOnlyBitSet16 current, List<Cell> visited, List<IPossibilitySet> result, bool excludeSameLine)
    {
        for (int n = start; n < 9; n++)
        {
            int row = miniRow * 3 + n / 3;
            int col = miniCol * 3 + n % 3;

            var inspected = solverData.PossibilitiesAt(row, col);
            if (inspected.Count == 0 || (current.Count != 0 && !current.ContainsAny(inspected))) continue;

            var mashed = current | inspected;
            visited.Add(new Cell(row, col));

            if (mashed.Count == visited.Count + difference && (!excludeSameLine || NotInSameRowOrColumn(visited)))
            {
                result.Add(new SnapshotPossibilitySet(visited.ToArray(), mashed, solverData.CurrentState));
            }

            if(maxSize > visited.Count) InMiniGrid(solverData, maxSize, difference, miniRow, miniCol, n + 1, mashed, visited, result, excludeSameLine);
            
            visited.RemoveAt(visited.Count - 1);
        }
    }

    private static bool NotInSameRowOrColumn(List<Cell> cells)
    {
        if (cells.Count <= 1) return false;
        
        int row = cells[0].Row;
        int col = cells[0].Column;

        bool rowOk = false;
        bool colOk = false;

        for (int i = 1; i < cells.Count; i++)
        {
            if (!rowOk && cells[i].Row != row) rowOk = true;
            if (!colOk && cells[i].Column != col) colOk = true;

            if (rowOk && colOk) return true;
        }

        return false;
    }
}