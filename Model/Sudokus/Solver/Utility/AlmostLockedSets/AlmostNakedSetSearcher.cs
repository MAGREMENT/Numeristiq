using System.Collections.Generic;
using Model.Sudokus.Solver.PossibilityPosition;
using Model.Utility;
using Model.Utility.BitSets;

namespace Model.Sudokus.Solver.Utility.AlmostLockedSets;

public class AlmostNakedSetSearcher
{
    private readonly IStrategyUser _strategyUser;

    public int Max { get; set; } = 5;
    public int Difference { get; set; } = 1;

    public AlmostNakedSetSearcher(IStrategyUser strategyUser)
    {
        _strategyUser = strategyUser;
    }
    
    public List<IPossibilitiesPositions> InCells(List<Cell> coords)
    {
        List<IPossibilitiesPositions> result = new();

        InCells(coords, new List<Cell>(), new ReadOnlyBitSet16(), 0, result);
        
        return result;
    }

    private void InCells(List<Cell> coords, List<Cell> visited,
        ReadOnlyBitSet16 current, int start, List<IPossibilitiesPositions> result)
    {
        for (int i = start; i < coords.Count; i++)
        {
            if (!SudokuCellUtility.ShareAUnitWithAll(coords[i], visited)) continue;

            var inspected = _strategyUser.PossibilitiesAt(coords[i].Row, coords[i].Column);
            if(inspected.Count == 0 || (current.Count != 0 && !current.ContainsAny(inspected))) continue;

            var or = current | inspected;
            visited.Add(coords[i]);

            if (or.Count == visited.Count + Difference)
            {
                result.Add(new CAPPossibilitiesPositions(visited.ToArray(), or, _strategyUser));
            }

            if (Max > visited.Count) InCells(coords, visited, or, i + 1, result);
            
            visited.RemoveAt(visited.Count - 1);
        }
    }
    
    public List<IPossibilitiesPositions> FullGrid()
    {
        var result = new List<IPossibilitiesPositions>();
        var possibilities = new ReadOnlyBitSet16();
        var cells = new List<Cell>();

        for (int row = 0; row < 9; row++)
        {
            InRow(row, 0, possibilities, cells, result);
            possibilities = new ReadOnlyBitSet16();
            cells.Clear();
        }
        
        for (int col = 0; col < 9; col++)
        {
            InColumn(col, 0, possibilities, cells, result, true);
            possibilities = new ReadOnlyBitSet16();
            cells.Clear();
        }

        for (int miniRow = 0; miniRow < 3; miniRow++)
        {
            for (int miniCol = 0; miniCol < 3; miniCol++)
            {
                InMiniGrid(miniRow, miniCol, 0, possibilities, cells, result, true);
                possibilities = new ReadOnlyBitSet16();
                cells.Clear();
            }
        }

        return result;
    }

    public List<IPossibilitiesPositions> InRow(int row)
    {
        var result = new List<IPossibilitiesPositions>();

        InRow(row, 0, new ReadOnlyBitSet16(), new List<Cell>(), result);

        return result;
    }

    public List<IPossibilitiesPositions> InColumn(int col)
    {
        var result = new List<IPossibilitiesPositions>();

        InColumn(col, 0, new ReadOnlyBitSet16(), new List<Cell>(), result, false);

        return result;
    }

    public List<IPossibilitiesPositions> InMiniGrid(int miniRow, int miniCol)
    {
        var result = new List<IPossibilitiesPositions>();

        InMiniGrid(miniRow, miniCol, 0, new ReadOnlyBitSet16(), new List<Cell>(), result, false);

        return result;
    }
    
    private void InRow(int row, int start, ReadOnlyBitSet16 current,
        List<Cell> visited, List<IPossibilitiesPositions> result)
    {
        for (int col = start; col < 9; col++)
        {
            var inspected = _strategyUser.PossibilitiesAt(row, col);
            if (inspected.Count == 0 || (current.Count != 0 && !current.ContainsAny(inspected))) continue;

            var mashed = current | inspected;
            visited.Add(new Cell(row, col));

            if (mashed.Count == visited.Count + Difference)
            {
                result.Add(new CAPPossibilitiesPositions(visited.ToArray(), mashed, _strategyUser));
            }

            if(Max > visited.Count) InRow(row, col + 1, mashed, visited, result);
            
            visited.RemoveAt(visited.Count - 1);
        }
    }
    
    private void InColumn(int col, int start, ReadOnlyBitSet16 current,
        List<Cell> visited, List<IPossibilitiesPositions> result, bool excludeSingles)
    {
        for (int row = start; row < 9; row++)
        {
            var inspected = _strategyUser.PossibilitiesAt(row, col);
            if (inspected.Count == 0 || (current.Count != 0 && !current.ContainsAny(inspected))) continue;

            var mashed = current | inspected;
            visited.Add(new Cell(row, col));

            if (mashed.Count == visited.Count + Difference && (!excludeSingles || visited.Count > 1))
            {
                result.Add(new CAPPossibilitiesPositions(visited.ToArray(), mashed, _strategyUser));
            }

            if(Max > visited.Count) InColumn(col, row + 1, mashed, visited, result, excludeSingles);
            
            visited.RemoveAt(visited.Count - 1);
        }
    }

    private void InMiniGrid(int miniRow, int miniCol, int start,
        ReadOnlyBitSet16 current, List<Cell> visited, List<IPossibilitiesPositions> result, bool excludeSameLine)
    {
        for (int n = start; n < 9; n++)
        {
            int row = miniRow * 3 + n / 3;
            int col = miniCol * 3 + n % 3;

            var inspected = _strategyUser.PossibilitiesAt(row, col);
            if (inspected.Count == 0 || (current.Count != 0 && !current.ContainsAny(inspected))) continue;

            var mashed = current | inspected;
            visited.Add(new Cell(row, col));

            if (mashed.Count == visited.Count + Difference && (!excludeSameLine || NotInSameRowOrColumn(visited)))
            {
                result.Add(new CAPPossibilitiesPositions(visited.ToArray(), mashed, _strategyUser));
            }

            if(Max > visited.Count) InMiniGrid(miniRow, miniCol, n + 1, mashed, visited, result, excludeSameLine);
            
            visited.RemoveAt(visited.Count - 1);
        }
    }

    private bool NotInSameRowOrColumn(List<Cell> cells)
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