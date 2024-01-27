using System;
using System.Collections.Generic;
using Global;

namespace Model.TectonicSolving;

public class ArrayTectonic : ITectonic
{
    private readonly TectonicCell?[,] _cells;
    private readonly Zone[] _zones;
    
    public ArrayTectonic(int width, int height, Zone[] zones)
    {
        _cells = new TectonicCell?[width, height];
        _zones = zones;

        for (int i = 0; i < zones.Length; i++)
        {
            foreach (var cell in zones[i])
            {
                _cells[cell.Row, cell.Column] = new TectonicCell(i);
            }
        }
    }

    public IReadOnlyList<Zone> Zones => _zones;

    public int this[int row, int col]
    {
        set
        {
            if (_cells[row, col] is not null) _cells[row, col]!.Number = value;
        }
    }

    public Zone GetZone(Cell cell)
    {
        if (!IsValid(cell)) throw new ArgumentOutOfRangeException();
        return _zones[_cells[cell.Row, cell.Column]!.Zone];
    }

    public IEnumerable<Cell> GetNeighbors(Cell cell)
    {
        var result = new Cell(cell.Row - 1, cell.Column);
        if (IsValid(result)) yield return result;

        result = new Cell(cell.Row - 1, cell.Column - 1);
        if (IsValid(result)) yield return result;
        
        result = new Cell(cell.Row, cell.Column - 1);
        if (IsValid(result)) yield return result;
        
        result = new Cell(cell.Row + 1, cell.Column - 1);
        if (IsValid(result)) yield return result;
        
        result = new Cell(cell.Row + 1, cell.Column);
        if (IsValid(result)) yield return result;
        
        result = new Cell(cell.Row + 1, cell.Column + 1);
        if (IsValid(result)) yield return result;
        
        result = new Cell(cell.Row, cell.Column + 1);
        if (IsValid(result)) yield return result;
        
        result = new Cell(cell.Row - 1, cell.Column + 1);
        if (IsValid(result)) yield return result;
    }

    public IEnumerable<Cell> EachCell()
    {
        for (int row = 0; row < _cells.GetLength(0); row++)
        {
            for (int col = 0; col < _cells.GetLength(1); col++)
            {
                if (_cells[row, col] is not null) yield return new Cell(row, col);
            }
        }
    }

    public IEnumerable<CellNumber> EachCellNumber()
    {
        for (int row = 0; row < _cells.GetLength(0); row++)
        {
            for (int col = 0; col < _cells.GetLength(1); col++)
            {
                if (_cells[row, col] is not null)
                    yield return new CellNumber(new Cell(row, col), _cells[row, col]!.Number);
            }
        }
    }

    private bool IsValid(Cell cell)
    {
        return cell.Row > 0 && cell.Row < _cells.GetLength(0)
                            && cell.Column > 0 && cell.Column < _cells.GetLength(1)
                            && _cells[cell.Row, cell.Column] != null;
    }
}

public class TectonicCell
{
    public int Number { get; set; }
    public int Zone { get; }

    public TectonicCell(int zone)
    {
        Zone = zone;
    }
}