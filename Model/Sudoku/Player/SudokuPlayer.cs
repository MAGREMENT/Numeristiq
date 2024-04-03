using System.Collections.Generic;
using Model.Sudoku.Player.HistoricEvents;
using Model.Utility;
using Model.Utility.BitSets;

namespace Model.Sudoku.Player;

public class SudokuPlayer : IPlayerData
{
    private readonly PlayerCell[,] _cells = new PlayerCell[9, 9];
    private readonly Dictionary<Cell, ReadOnlyBitSet16> _highlights = new();
    private readonly Historic _historic = new();
    private readonly PlayerTimer _timer = new();

    public ISubscribableTimer Timer => _timer;
    
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

    public bool Execute(IPlayerAction action, IEnumerable<Cell> on)
    {
        var collection = new EventCollection();
        bool changeMade = false;

        foreach (var cell in on)
        {
            if (action.CanExecute(this, cell))
            {
                collection.Add(action.Execute(this, cell));
                changeMade = true;
            }
        }
        
        _historic.AddNewEvent(collection);
        return changeMade;
    }

    public bool Execute(IPlayerAction action, Cell on)
    {
        if (!action.CanExecute(this, on)) return false;
        
        var e = action.Execute(this, on);
        if (e is not null) _historic.AddNewEvent(e);
        return true;
    }

    public void StartTimer()
    {
        OnTimerStarting();
        _timer.Start();
    }

    public void PauseTimer()
    {
        _timer.Pause();
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns>True if the timer started from 0</returns>
    public bool PlayTimer()
    {
        var fromZero = !_timer.HasTimeElapsed;
        if (fromZero) OnTimerStarting();
        
        _timer.Play();
        return fromZero;
    }

    public void StopTimer()
    {
        _timer.Stop();
        
        for (int row = 0; row < 9; row++)
        {
            for (int col = 0; col < 9; col++)
            {
                _cells[row, col].Editable = true;
            }
        }
    }

    public bool CanMoveBack() => _historic.CanMoveBack();

    public bool CanMoveForward() => _historic.CanMoveForward();

    public void MoveBack()
    {
        _historic.MoveBack()?.Reverse(this);
    }

    public void MoveForward()
    {
        _historic.MoveForward()?.Do(this);
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

    public void SetHighlightsFor(Cell cell, ReadOnlyBitSet16 collection)
    {
        if (collection.Count == 0) _highlights.Remove(cell);
        _highlights[cell] = collection;
    }
    
    public ReadOnlyBitSet16 GetHighlightsFor(Cell cell)
    {
        return _highlights.TryGetValue(cell, out var result) ? result : new ReadOnlyBitSet16();
    }

    public IEnumerable<KeyValuePair<Cell, ReadOnlyBitSet16>> EnumerateHighlights()
    {
        return _highlights;
    }

    private void OnTimerStarting()
    {
        _historic.Clear();

        for (int row = 0; row < 9; row++)
        {
            for (int col = 0; col < 9; col++)
            {
                if(_cells[row, col].IsNumber()) _cells[row, col].Editable = false;
            }
        }
    }
}

public interface IPlayerData : IReadOnlyPlayerData
{
    public new PlayerCell this[int row, int col] { get; set; }
    public new PlayerCell this[Cell cell] { get; set; }
    public void SetHighlightsFor(Cell cell, ReadOnlyBitSet16 collection);
}

public interface IReadOnlyPlayerData
{
    public PlayerCell this[int row, int col] { get; }
    public PlayerCell this[Cell cell] { get; }
    public ReadOnlyBitSet16 GetHighlightsFor(Cell cell);
}

public enum HighlightColor
{
    First, Second, Third, Fourth, Fifth, Sixth, Seventh
}

public static class HighlightColorExtensions
{
    public static HighlightColor[] ToColorArray(this ReadOnlyBitSet16 bitSet)
    {
        var result = new HighlightColor[bitSet.Count];
        var cursor = 0;
        
        foreach (var i in bitSet.Enumerate(0, 6))
        {
            result[cursor++] = (HighlightColor)i;
        }

        return result;
    }
}