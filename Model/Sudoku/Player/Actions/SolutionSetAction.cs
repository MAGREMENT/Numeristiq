using Model.Sudoku.Player.HistoricEvents;
using Model.Utility;

namespace Model.Sudoku.Player.Actions;

public class SolutionSetAction : IPlayerCellAction
{
    private readonly int _number;

    public SolutionSetAction(int number)
    {
        _number = number;
    }

    public bool CanExecute(PlayerCell pc, Cell cell)
    {
        return pc.Editable;
    }

    public IHistoricEvent? Execute(PlayerCell pc, Cell cell, IPlayerCellSetter setter)
    {
        var old = pc.Number();
        pc.SetNumber(_number);
        setter[cell] = pc;

        return NumberChangeEvent.TryCreate(old, _number, cell);
    }
}