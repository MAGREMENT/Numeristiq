using Model.Utility;
using Model.Utility.BitSets;

namespace Model.Sudokus.Player.HistoricEvents;

public class HighlightChangeEvent : IHistoricEvent
{
    private readonly HighlightData _from;
    private readonly HighlightData _to;
    private readonly Cell _cell;

    public HighlightChangeEvent(HighlightData from, HighlightData to, Cell cell)
    {
        _from = from;
        _to = to;
        _cell = cell;
    }

    public void Do(IPlayerData data)
    {
        data.SetHighlightsFor(_cell, _to);
    }

    public void Reverse(IPlayerData data)
    {
        data.SetHighlightsFor(_cell, _from);
    }
}