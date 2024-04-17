using Model.Sudokus.Player.HistoricEvents;
using Model.Utility;

namespace Model.Sudokus.Player.Actions;

public class HighlightChangeAction : IPlayerAction
{
    private readonly int _color;

    public HighlightChangeAction(HighlightColor color)
    {
        _color = (int)color;
    }

    public bool CanExecute(IReadOnlyPlayerData data, Cell cell)
    {
        return true;
    }

    public IHistoricEvent? Execute(IPlayerData data, Cell cell)
    {
        var old = data.GetHighlightsFor(cell);
        var newH = old.Contains(_color) ? old - _color : old + _color;
        data.SetHighlightsFor(cell, newH);

        return new HighlightChangeEvent(old, newH, cell);
    }
}