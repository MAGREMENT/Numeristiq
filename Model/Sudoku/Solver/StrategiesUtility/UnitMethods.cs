using System.Collections.Generic;
using Model.Sudoku.Solver.Position;
using Model.Utility;

namespace Model.Sudoku.Solver.StrategiesUtility;

public static class UnitMethods
{
    public static readonly IUnitMethods[] All = { new RowMethods(), new ColumnMethods(), new BoxMethods() };

    public static IUnitMethods Get(Unit u)
    {
        return All[(int)u];
    }
}

public interface IUnitMethods
{
    IEnumerable<Cell> EveryCell(int unit);

    IEnumerable<Cell> EveryCell(Cell cell);

    IEnumerable<Cell> EveryCell(IReadOnlyGridPositions gp, int unit);
    
    int Count(IReadOnlyGridPositions gp, Cell c);

    int Count(IReadOnlyGridPositions gp, int unit);

    void Fill(GridPositions gp, Cell c);
    
    void Fill(GridPositions gp, int unit);

    void Void(GridPositions gp, Cell c);
    
    void Void(GridPositions gp, int unit);

    bool Contains(Cell cell, int unitNumber);

    CoverHouse ToCoverHouse(Cell cell);
}

public class RowMethods : IUnitMethods
{
    public IEnumerable<Cell> EveryCell(int unit)
    {
        for (int col = 0; col < 9; col++)
        {
            yield return new Cell(unit, col);
        }
    }

    public IEnumerable<Cell> EveryCell(Cell cell)
    {
        for (int col = 0; col < 9; col++)
        {
            yield return new Cell(cell.Row, col);
        }
    }

    public IEnumerable<Cell> EveryCell(IReadOnlyGridPositions gp, int unit)
    {
        for (int col = 0; col < 9; col++)
        {
            var c = new Cell(unit, col);
            if (gp.Contains(c)) yield return c;
        }
    }

    public int Count(IReadOnlyGridPositions gp, Cell c)
    {
        return gp.RowCount(c.Row);
    }

    public int Count(IReadOnlyGridPositions gp, int unit)
    {
        return gp.RowCount(unit);
    }

    public void Fill(GridPositions gp, Cell c)
    {
        gp.FillRow(c.Row);
    }

    public void Fill(GridPositions gp, int unit)
    {
        gp.FillRow(unit);
    }

    public void Void(GridPositions gp, Cell c)
    { 
        gp.VoidRow(c.Row);
    }

    public void Void(GridPositions gp, int unit)
    {
        gp.VoidRow(unit);
    }

    public bool Contains(Cell cell, int unitNumber)
    {
        return cell.Row == unitNumber;
    }

    public CoverHouse ToCoverHouse(Cell cell)
    {
        return new CoverHouse(Unit.Row, cell.Row);
    }
}

public class ColumnMethods : IUnitMethods
{
    public IEnumerable<Cell> EveryCell(int unit)
    {
        for (int row = 0; row < 9; row++)
        {
            yield return new Cell(row, unit);
        }
    }

    public IEnumerable<Cell> EveryCell(Cell cell)
    {
        for (int row = 0; row < 9; row++)
        {
            yield return new Cell(row, cell.Column);
        }
    }

    public IEnumerable<Cell> EveryCell(IReadOnlyGridPositions gp, int unit)
    {
        for (int row = 0; row < 9; row++)
        {
            var c = new Cell(row, unit);
            if (gp.Contains(c)) yield return c;
        }
    }

    public int Count(IReadOnlyGridPositions gp, Cell c)
    {
        return gp.ColumnCount(c.Column);
    }

    public int Count(IReadOnlyGridPositions gp, int unit)
    {
        return gp.ColumnCount(unit);
    }

    public void Fill(GridPositions gp, Cell c)
    {
        gp.FillColumn(c.Column);
    }

    public void Fill(GridPositions gp, int unit)
    {
        gp.FillColumn(unit);
    }

    public void Void(GridPositions gp, Cell c)
    { 
        gp.VoidColumn(c.Column);
    }

    public void Void(GridPositions gp, int unit)
    {
        gp.VoidColumn(unit);
    }

    public bool Contains(Cell cell, int unitNumber)
    {
        return cell.Column == unitNumber;
    }

    public CoverHouse ToCoverHouse(Cell cell)
    {
        return new CoverHouse(Unit.Column, cell.Column);
    }
}

public class BoxMethods : IUnitMethods
{
    public IEnumerable<Cell> EveryCell(int unit)
    {
        var startRow = unit / 3 * 3;
        var startCol = unit % 3 * 3;

        for (int r = 0; r < 3; r++)
        {
            for (int c = 0; c < 3; c++)
            {
                yield return new Cell(startRow + r, startCol + c);
            }
        }
    }

    public IEnumerable<Cell> EveryCell(Cell cell)
    {
        var startRow = cell.Row / 3 * 3;
        var startCol = cell.Column / 3 * 3;

        for (int r = 0; r < 3; r++)
        {
            for (int c = 0; c < 3; c++)
            {
                yield return new Cell(startRow + r, startCol + c);
            }
        }
    }

    public IEnumerable<Cell> EveryCell(IReadOnlyGridPositions gp, int unit)
    {
        var startRow = unit / 3 * 3;
        var startCol = unit % 3 * 3;

        for (int r = 0; r < 3; r++)
        {
            for (int c = 0; c < 3; c++)
            {
                var cell = new Cell(startRow + r, startCol + c);
                if (gp.Contains(cell)) yield return cell;
            }
        }
    }

    public int Count(IReadOnlyGridPositions gp, Cell c)
    {
        return gp.MiniGridCount(c.Row / 3, c.Column / 3);
    }

    public int Count(IReadOnlyGridPositions gp, int unit)
    {
        return gp.MiniGridCount(unit / 3, unit % 3);
    }

    public void Fill(GridPositions gp, Cell c)
    {
        gp.FillMiniGrid(c.Row / 3, c.Column / 3);
    }

    public void Fill(GridPositions gp, int unit)
    {
        gp.FillMiniGrid(unit / 3, unit % 3);
    }

    public void Void(GridPositions gp, Cell c)
    { 
        gp.VoidMiniGrid(c.Row / 3, c.Column / 3);
    }

    public void Void(GridPositions gp, int unit)
    {
        gp.VoidMiniGrid(unit / 3, unit % 3);
    }

    public bool Contains(Cell cell, int unitNumber)
    {
        return cell.Row / 3 * 3 + cell.Column / 3 == unitNumber;
    }

    public CoverHouse ToCoverHouse(Cell cell)
    {
        return new CoverHouse(Unit.MiniGrid, cell.Row / 3 * 3 + cell.Column / 3);
    }
}