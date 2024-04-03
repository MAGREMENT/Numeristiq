using Model.Sudoku.Player.HistoricEvents;
using Model.Utility;

namespace Model.Sudoku.Player.Actions;

public class PossibilityChangeAction : IPlayerAction
{
    private readonly int _number;
    private readonly PossibilitiesLocation _location;

    public PossibilityChangeAction(int number, PossibilitiesLocation location)
    {
        _number = number;
        _location = location;
    }

    public bool CanExecute(IReadOnlyPlayerData data, Cell cell)
    {
        return data.GetCellDataFor(cell).Editable;
    }

    public IHistoricEvent? Execute(IPlayerData data, Cell cell)
    {
        var pc =  data.GetCellDataFor(cell);
        var old = pc;
        if(pc.PeekPossibility(_number, _location)) pc.RemovePossibility(_number, _location);
        else pc.AddPossibility(_number, _location);
        data.SetCellDataFor(cell, pc);
        
        return new CellChangeEvent(old, pc, cell);
    }
}