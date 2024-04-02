using Model.Utility;
using Model.Utility.BitSets;

namespace Model.Sudoku.Player.HistoricEvents;

public class HighlightChangeEvent : IHistoricEvent
{
    private readonly ReadOnlyBitSet16 _from;
    private readonly ReadOnlyBitSet16 _to;
    private readonly Cell _cell;

    public HighlightChangeEvent(ReadOnlyBitSet16 from, ReadOnlyBitSet16 to, Cell cell)
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