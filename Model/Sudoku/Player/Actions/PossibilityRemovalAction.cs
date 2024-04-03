using Model.Sudoku.Player.HistoricEvents;
using Model.Utility;

namespace Model.Sudoku.Player.Actions;

public class PossibilityRemovalAction : IPlayerAction
{
    private readonly PossibilitiesLocation _location;

    public PossibilityRemovalAction(PossibilitiesLocation location)
    {
        _location = location;
    }

    public bool CanExecute(IReadOnlyPlayerData data, Cell cell)
    {
        var c =  data.GetCellDataFor(cell);
        return c.Editable && c.PossibilitiesCount(_location) > 0;
    }

    public IHistoricEvent? Execute(IPlayerData data, Cell cell)
    {
        var pc =  data.GetCellDataFor(cell);
        var old = pc;
        pc.RemovePossibility(_location);
        data.SetCellDataFor(cell, pc);
        
        return new CellChangeEvent(old, pc, cell);
    }
}