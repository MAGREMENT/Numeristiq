using Model.Sudokus.Player.HistoricEvents;
using Model.Utility;

namespace Model.Sudokus.Player.Actions;

public class CellHighlightChangeAction : ICellAction
{
    private readonly HighlightColor _color;

    public CellHighlightChangeAction(HighlightColor color)
    {
        _color = color;
    }

    public bool CanExecute(IReadOnlyPlayerData data, Cell cell)
    {
        return true;
    }

    public IHistoricEvent Execute(IPlayerData data, Cell cell)
    {
        var old = data.GetHighlightsFor(cell);
        var newH = old.ApplyColorToCell(_color);
        data.SetHighlightsFor(cell, newH);

        return new HighlightChangeEvent(old, newH, cell);
    }
}