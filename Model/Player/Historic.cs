using System.Collections.Generic;

namespace Model.Player;

public class Historic
{
    private const int MaxBackCapacity = 20;

    private readonly List<HistoricPoint> _back = new();
    private HistoricPoint? _current;
    private int _cursor;

    private readonly IHistoryCreator _creator;

    public event OnMoveAvailabilityChange? MoveAvailabilityChanged;

    public Historic(IHistoryCreator creator)
    {
        _creator = creator;
    }

    public void NewHistoricPoint()
    {
        if (_cursor < _back.Count) _back.RemoveRange(_cursor, _back.Count - _cursor);

        _back.Add(HistoricPoint.From(_creator));
        if (_back.Count > MaxBackCapacity) _back.RemoveAt(0);

        _current = null;
        _cursor = _back.Count;
        
        MoveAvailabilityChanged?.Invoke(CanMoveBack(),  CanMoveForward());
    }

    public void MoveBack()
    {
        if (!CanMoveBack()) return;

        if (_cursor == _back.Count && _current is null)
        {
            _current = HistoricPoint.From(_creator);
        }

        _cursor--;
        _creator.ShowHistoricPoint(_back[_cursor]);
        
        MoveAvailabilityChanged?.Invoke(CanMoveBack(),  CanMoveForward());
    }

    public void MoveForward()
    {
        if (!CanMoveForward()) return;

        _cursor++;
        var h = _cursor < _back.Count ? _back[_cursor] : _current!;
        _creator.ShowHistoricPoint(h);
        
        MoveAvailabilityChanged?.Invoke(CanMoveBack(), CanMoveForward());
    }

    private bool CanMoveBack()
    {
        return _cursor > 0;
    }

    private bool CanMoveForward()
    {
        return _cursor < _back.Count - 1 || (_cursor == _back.Count - 1 && _current is not null);
    }
}

public class HistoricPoint
{
    private readonly PlayerCell[,] _cells = new PlayerCell[9, 9];

    public static HistoricPoint From(IHistoryCreator creator)
    {
        HistoricPoint hp = new();
        for (int row = 0; row < 9; row++)
        {
            for (int col = 0; col < 9; col++)
            {
                hp._cells[row, col] = creator[row, col];
            }
        }

        return hp;
    }

    public PlayerCell this[int row, int column] => _cells[row, column];
}

public delegate void OnMoveAvailabilityChange(bool canMoveBack, bool canMoveForward);