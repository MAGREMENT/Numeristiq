using Model.Sudokus.Player.HistoricEvents;
using Model.Utility;

namespace Model.Sudokus.Player.Actions;

public class CellPossibilityHighlightChangeAction : ICellAction
{
    private readonly HighlightColor _color;
    private readonly PossibilitiesLocation _location;
    private readonly int _p;

    public CellPossibilityHighlightChangeAction(HighlightColor color, PossibilitiesLocation location, int p)
    {
        _color = color;
        _location = location;
        _p = p;
    }

    public bool CanExecute(IReadOnlyPlayerData data, Cell cell)
    {
        return true;
    }

    public IHistoricEvent Execute(IPlayerData data, Cell cell)
    {
        var old = data.GetHighlightsFor(cell);
        var newH = old.ApplyColorToPossibility(_p, _color, _location);
        data.SetHighlightsFor(cell, newH);

        return new HighlightChangeEvent(old, newH, cell);
    }
}