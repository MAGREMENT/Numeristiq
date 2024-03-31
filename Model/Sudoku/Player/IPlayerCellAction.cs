using Model.Utility;

namespace Model.Sudoku.Player;

public interface IPlayerCellAction
{
    /// <summary>
    /// 
    /// </summary>
    /// <returns>True if the action can be executed</returns>
    bool CanExecute(PlayerCell pc, Cell cell);
    
    /// <summary>
    /// 
    /// </summary>
    /// <returns>An event if a significant change was made, null if not</returns>
    IHistoricEvent? Execute(PlayerCell pc, Cell cell, IPlayerCellSetter setter);
}

public interface IPlayerCellSetter
{
    public PlayerCell this[int row, int col] { set; }
    public PlayerCell this[Cell cell] { set; }
}