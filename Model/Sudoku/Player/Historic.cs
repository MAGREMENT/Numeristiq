namespace Model.Sudoku.Player;

public class Historic
{
    public void AddNewEvent(IHistoricEvent e)
    {
        //TODO
    }

    public void Clear()
    {
        //TODO
    }
}

public interface IHistoricEvent
{
    void Do(IPlayerData data);
    void Reverse(IPlayerData data);
}