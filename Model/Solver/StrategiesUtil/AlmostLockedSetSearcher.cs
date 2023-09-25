using System.Collections.Generic;
using Model.Solver.Possibilities;

namespace Model.Solver.StrategiesUtil;

public static class AlmostLockedSetSearcher //TODO do "in rows, cols, minis"
{
    public static List<AlmostLockedSet> InCells(IStrategyManager view, List<Cell> coords, int max)
    {
        List<AlmostLockedSet> result = new();
        if (max < 1) return result;
        
        var visited = new RecursionList<Cell>(coords.Count);
        for(int i = 0; i < coords.Count; i++){
            IReadOnlyPossibilities current = view.PossibilitiesAt(coords[i].Row, coords[i].Col);
            if (current.Count == 0) continue;

            if (current.Count == 2) result.Add(new AlmostLockedSet(coords[i], current));
            
            if (max > 1)
            {
                visited.SetCursorAndSetOrAdd(0, coords[i]);
                InCells(view, coords, visited, current, i + 1, max, result);
            }
        }

        return result;
    }

    private static void InCells(IStrategyManager view, List<Cell> coords, RecursionList<Cell> visited,
        IReadOnlyPossibilities current, int start, int max, List<AlmostLockedSet> result)
    {
        int next = visited.Cursor + 1;
        for (int i = start; i < coords.Count; i++)
        {
            visited.Cursor = next - 1;
            if (!coords[i].ShareAUnitWithAll(visited)) continue;

            var inspected = view.PossibilitiesAt(coords[i].Row, coords[i].Col);
            if(inspected.Count == 0 || !current.PeekAny(inspected)) continue;

            IPossibilities or = current.Or(inspected);
            visited.SetCursorAndSetOrAdd(next, coords[i]);

            if (or.Count == next + 2)
            {
                result.Add(new AlmostLockedSet(visited.CopyUntilCursor(), or));
            }

            if (max > next) InCells(view, coords, visited, or, i + 1, max, result);
        }
    }
    
    public static List<AlmostLockedSet> FullGrid(IStrategyManager view)
    {
        var result = new List<AlmostLockedSet>();
        var visited = new RecursionList<Cell>(7);

        for (int row = 0; row < 9; row++)
        {
            for (int col = 0; col < 9; col++)
            {
                if (view.Sudoku[row, col] != 0) continue;

                if (view.PossibilitiesAt(row, col).Count == 2)
                    result.Add(new AlmostLockedSet(new Cell(row, col), view.PossibilitiesAt(row, col)));

                visited.SetCursorAndSetOrAdd(0, new Cell(row, col));
                InRow(view, row, col + 1, view.PossibilitiesAt(row, col), visited, result);
            }
        }
        
        for (int col = 0; col < 9; col++)
        {
            for (int row = 0; row < 9; row++)
            {
                if (view.Sudoku[row, col] != 0) continue;

                visited.SetCursorAndSetOrAdd(0, new Cell(row, col));
                InColumn(view, col, row + 1, view.PossibilitiesAt(row, col), visited, result);
            }
        }

        for (int miniRow = 0; miniRow < 3; miniRow++)
        {
            for (int miniCol = 0; miniCol < 3; miniCol++)
            {
                for (int n = 0; n < 9; n++)
                {
                    int row = miniRow * 3 + n / 3;
                    int col = miniCol * 3 + n % 3;
                    if (view.Sudoku[row, col] != 0) continue;

                    visited.SetCursorAndSetOrAdd(0, new Cell(row, col));
                    InMiniGrid(view, miniRow, miniCol, n + 1, view.PossibilitiesAt(row, col), visited, result);
                }
            }
        }

        return result;
    }

    public static void InRow(IStrategyManager strategyManager, int row)
    {
        
    }
    
    private static void InRow(IStrategyManager strategyManager, int row, int start, IReadOnlyPossibilities current,
        RecursionList<Cell> visited, List<AlmostLockedSet> result)
    {
        int next = visited.Cursor + 1;
        for (int col = start; col < 9; col++)
        {
            var inspected = strategyManager.PossibilitiesAt(row, col);
            if (inspected.Count == 0 || !current.PeekAny(inspected)) continue;

            IPossibilities mashed = current.Or(inspected);
            visited.SetCursorAndSetOrAdd(next, new Cell(row, col));

            if (mashed.Count == next + 2)
            {
                result.Add(new AlmostLockedSet(visited.CopyUntilCursor(), mashed));
            }

            InRow(strategyManager, row, col + 1, mashed, visited, result);
        }
    }
    
    private static void InColumn(IStrategyManager strategyManager, int col, int start, IReadOnlyPossibilities current,
        RecursionList<Cell> visited, List<AlmostLockedSet> result)
    {
        int next = visited.Cursor + 1;
        for (int row = start; row < 9; row++)
        {
            var inspected = strategyManager.PossibilitiesAt(row, col);
            if (inspected.Count == 0 || !current.PeekAny(inspected)) continue;

            IPossibilities mashed = current.Or(inspected);
            visited.SetCursorAndSetOrAdd(next, new Cell(row, col));

            if (mashed.Count == next + 2)
            {
                result.Add(new AlmostLockedSet(visited.CopyUntilCursor(), mashed));
            }

            InColumn(strategyManager, col, row + 1, mashed, visited, result);
        }
    }

    private static void InMiniGrid(IStrategyManager strategyManager, int miniRow, int miniCol, int start,
        IReadOnlyPossibilities current, RecursionList<Cell> visited, List<AlmostLockedSet> result)
    {
        var next = visited.Cursor + 1;
        for (int n = start; n < 9; n++)
        {
            int row = miniRow * 3 + n / 3;
            int col = miniCol * 3 + n % 3;

            var inspected = strategyManager.PossibilitiesAt(row, col);
            if (inspected.Count == 0 || !current.PeekAny(inspected)) continue;

            IPossibilities mashed = current.Or(inspected);
            visited.SetCursorAndSetOrAdd(next, new Cell(row, col));

            if (mashed.Count == next + 2 && NotInSameRowOrColumn(visited))
            {
                result.Add(new AlmostLockedSet(visited.CopyUntilCursor(), mashed));
            }

            InMiniGrid(strategyManager, miniRow, miniCol, n + 1, mashed, visited, result);
        }
    }

    private static bool NotInSameRowOrColumn(RecursionList<Cell> cells)
    {
        int row = cells[0].Row;
        int col = cells[0].Col;

        bool rowOk = false;
        bool colOk = false;

        for (int i = 1; i <= cells.Cursor; i++)
        {
            if (!rowOk && cells[i].Row != row) rowOk = true;
            if (!colOk && cells[i].Col != col) colOk = true;

            if (rowOk && colOk) return true;
        }

        return false;
    }
}