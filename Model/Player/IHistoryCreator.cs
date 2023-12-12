namespace Model.Player;

public interface IHistoryCreator
{
    void ShowHistoricPoint(HistoricPoint point);
    public PlayerCell this[int row, int column] { get; }
}