using System;

namespace Model.Sudoku.Player.HistoricEvents;

public class EventCollection : IHistoricEvent
{
    private IHistoricEvent[] _events = Array.Empty<IHistoricEvent>();
    
    public int Count { get; private set; }

    public void Add(IHistoricEvent? e)
    {
        if (e is null) return;
        
        GrowIfNecessary();
        _events[Count] = e;
        Count++;
    }
    
    public void Do(IPlayerCellSetter setter)
    {
        for(int i = 0; i < Count; i++)
        {
            _events[i].Do(setter);
        }
    }

    public void Reverse(IPlayerCellSetter setter)
    {
        for(int i = 0; i < Count; i++)
        {
            _events[i].Reverse(setter);
        }
    }

    private void GrowIfNecessary()
    {
        if (_events.Length > Count) return;

        var buffer = new IHistoricEvent[_events.Length == 0 ? 4 : _events.Length * 2];
        Array.Copy(_events, 0, buffer, 0, _events.Length);
        _events = buffer;
    }
}