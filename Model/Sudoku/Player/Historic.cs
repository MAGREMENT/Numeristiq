namespace Model.Sudoku.Player;

public class Historic
{
    public void AddNewEvent(IHistoricEvent e)
    {
        //TODO
    }
}

public interface IHistoricEvent
{
    void Do(IPlayerCellSetter setter);
    void Reverse(IPlayerCellSetter setter);
}