namespace Model.Sudokus.Player.HistoricEvents;

public class AllCellsChangeEvent : IHistoricEvent
{
    private readonly PlayerCell[,] _from;
    private readonly PlayerCell[,] _to;

    public AllCellsChangeEvent(PlayerCell[,] from, PlayerCell[,] to)
    {
        _from = from;
        _to = to;
    }

    public void Do(IPlayerData data)
    {
        for (int row = 0; row < 9; row++)
        {
            for (int col = 0; col < 9; col++)
            {
                data.SetCellDataFor(row, col, _to[row, col]);
            }
        }
    }

    public void Reverse(IPlayerData data)
    {
        for (int row = 0; row < 9; row++)
        {
            for (int col = 0; col < 9; col++)
            {
                data.SetCellDataFor(row, col, _from[row, col]);
            }
        }
    }
}