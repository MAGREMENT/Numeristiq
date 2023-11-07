using System.Collections.Generic;
using Model.Solver.Possibilities;

namespace Model.Solver.StrategiesUtil.AlmostLockedSets;

public class AlmostLockedSetSearcher //TODO => replace ALS by IPP
{
    private readonly IStrategyManager _strategyManager;

    public int Max { get; set; } = 5;

    public AlmostLockedSetSearcher(IStrategyManager strategyManager)
    {
        _strategyManager = strategyManager;
    }
    
    public List<AlmostLockedSet> InCells(List<Cell> coords)
    {
        List<AlmostLockedSet> result = new();
        if (Max < 1) return result;
        
        var visited = new List<Cell>(coords.Count);
        for(int i = 0; i < coords.Count; i++){
            IReadOnlyPossibilities current = _strategyManager.PossibilitiesAt(coords[i].Row, coords[i].Col);
            if (current.Count == 0) continue;

            if (current.Count == 2) result.Add(new AlmostLockedSet(coords[i], current));
            
            if (Max > 1)
            {
                visited.Add(coords[i]);
                InCells(coords, visited, current, i + 1, result);
                visited.RemoveAt(visited.Count - 1);
            }
        }

        return result;
    }

    private void InCells(List<Cell> coords, List<Cell> visited,
        IReadOnlyPossibilities current, int start, List<AlmostLockedSet> result)
    {
        for (int i = start; i < coords.Count; i++)
        {
            if (!coords[i].ShareAUnitWithAll(visited)) continue;

            var inspected = _strategyManager.PossibilitiesAt(coords[i].Row, coords[i].Col);
            if(inspected.Count == 0 || !current.PeekAny(inspected)) continue;

            IPossibilities or = current.Or(inspected);
            visited.Add(coords[i]);

            if (or.Count == visited.Count + 1)
            {
                result.Add(new AlmostLockedSet(visited.ToArray(), or));
            }

            if (Max > visited.Count) InCells(coords, visited, or, i + 1, result);
            
            visited.RemoveAt(visited.Count - 1);
        }
    }
    
    public List<AlmostLockedSet> FullGrid()
    {
        var result = new List<AlmostLockedSet>();

        for (int row = 0; row < 9; row++)
        {
            result.AddRange(InRow(row));
        }
        
        for (int col = 0; col < 9; col++)
        {
            result.AddRange(InColumn(col));
        }

        for (int miniRow = 0; miniRow < 3; miniRow++)
        {
            for (int miniCol = 0; miniCol < 3; miniCol++)
            {
                result.AddRange(InMiniGrid(miniRow, miniCol, true));
            }
        }

        return result;
    }

    public List<AlmostLockedSet> InRow(int row)
    {
        var result = new List<AlmostLockedSet>();
        var visited = new List<Cell>();
        
        for (int col = 0; col < 9; col++)
        {
            if (_strategyManager.Sudoku[row, col] != 0) continue;

            if (_strategyManager.PossibilitiesAt(row, col).Count == 2)
                result.Add(new AlmostLockedSet(new Cell(row, col), _strategyManager.PossibilitiesAt(row, col)));

            visited.Add(new Cell(row, col));
            if(Max > 1) InRow(row, col + 1, _strategyManager.PossibilitiesAt(row, col), visited, result);
            visited.RemoveAt(visited.Count - 1);
        }

        return result;
    }

    public List<AlmostLockedSet> InColumn(int col)
    {
        var result = new List<AlmostLockedSet>();
        var visited = new List<Cell>();
        
        for (int row = 0; row < 9; row++)
        {
            if (_strategyManager.Sudoku[row, col] != 0) continue;

            visited.Add(new Cell(row, col));
            if(Max > 1) InColumn(col, row + 1, _strategyManager.PossibilitiesAt(row, col), visited, result);
            visited.RemoveAt(visited.Count - 1);
        }

        return result;
    }

    public List<AlmostLockedSet> InMiniGrid(int miniRow, int miniCol,
        bool excludeSameLine = false)
    {
        var result = new List<AlmostLockedSet>();
        var visited = new List<Cell>();
        
        for (int n = 0; n < 9; n++)
        {
            int row = miniRow * 3 + n / 3;
            int col = miniCol * 3 + n % 3;
            if (_strategyManager.Sudoku[row, col] != 0) continue;

            visited.Add(new Cell(row, col));
            if(Max > 1) InMiniGrid(miniRow, miniCol, n + 1, _strategyManager.PossibilitiesAt(row, col), visited, result, excludeSameLine);
            visited.RemoveAt(visited.Count - 1);
        }

        return result;
    }
    
    private void InRow(int row, int start, IReadOnlyPossibilities current,
        List<Cell> visited, List<AlmostLockedSet> result)
    {
        for (int col = start; col < 9; col++)
        {
            var inspected = _strategyManager.PossibilitiesAt(row, col);
            if (inspected.Count == 0 || !current.PeekAny(inspected)) continue;

            IPossibilities mashed = current.Or(inspected);
            visited.Add(new Cell(row, col));

            if (mashed.Count == visited.Count + 1)
            {
                result.Add(new AlmostLockedSet(visited.ToArray(), mashed));
            }

            if(Max > visited.Count) InRow(row, col + 1, mashed, visited, result);
            
            visited.RemoveAt(visited.Count - 1);
        }
    }
    
    private void InColumn(int col, int start, IReadOnlyPossibilities current,
        List<Cell> visited, List<AlmostLockedSet> result)
    {
        for (int row = start; row < 9; row++)
        {
            var inspected = _strategyManager.PossibilitiesAt(row, col);
            if (inspected.Count == 0 || !current.PeekAny(inspected)) continue;

            IPossibilities mashed = current.Or(inspected);
            visited.Add(new Cell(row, col));

            if (mashed.Count == visited.Count + 1)
            {
                result.Add(new AlmostLockedSet(visited.ToArray(), mashed));
            }

            if(Max > visited.Count) InColumn(col, row + 1, mashed, visited, result);
            
            visited.RemoveAt(visited.Count - 1);
        }
    }

    private void InMiniGrid(int miniRow, int miniCol, int start,
        IReadOnlyPossibilities current, List<Cell> visited, List<AlmostLockedSet> result, bool excludeSameLine)
    {
        for (int n = start; n < 9; n++)
        {
            int row = miniRow * 3 + n / 3;
            int col = miniCol * 3 + n % 3;

            var inspected = _strategyManager.PossibilitiesAt(row, col);
            if (inspected.Count == 0 || !current.PeekAny(inspected)) continue;

            IPossibilities mashed = current.Or(inspected);
            visited.Add(new Cell(row, col));

            if (mashed.Count == visited.Count + 1 && (!excludeSameLine || NotInSameRowOrColumn(visited)))
            {
                result.Add(new AlmostLockedSet(visited.ToArray(), mashed));
            }

            if(Max > visited.Count) InMiniGrid(miniRow, miniCol, n + 1, mashed, visited, result, excludeSameLine);
            
            visited.RemoveAt(visited.Count - 1);
        }
    }

    private bool NotInSameRowOrColumn(List<Cell> cells)
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