namespace Model.Sudoku.Player;

public interface IHistoryCreator : IPlayerState
{
    void ShowHistoricPoint(HistoricPoint point);
}