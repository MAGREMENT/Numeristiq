using Model.Sudoku.Player.HistoricEvents;
using Model.Utility;

namespace Model.Sudoku.Player.Actions;

public class SolutionChangeAction : IPlayerAction
{
    private readonly int _number;

    public SolutionChangeAction(int number)
    {
        _number = number;
    }

    public bool CanExecute(IReadOnlyPlayerData data, Cell cell)
    {
        var pc = data[cell];
        return data[cell].Editable && (pc.Number() != 0 || _number != 0);
    }

    public IHistoricEvent? Execute(IPlayerData data, Cell cell)
    {
        var pc = data[cell];
        var old = pc;
        pc.SetNumber(old.Number() == _number ? 0 : _number);
        data[cell] = pc;

        return new CellChangeEvent(old, pc, cell);
    }
}