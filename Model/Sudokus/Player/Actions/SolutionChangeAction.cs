﻿using Model.Sudokus.Player.HistoricEvents;
using Model.Utility;

namespace Model.Sudokus.Player.Actions;

public class SolutionChangeAction : ICellAction
{
    private readonly int _number;

    public SolutionChangeAction(int number)
    {
        _number = number;
    }

    public bool CanExecute(IReadOnlyPlayerData data, Cell cell)
    {
        var pc =  data.GetCellDataFor(cell);
        return pc.Editable && (pc.Number() != 0 || _number != 0);
    }

    public IHistoricEvent Execute(IPlayerData data, Cell cell)
    {
        var pc =  data.GetCellDataFor(cell);
        var old = pc;
        pc.SetNumber(old.Number() == _number ? 0 : _number);
        data.SetCellDataFor(cell, pc);

        return new CellChangeEvent(old, pc, cell);
    }
}