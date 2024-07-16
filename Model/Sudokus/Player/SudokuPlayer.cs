using System;
using System.Collections.Generic;
using Model.Core;
using Model.Core.BackTracking;
using Model.Sudokus.Player.HistoricEvents;
using Model.Utility;
using Model.Utility.BitSets;

namespace Model.Sudokus.Player;

public class SudokuPlayer : IPlayerData, INumericSolvingState, IPossibilitiesGiver
{
    private readonly PlayerCell[,] _cells = new PlayerCell[9, 9];
    private readonly Dictionary<Cell, HighlightData> _highlights = new();
    private readonly ActionHistory _actionHistory = new();
    private readonly PlayerTimer _timer = new();

    public PossibilitiesLocation MainLocation { get; set; } = PossibilitiesLocation.Middle;
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

    public bool Execute(ICellAction action, IEnumerable<Cell> on)
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
        
        _actionHistory.AddNewEvent(collection);
        return changeMade;
    }

    public bool Execute(ICellAction action, Cell on)
    {
        if (!action.CanExecute(this, on)) return false;
        
        var e = action.Execute(this, on);
        if (e is not null) _actionHistory.AddNewEvent(e);
        return true;
    }
    
    public bool Execute(ICellPossibilityAction action, IEnumerable<CellPossibility> on)
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
        
        _actionHistory.AddNewEvent(collection);
        return changeMade;
    }
    
    public bool Execute(ICellPossibilityAction action, CellPossibility on)
    {
        if (!action.CanExecute(this, on)) return false;
        
        var e = action.Execute(this, on);
        if (e is not null) _actionHistory.AddNewEvent(e);
        return true;
    }

    public bool Execute(IGlobalAction action)
    {
        if (!action.CanExecute(this)) return false;
        var e = action.Execute(this);
        if (e is not null) _actionHistory.AddNewEvent(e);
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

    public bool CanMoveBack() => _actionHistory.CanMoveBack();

    public bool CanMoveForward() => _actionHistory.CanMoveForward();

    public void MoveBack()
    {
        _actionHistory.MoveBack()?.Reverse(this);
    }

    public void MoveForward()
    {
        _actionHistory.MoveForward()?.Do(this);
    }

    public void SetCellDataFor(int row, int col, PlayerCell data) => _cells[row, col] = data;

    public void SetCellDataFor(Cell cell, PlayerCell data) => _cells[cell.Row, cell.Column] = data;

    public void SetHighlightsFor(Cell cell, HighlightData data)
    {
        if (data.Count == 0) _highlights.Remove(cell);
        _highlights[cell] = data;
    }

    public PlayerCell GetCellDataFor(int row, int col) => _cells[row, col];

    public PlayerCell GetCellDataFor(Cell cell) => _cells[cell.Row, cell.Column];
    public PlayerCell[,] CopyCellData()
    {
        var result = new PlayerCell[9, 9];
        Array.Copy(_cells, result, _cells.Length);
        return result;
    }

    public HighlightData GetHighlightsFor(Cell cell)
    {
        return _highlights.TryGetValue(cell, out var result) ? result : new HighlightData();
    }

    public IEnumerable<KeyValuePair<Cell, HighlightData>> EnumerateHighlights()
    {
        return _highlights;
    }

    private void OnTimerStarting()
    {
        _actionHistory.Clear();

        for (int row = 0; row < 9; row++)
        {
            for (int col = 0; col < 9; col++)
            {
                if(_cells[row, col].IsNumber()) _cells[row, col].Editable = false;
            }
        }
    }

    public int RowCount => 9;
    public int ColumnCount => 9;
    public int this[int row, int col] => _cells[row, col].Number();

    public ReadOnlyBitSet16 PossibilitiesAt(int row, int col) => _cells[row, col].PossibilitiesCount(MainLocation) > 0
        ? _cells[row, col].PossibilitiesAsBitSet(MainLocation)
        : ReadOnlyBitSet16.Filled(1, 9);
    public IEnumerable<int> EnumeratePossibilitiesAt(int row, int col) => _cells[row, col].PossibilitiesCount(MainLocation) > 0
        ? _cells[row, col].Possibilities(MainLocation)
        : ConstantPossibilitiesGiver.Instance.EnumeratePossibilitiesAt(row, col);
}

public interface IPlayerData : IReadOnlyPlayerData
{
    public void SetCellDataFor(int row, int col, PlayerCell data);
    public void SetCellDataFor(Cell cell, PlayerCell data);
    public void SetHighlightsFor(Cell cell, HighlightData collection);
}

public interface IReadOnlyPlayerData
{
    public PlayerCell GetCellDataFor(int row, int col);
    public PlayerCell GetCellDataFor(Cell cell);
    public PlayerCell[,] CopyCellData();
    public HighlightData GetHighlightsFor(Cell cell);
}