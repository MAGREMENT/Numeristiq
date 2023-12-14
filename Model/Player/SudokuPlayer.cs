using System.Collections.Generic;
using Global;
using Global.Enums;

namespace Model.Player;

public class SudokuPlayer : IPlayer, IHistoryCreator
{
    private readonly PlayerCell[,] _cells = new PlayerCell[9, 9];
    private readonly Dictionary<Cell, HighlightingCollection> _highlighting = new();
    private readonly Historic _historic;

    public bool MultiHighlighting { get; set; } = true;
    public event OnChange? Changed;
    public event OnMoveAvailabilityChange? MoveAvailabilityChanged;

    public SudokuPlayer()
    {
        for (int row = 0; row < 9; row++)
        {
            for (int col = 0; col < 9; col++)
            {
                _cells[row, col] = new PlayerCell(true);
            }
        }
        
        _historic = new Historic(this);

        _historic.MoveAvailabilityChanged += (a, b) => MoveAvailabilityChanged?.Invoke(a, b);
    }

    public void SetNumber(int number, IEnumerable<Cell> cells)
    {
        bool yes = false;
        
        foreach (var c in cells)
        {
            if (!yes && _cells[c.Row, c.Column].Editable && _cells[c.Row, c.Column].Number() != number)
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
            if (!yes && _cells[c.Row, c.Column].Editable && _cells[c.Row, c.Column].Number() == number)
            {
                _historic.NewHistoricPoint();
                yes = true;
            }
            
            _cells[c.Row, c.Column].RemoveNumber(number);
        }
        
        if (yes) Changed?.Invoke();
    }

    public void RemoveNumber(IEnumerable<Cell> cells)
    {
        bool yes = false;

        foreach (var c in cells)
        {
            if (!yes && _cells[c.Row, c.Column].Editable && _cells[c.Row, c.Column].IsNumber())
            {
                _historic.NewHistoricPoint();
                yes = true;
            }
            
            _cells[c.Row, c.Column].RemoveNumber();
        }
        
        if (yes) Changed?.Invoke();
    }

    public void AddPossibility(int possibility, PossibilitiesLocation location, IEnumerable<Cell> cells)
    {
        bool yes = false;
        
        foreach (var c in cells)
        {
            if (!yes && _cells[c.Row, c.Column].Editable && !_cells[c.Row, c.Column].PeekPossibility(possibility, location))
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
            if (!yes && _cells[c.Row, c.Column].Editable && _cells[c.Row, c.Column].PeekPossibility(possibility, location))
            {
                _historic.NewHistoricPoint();
                yes = true;
            }
            
            _cells[c.Row, c.Column].RemovePossibility(possibility, location);
        }
        
        if (yes) Changed?.Invoke();
    }

    public void RemovePossibility(PossibilitiesLocation location, IEnumerable<Cell> cells)
    {
        bool yes = false;
        
        foreach (var c in cells)
        {
            if (!yes && _cells[c.Row, c.Column].Editable && _cells[c.Row, c.Column].PossibilitiesCount(location) > 0)
            {
                _historic.NewHistoricPoint();
                yes = true;
            }
            
            _cells[c.Row, c.Column].RemovePossibility(location);
        }
        
        if (yes) Changed?.Invoke();
    }

    public void Highlight(HighlightColor color, IEnumerable<Cell> cells)
    {
        bool yes = false;

        if (color == HighlightColor.None)
        {
            foreach (var c in cells)
            {
                if (!yes && _highlighting.ContainsKey(c))
                {
                    _historic.NewHistoricPoint();
                    yes = true;
                }

                _highlighting.Remove(c);
            }
        }
        else if(MultiHighlighting)
        {
            _historic.NewHistoricPoint();
            yes = true;
            
            foreach (var c in cells)
            {
                if (_highlighting.TryGetValue(c, out var h))
                {
                    if (h.Contains(color))
                    {
                        var removed = h.Remove(color);
                        if (removed is null) _highlighting.Remove(c);
                        else _highlighting[c] = h;
                    }
                    else
                    {
                        _highlighting[c] = h.Add(color);
                    }
                }
                else _highlighting[c] = new MonoHighlighting(color);
            }
        }
        else
        {
            var highlight = new MonoHighlighting(color);
            foreach (var c in cells)
            {
                if (!yes && (!_highlighting.TryGetValue(c, out var h) || !highlight.Equals(h)))
                {
                    _historic.NewHistoricPoint();
                    yes = true;
                }

                _highlighting[c] = highlight;
            }
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
        
        _highlighting.Clear();
        foreach (var ch in point.Highlighting)
        {
            _highlighting[ch.Cell] = ch.Highlighting;
        }
        
        Changed?.Invoke();
    }

    public PlayerCell this[int row, int column] => _cells[row, column];

    public IEnumerable<CellHighlighting> Highlighting
    {
        get
        {
            foreach (var entry in _highlighting)
            {
                yield return new CellHighlighting(entry.Key, entry.Value);
            }
        }
    }
}

public interface IPlayerState
{
    public PlayerCell this[int row, int column] { get; }
    public IEnumerable<CellHighlighting> Highlighting { get; }
}