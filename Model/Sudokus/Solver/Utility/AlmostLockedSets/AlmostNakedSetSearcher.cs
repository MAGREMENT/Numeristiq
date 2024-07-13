using System.Collections.Generic;
using Model.Sudokus.Solver.PossibilityPosition;
using Model.Utility;
using Model.Utility.BitSets;

namespace Model.Sudokus.Solver.Utility.AlmostLockedSets;

public class AlmostNakedSetSearcher
{
    private readonly ISudokuSolverData _solverData;
    private int _maxSize = 5;
    private int _difference = 1;

    public AlmostNakedSetSearcher(ISudokuSolverData solverData)
    {
        _solverData = solverData;
    }
    
    public List<IPossibilitiesPositions> InCells(IReadOnlyList<Cell> coords, int maxSize, int difference)
    {
        List<IPossibilitiesPositions> result = new();

        _maxSize = maxSize;
        _difference = difference;
        InCells(coords, new List<Cell>(), new ReadOnlyBitSet16(), 0, result);
        
        return result;
    }
    
    public List<IPossibilitiesPositions> FullGrid(int maxSize, int difference)
    {
        var result = new List<IPossibilitiesPositions>();
        var possibilities = new ReadOnlyBitSet16();
        var cells = new List<Cell>();
        _maxSize = maxSize;
        _difference = difference;

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

    public List<IPossibilitiesPositions> InRow(int row, int maxSize, int difference)
    {
        var result = new List<IPossibilitiesPositions>();

        _maxSize = maxSize;
        _difference = difference;
        InRow(row, 0, new ReadOnlyBitSet16(), new List<Cell>(), result);

        return result;
    }

    public List<IPossibilitiesPositions> InColumn(int col, int maxSize, int difference)
    {
        var result = new List<IPossibilitiesPositions>();

        _maxSize = maxSize;
        _difference = difference;
        InColumn(col, 0, new ReadOnlyBitSet16(), new List<Cell>(), result, false);

        return result;
    }

    public List<IPossibilitiesPositions> InMiniGrid(int miniRow, int miniCol, int maxSize, int difference)
    {
        var result = new List<IPossibilitiesPositions>();

        _maxSize = maxSize;
        _difference = difference;
        InMiniGrid(miniRow, miniCol, 0, new ReadOnlyBitSet16(), new List<Cell>(), result, false);

        return result;
    }
    
    private void InCells(IReadOnlyList<Cell> coords, List<Cell> visited,
        ReadOnlyBitSet16 current, int start, List<IPossibilitiesPositions> result)
    {
        for (int i = start; i < coords.Count; i++)
        {
            if (!SudokuCellUtility.ShareAUnitWithAll(coords[i], visited)) continue;

            var inspected = _solverData.PossibilitiesAt(coords[i].Row, coords[i].Column);
            if(inspected.Count == 0 || (current.Count != 0 && !current.ContainsAny(inspected))) continue;

            var or = current | inspected;
            visited.Add(coords[i]);

            if (or.Count == visited.Count + _difference)
            {
                result.Add(new CAPPossibilitiesPositions(visited.ToArray(), or, _solverData.CurrentState));
            }

            if (_maxSize > visited.Count) InCells(coords, visited, or, i + 1, result);
            
            visited.RemoveAt(visited.Count - 1);
        }
    }
    
    private void InRow(int row, int start, ReadOnlyBitSet16 current,
        List<Cell> visited, List<IPossibilitiesPositions> result)
    {
        for (int col = start; col < 9; col++)
        {
            var inspected = _solverData.PossibilitiesAt(row, col);
            if (inspected.Count == 0 || (current.Count != 0 && !current.ContainsAny(inspected))) continue;

            var mashed = current | inspected;
            visited.Add(new Cell(row, col));

            if (mashed.Count == visited.Count + _difference)
            {
                result.Add(new CAPPossibilitiesPositions(visited.ToArray(), mashed, _solverData.CurrentState));
            }

            if(_maxSize > visited.Count) InRow(row, col + 1, mashed, visited, result);
            
            visited.RemoveAt(visited.Count - 1);
        }
    }
    
    private void InColumn(int col, int start, ReadOnlyBitSet16 current,
        List<Cell> visited, List<IPossibilitiesPositions> result, bool excludeSingles)
    {
        for (int row = start; row < 9; row++)
        {
            var inspected = _solverData.PossibilitiesAt(row, col);
            if (inspected.Count == 0 || (current.Count != 0 && !current.ContainsAny(inspected))) continue;

            var mashed = current | inspected;
            visited.Add(new Cell(row, col));

            if (mashed.Count == visited.Count + _difference && (!excludeSingles || visited.Count > 1))
            {
                result.Add(new CAPPossibilitiesPositions(visited.ToArray(), mashed, _solverData.CurrentState));
            }

            if(_maxSize > visited.Count) InColumn(col, row + 1, mashed, visited, result, excludeSingles);
            
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

            var inspected = _solverData.PossibilitiesAt(row, col);
            if (inspected.Count == 0 || (current.Count != 0 && !current.ContainsAny(inspected))) continue;

            var mashed = current | inspected;
            visited.Add(new Cell(row, col));

            if (mashed.Count == visited.Count + _difference && (!excludeSameLine || NotInSameRowOrColumn(visited)))
            {
                result.Add(new CAPPossibilitiesPositions(visited.ToArray(), mashed, _solverData.CurrentState));
            }

            if(_maxSize > visited.Count) InMiniGrid(miniRow, miniCol, n + 1, mashed, visited, result, excludeSameLine);
            
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