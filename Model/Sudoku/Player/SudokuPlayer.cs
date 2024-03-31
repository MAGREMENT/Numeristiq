using System.Collections.Generic;
using Model.Sudoku.Player.HistoricEvents;
using Model.Utility;

namespace Model.Sudoku.Player;

public class SudokuPlayer : IPlayerCellSetter
{
    private readonly PlayerCell[,] _cells = new PlayerCell[9, 9];
    private readonly Dictionary<Cell, HighlightingCollection> _highlighting = new();
    private readonly Historic _historic = new();
    
    public SudokuPlayer()
    {
        for (int row = 0; row < 9; row++)
        {
            for (int col = 0; col < 9; col++)
            {
                _cells[row, col] = new PlayerCell(true);
            }
        }
    }

    public bool Execute(IPlayerCellAction cellAction, IEnumerable<Cell> on)
    {
        var collection = new EventCollection();

        foreach (var cell in on)
        {
            var c = _cells[cell.Row, cell.Column];
            if (cellAction.CanExecute(c, cell))
            {
                collection.Add(cellAction.Execute(c, cell, this));
            }
        }

        if (collection.Count == 0) return false;
        
        _historic.AddNewEvent(collection);
        return true;
    }

    public bool Execute(IPlayerCellAction cellAction, Cell on)
    {
        var c = _cells[on.Row, on.Column];
        if (!cellAction.CanExecute(c, on)) return false;
        
        var e = cellAction.Execute(c, on, this);
        if (e is null) return false;
        
        _historic.AddNewEvent(e);
        return true;
    }

    public PlayerCell this[int row, int col]
    {
        get => _cells[row, col];
        set => _cells[row, col] = value;
    }

    public PlayerCell this[Cell cell]
    {
        get => _cells[cell.Row, cell.Column];
        set => _cells[cell.Row, cell.Column] = value;
    }
}