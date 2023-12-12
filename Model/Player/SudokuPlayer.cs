using System.Collections.Generic;
using Global;
using Global.Enums;

namespace Model.Player;

public class SudokuPlayer : IPlayer
{
    private readonly PlayerCell[,] _cells = new PlayerCell[9, 9];
    
    public event OnChange? Changed;

    public void SetNumber(int number, IEnumerable<Cell> cells)
    {
        foreach (var c in cells)
        {
            _cells[c.Row, c.Column].SetNumber(number);
        }
        
        Changed?.Invoke();
    }

    public void RemoveNumber(int number, IEnumerable<Cell> cells)
    {
        foreach (var c in cells)
        {
            _cells[c.Row, c.Column].RemoveNumber(number);
        }
        
        Changed?.Invoke();
    }

    public void AddPossibility(int possibility, PossibilitiesLocation location, IEnumerable<Cell> cells)
    {
        foreach (var c in cells)
        {
            _cells[c.Row, c.Column].AddPossibility(possibility, location);
        }
        
        Changed?.Invoke();
    }
    
    public void RemovePossibility(int possibility, PossibilitiesLocation location, IEnumerable<Cell> cells)
    {
        foreach (var c in cells)
        {
            _cells[c.Row, c.Column].RemovePossibility(possibility, location);
        }
        
        Changed?.Invoke();
    }

    public PlayerCell this[int row, int column] => _cells[row, column];
}