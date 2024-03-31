using Model.Utility;

namespace Model.Sudoku.Player.HistoricEvents;

public class NumberChangeEvent : IHistoricEvent
{
    private readonly int _from;
    private readonly int _to;
    private readonly Cell _cell;

    public NumberChangeEvent(int from, int to, Cell cell)
    {
        _from = from;
        _to = to;
        _cell = cell;
    }

    public static NumberChangeEvent? TryCreate(int from, int to, Cell cell)
    {
        return from == to ? null : new NumberChangeEvent(from, to, cell);
    }

    public void Do(IPlayerCellSetter setter)
    {
        var pc = new PlayerCell();
        pc.SetNumber(_to);
        setter[_cell] = pc;
    }

    public void Reverse(IPlayerCellSetter setter)
    {
        var pc = new PlayerCell();
        pc.SetNumber(_from);
        setter[_cell] = pc;
    }
}