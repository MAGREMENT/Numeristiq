using System.Collections.Generic;
using Global;
using Global.Enums;

namespace Model.Player;

public class SudokuPlayer : IPlayer, IHistoryCreator
{
    private readonly PlayerCell[,] _cells = new PlayerCell[9, 9];
    private readonly Historic _historic;
    
    public event OnChange? Changed;
    public event OnMoveAvailabilityChange? MoveAvailabilityChanged;

    public SudokuPlayer()
    {
        _historic = new Historic(this);

        _historic.MoveAvailabilityChanged += (a, b) => MoveAvailabilityChanged?.Invoke(a, b);
    }

    public void SetNumber(int number, IEnumerable<Cell> cells)
    {
        bool yes = false;
        
        foreach (var c in cells)
        {
            if (!yes && _cells[c.Row, c.Column].Number() != number)
            {
                _historic.NewHistoricPoint();
                yes = true;
            }
            
            _cells[c.Row, c.Column].SetNumber(number);
        }

        if (yes) Changed?.Invoke();
    }

    public void RemoveNumber(int number, IEnumerable<Cell> cells)
    {
        bool yes = false;

        foreach (var c in cells)
        {
            if (!yes && _cells[c.Row, c.Column].Number() == number)
            {
                _historic.NewHistoricPoint();
                yes = true;
            }
            
            _cells[c.Row, c.Column].RemoveNumber(number);
        }
        
        if (yes) Changed?.Invoke();
    }

    public void AddPossibility(int possibility, PossibilitiesLocation location, IEnumerable<Cell> cells)
    {
        bool yes = false;
        
        foreach (var c in cells)
        {
            if (!yes && !_cells[c.Row, c.Column].PeekPossibility(possibility, location))
            {
                _historic.NewHistoricPoint();
                yes = true;
            }
            
            _cells[c.Row, c.Column].AddPossibility(possibility, location);
        }
        
        if (yes) Changed?.Invoke();
    }
    
    public void RemovePossibility(int possibility, PossibilitiesLocation location, IEnumerable<Cell> cells)
    {
        bool yes = false;
        
        foreach (var c in cells)
        {
            if (!yes && _cells[c.Row, c.Column].PeekPossibility(possibility, location))
            {
                _historic.NewHistoricPoint();
                yes = true;
            }
            
            _cells[c.Row, c.Column].RemovePossibility(possibility, location);
        }
        
        if (yes) Changed?.Invoke();
    }

    public void MoveBack()
    {
        _historic.MoveBack();
    }

    public void MoveForward()
    {
        _historic.MoveForward();
    }

    public void ShowHistoricPoint(HistoricPoint point)
    {
        for (int row = 0; row < 9; row++)
        {
            for (int col = 0; col < 9; col++)
            {
                _cells[row, col] = point[row, col];
            }
        }
        
        Changed?.Invoke();
    }

    public PlayerCell this[int row, int column] => _cells[row, column];
}