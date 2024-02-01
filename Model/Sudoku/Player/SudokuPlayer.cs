using System.Collections.Generic;
using Model.Sudoku.Solver;
using Model.Sudoku.Solver.Possibility;
using Model.Utility;

namespace Model.Sudoku.Player;

public class SudokuPlayer : IPlayer, IHistoryCreator
{
    private readonly PlayerCell[,] _cells = new PlayerCell[9, 9];
    private readonly Dictionary<Cell, HighlightingCollection> _highlighting = new();
    private readonly Historic _historic;

    public bool MultiColorHighlighting { get; set; } = true;
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

    public void ClearNumbers(IEnumerable<Cell> cells)
    {
        bool yes = false;

        foreach (var c in cells)
        {
            if (!yes && _cells[c.Row, c.Column].Editable && !_cells[c.Row, c.Column].IsEmpty())
            {
                _historic.NewHistoricPoint();
                yes = true;
            }
            
            _cells[c.Row, c.Column].Empty();
        }
        
        if(yes) Changed?.Invoke();
    }

    public void ClearNumbers()
    {
        bool yes = false;
        
        for (int row = 0; row < 9; row++)
        {
            for (int col = 0; col < 9; col++)
            {
                if (!yes && _cells[row, col].Editable && !_cells[row, col].IsEmpty())
                {
                    _historic.NewHistoricPoint();
                    yes = true;
                }
            
                _cells[row, col].Empty(); 
            }
        }

        if(yes) Changed?.Invoke();
    }
    
    public void ComputeDefaultPossibilities()
    {
        var possibilities = new Possibilities();
        bool yes = false;
        
        for (int row = 0; row < 9; row++)
        {
            for (int col = 0; col < 9; col++)
            {
                if(_cells[row, col].IsNumber()) continue;
                
                PossibilitiesFor(possibilities, row, col);
                for (int n = 1; n <= 9; n++)
                {
                    if (possibilities.Peek(n))
                    {
                        if (!yes && !_cells[row, col].PeekPossibility(n, PossibilitiesLocation.Middle))
                        {
                            _historic.NewHistoricPoint();
                            yes = true;
                        }

                        _cells[row, col].AddPossibility(n, PossibilitiesLocation.Middle);
                    }
                    else
                    {
                        if (!yes && _cells[row, col].PeekPossibility(n, PossibilitiesLocation.Middle))
                        {
                            _historic.NewHistoricPoint();
                            yes = true;
                        }

                        _cells[row, col].RemovePossibility(n, PossibilitiesLocation.Middle);
                    }
                }
                
                possibilities.Reset();
            }
        }
        
        if(yes) Changed?.Invoke();
    }

    public void ComputeDefaultPossibilities(IEnumerable<Cell> cells)
    {
        var possibilities = new Possibilities();
        bool yes = false;

        foreach (var c in cells)
        {
            if(_cells[c.Row, c.Column].IsNumber()) continue;
                
            PossibilitiesFor(possibilities, c.Row, c.Column);
            for (int n = 1; n <= 9; n++)
            {
                if (possibilities.Peek(n))
                {
                    if (!yes && !_cells[c.Row, c.Column].PeekPossibility(n, PossibilitiesLocation.Middle))
                    {
                        _historic.NewHistoricPoint();
                        yes = true;
                    }

                    _cells[c.Row, c.Column].AddPossibility(n, PossibilitiesLocation.Middle);
                }
                else
                {
                    if (!yes && _cells[c.Row, c.Column].PeekPossibility(n, PossibilitiesLocation.Middle))
                    {
                        _historic.NewHistoricPoint();
                        yes = true;
                    }

                    _cells[c.Row, c.Column].RemovePossibility(n, PossibilitiesLocation.Middle);
                }
            }
                
            possibilities.Reset();
        }
        
        if(yes) Changed?.Invoke();
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
        else if(MultiColorHighlighting)
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

    public void ClearHighlights(IEnumerable<Cell> cells)
    {
        bool yes = false;
        
        foreach (var c in cells)
        {
            if (!yes && _highlighting.ContainsKey(c))
            {
                _historic.NewHistoricPoint();
                yes = true;
            }

            _highlighting.Remove(c);
        }
        
        if (yes) Changed?.Invoke();
    }

    public void ClearHighlights()
    {
        bool yes = _highlighting.Count > 0;
        
        if (yes) _historic.NewHistoricPoint();
        _highlighting.Clear();
        if(yes) Changed?.Invoke();
    }

    public void MoveBack()
    {
        _historic.MoveBack();
    }

    public void MoveForward()
    {
        _historic.MoveForward();
    }

    public void Paste(Sudoku s)
    {
        _historic.CreateBuffer();

        for (int row = 0; row < 9; row++)
        {
            for (int col = 0; col < 9; col++)
            {
                _cells[row, col].Empty();
                if (s[row, col] != 0) _cells[row, col].SetNumber(s[row, col]);
            }
        }
        
        if(_historic.PushBufferIfDifferent()) Changed?.Invoke();
    }

    public void Paste(SolverState ss)
    {
        _historic.CreateBuffer();

        for (int row = 0; row < 9; row++)
        {
            for (int col = 0; col < 9; col++)
            {
                _cells[row, col].Empty();
                var current = ss.At(row, col);
                if (current.IsPossibilities)
                {
                    foreach (var p in current.AsPossibilities)
                    {
                        _cells[row, col].AddPossibility(p, PossibilitiesLocation.Middle);
                    }
                }
                else
                {
                    _cells[row, col].SetNumber(current.AsNumber);
                }
            }
        }
        
        if(_historic.PushBufferIfDifferent()) Changed?.Invoke();
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
    
    //Private-----------------------------------------------------------------------------------------------------------

    private void PossibilitiesFor(Possibilities possibilities, int row, int col)
    {
        for (int u = 0; u < 9; u++)
        {
            if (u != row && _cells[u, col].IsNumber()) possibilities.Remove(_cells[u, col].Number());
            if (u != col && _cells[row, u].IsNumber()) possibilities.Remove(_cells[row, u].Number());
        }

        var startR = row / 3 * 3;
        var startC = col / 3 * 3;
        for (int r = 0; r < 3; r++)
        {
            for (int c = 0; c < 3; c++)
            {
                var inspectedRow = startR + r;
                var inspectedCol = startC + c;

                if ((row != inspectedRow || col != inspectedCol) && _cells[inspectedRow, inspectedCol].IsNumber())
                    possibilities.Remove(_cells[inspectedRow, inspectedCol].Number());
            }
        }
    }
}

public interface IPlayerState
{
    public PlayerCell this[int row, int column] { get; }
    public IEnumerable<CellHighlighting> Highlighting { get; }
}