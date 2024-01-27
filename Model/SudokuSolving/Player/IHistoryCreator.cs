namespace Model.SudokuSolving.Player;

public interface IHistoryCreator : IPlayerState
{
    void ShowHistoricPoint(HistoricPoint point);
}