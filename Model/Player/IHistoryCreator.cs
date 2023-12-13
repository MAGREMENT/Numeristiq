namespace Model.Player;

public interface IHistoryCreator : IPlayerState
{
    void ShowHistoricPoint(HistoricPoint point);
}