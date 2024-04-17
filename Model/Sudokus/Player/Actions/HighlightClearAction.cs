using Model.Sudokus.Player.HistoricEvents;
using Model.Utility;
using Model.Utility.BitSets;

namespace Model.Sudokus.Player.Actions;

public class HighlightClearAction : IPlayerAction
{
    public bool CanExecute(IReadOnlyPlayerData data, Cell cell)
    {
        return data.GetHighlightsFor(cell).Count > 0;
    }

    public IHistoricEvent? Execute(IPlayerData data, Cell cell)
    {
        var old = data.GetHighlightsFor(cell);
        data.SetHighlightsFor(cell, new ReadOnlyBitSet16());

        return new HighlightChangeEvent(old, new ReadOnlyBitSet16(), cell);
    }
}