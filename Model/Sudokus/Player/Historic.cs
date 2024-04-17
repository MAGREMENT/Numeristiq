using System.Collections.Generic;

namespace Model.Sudokus.Player;

public class Historic
{
    private const int MaxEventCount = 20;
    
    private readonly List<IHistoricEvent> _events = new(MaxEventCount);
    private int _cursor;
    
    public void AddNewEvent(IHistoricEvent e)
    {
        if (_cursor != _events.Count) _events.RemoveRange(_cursor, _events.Count - _cursor);
        
        if (_events.Count == MaxEventCount) _events.RemoveAt(0);
        
        _events.Add(e);
        _cursor = _events.Count;
    }

    public void Clear()
    {
        _events.Clear();
        _cursor = 0;
    }

    public bool CanMoveBack() => IsInEvents(_cursor - 1);

    public bool CanMoveForward() => IsInEvents(_cursor);

    public IHistoricEvent? MoveBack()
    {
        if (!CanMoveBack()) return null;

        _cursor--;
        return _events[_cursor];
    }

    public IHistoricEvent? MoveForward()
    {
        if (!CanMoveForward()) return null;

        var e = _events[_cursor];
        _cursor++;
        return e;
    }

    private bool IsInEvents(int pos)
    {
        return pos >= 0 && pos < _events.Count;
    }
}

public interface IHistoricEvent
{
    void Do(IPlayerData data);
    void Reverse(IPlayerData data);
}