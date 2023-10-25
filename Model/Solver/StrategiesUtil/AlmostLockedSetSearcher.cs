using System.Collections.Generic;
using Model.Solver.Possibilities;

namespace Model.Solver.StrategiesUtil;

public static class AlmostLockedSetSearcher
{
    public static List<AlmostLockedSet> InCells(IStrategyManager strategyManager, List<Cell> coords, int max = 9)
    {
        List<AlmostLockedSet> result = new();
        if (max < 1) return result;
        
        var visited = new List<Cell>(coords.Count);
        for(int i = 0; i < coords.Count; i++){
            IReadOnlyPossibilities current = strategyManager.PossibilitiesAt(coords[i].Row, coords[i].Col);
            if (current.Count == 0) continue;

            if (current.Count == 2) result.Add(new AlmostLockedSet(coords[i], current));
            
            if (max > 1)
            {
                visited.Add(coords[i]);
                InCells(strategyManager, coords, visited, current, i + 1, max, result);
                visited.RemoveAt(visited.Count - 1);
            }
        }

        return result;
    }

    private static void InCells(IStrategyManager strategyManager, List<Cell> coords, List<Cell> visited,
        IReadOnlyPossibilities current, int start, int max, List<AlmostLockedSet> result)
    {
        for (int i = start; i < coords.Count; i++)
        {
            if (!coords[i].ShareAUnitWithAll(visited)) continue;

            var inspected = strategyManager.PossibilitiesAt(coords[i].Row, coords[i].Col);
            if(inspected.Count == 0 || !current.PeekAny(inspected)) continue;

            IPossibilities or = current.Or(inspected);
            visited.Add(coords[i]);

            if (or.Count == visited.Count + 1)
            {
                result.Add(new AlmostLockedSet(visited.ToArray(), or));
            }

            if (max > visited.Count) InCells(strategyManager, coords, visited, or, i + 1, max, result);
            
            visited.RemoveAt(visited.Count - 1);
        }
    }
    
    public static List<AlmostLockedSet> FullGrid(IStrategyManager strategyManager, int max = 9)
    {
        var result = new List<AlmostLockedSet>();

        for (int row = 0; row < 9; row++)
        {
            result.AddRange(InRow(strategyManager, row));
        }
        
        for (int col = 0; col < 9; col++)
        {
            result.AddRange(InColumn(strategyManager, col));
        }

        for (int miniRow = 0; miniRow < 3; miniRow++)
        {
            for (int miniCol = 0; miniCol < 3; miniCol++)
            {
                result.AddRange(InMiniGrid(strategyManager, miniRow, miniCol, true));
            }
        }

        return result;
    }

    public static List<AlmostLockedSet> InRow(IStrategyManager strategyManager, int row, int max = 9)
    {
        var result = new List<AlmostLockedSet>();
        var visited = new List<Cell>();
        
        for (int col = 0; col < 9; col++)
        {
            if (strategyManager.Sudoku[row, col] != 0) continue;

            if (strategyManager.PossibilitiesAt(row, col).Count == 2)
                result.Add(new AlmostLockedSet(new Cell(row, col), strategyManager.PossibilitiesAt(row, col)));

            visited.Add(new Cell(row, col));
            if(max > 1) InRow(strategyManager, row, col + 1, strategyManager.PossibilitiesAt(row, col), visited, result, max);
            visited.RemoveAt(visited.Count - 1);
        }

        return result;
    }

    public static List<AlmostLockedSet> InColumn(IStrategyManager strategyManager, int col, int max = 9)
    {
        var result = new List<AlmostLockedSet>();
        var visited = new List<Cell>();
        
        for (int row = 0; row < 9; row++)
        {
            if (strategyManager.Sudoku[row, col] != 0) continue;

            visited.Add(new Cell(row, col));
            if(max > 1) InColumn(strategyManager, col, row + 1, strategyManager.PossibilitiesAt(row, col), visited, result, max);
            visited.RemoveAt(visited.Count - 1);
        }

        return result;
    }

    public static List<AlmostLockedSet> InMiniGrid(IStrategyManager strategyManager, int miniRow, int miniCol,
        bool excludeSameLine = false, int max = 9)
    {
        var result = new List<AlmostLockedSet>();
        var visited = new List<Cell>();
        
        for (int n = 0; n < 9; n++)
        {
            int row = miniRow * 3 + n / 3;
            int col = miniCol * 3 + n % 3;
            if (strategyManager.Sudoku[row, col] != 0) continue;

            visited.Add(new Cell(row, col));
            if(max > 1) InMiniGrid(strategyManager, miniRow, miniCol, n + 1, strategyManager.PossibilitiesAt(row, col), visited, result, excludeSameLine, max);
            visited.RemoveAt(visited.Count - 1);
        }

        return result;
    }
    
    private static void InRow(IStrategyManager strategyManager, int row, int start, IReadOnlyPossibilities current,
        List<Cell> visited, List<AlmostLockedSet> result, int max)
    {
        for (int col = start; col < 9; col++)
        {
            var inspected = strategyManager.PossibilitiesAt(row, col);
            if (inspected.Count == 0 || !current.PeekAny(inspected)) continue;

            IPossibilities mashed = current.Or(inspected);
            visited.Add(new Cell(row, col));

            if (mashed.Count == visited.Count + 1)
            {
                result.Add(new AlmostLockedSet(visited.ToArray(), mashed));
            }

            if(max > visited.Count) InRow(strategyManager, row, col + 1, mashed, visited, result, max);
            
            visited.RemoveAt(visited.Count - 1);
        }
    }
    
    private static void InColumn(IStrategyManager strategyManager, int col, int start, IReadOnlyPossibilities current,
        List<Cell> visited, List<AlmostLockedSet> result, int max)
    {
        for (int row = start; row < 9; row++)
        {
            var inspected = strategyManager.PossibilitiesAt(row, col);
            if (inspected.Count == 0 || !current.PeekAny(inspected)) continue;

            IPossibilities mashed = current.Or(inspected);
            visited.Add(new Cell(row, col));

            if (mashed.Count == visited.Count + 1)
            {
                result.Add(new AlmostLockedSet(visited.ToArray(), mashed));
            }

            if(max > visited.Count) InColumn(strategyManager, col, row + 1, mashed, visited, result, max);
            
            visited.RemoveAt(visited.Count - 1);
        }
    }

    private static void InMiniGrid(IStrategyManager strategyManager, int miniRow, int miniCol, int start,
        IReadOnlyPossibilities current, List<Cell> visited, List<AlmostLockedSet> result, bool excludeSameLine, int max)
    {
        for (int n = start; n < 9; n++)
        {
            int row = miniRow * 3 + n / 3;
            int col = miniCol * 3 + n % 3;

            var inspected = strategyManager.PossibilitiesAt(row, col);
            if (inspected.Count == 0 || !current.PeekAny(inspected)) continue;

            IPossibilities mashed = current.Or(inspected);
            visited.Add(new Cell(row, col));

            if (mashed.Count == visited.Count + 1 && (!excludeSameLine || NotInSameRowOrColumn(visited)))
            {
                result.Add(new AlmostLockedSet(visited.ToArray(), mashed));
            }

            if(max > visited.Count) InMiniGrid(strategyManager, miniRow, miniCol, n + 1, mashed, visited, result, excludeSameLine, max);
            
            visited.RemoveAt(visited.Count - 1);
        }
    }

    private static bool NotInSameRowOrColumn(List<Cell> cells)
    {
        int row = cells[0].Row;
        int col = cells[0].Col;

        bool rowOk = false;
        bool colOk = false;

        for (int i = 1; i < cells.Count; i++)
        {
            if (!rowOk && cells[i].Row != row) rowOk = true;
            if (!colOk && cells[i].Col != col) colOk = true;

            if (rowOk && colOk) return true;
        }

        return false;
    }
}