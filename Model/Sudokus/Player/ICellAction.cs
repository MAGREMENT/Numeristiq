using Model.Utility;

namespace Model.Sudokus.Player;

public interface ICellAction
{
    /// <summary>
    /// 
    /// </summary>
    /// <returns>True if the action can be executed</returns>
    bool CanExecute(IReadOnlyPlayerData data, Cell cell);
    
    /// <summary>
    /// 
    /// </summary>
    /// <returns>An event if a significant change was made, null if not</returns>
    IHistoricEvent? Execute(IPlayerData data, Cell cell);
}

public interface ICellPossibilityAction
{
    /// <summary>
    /// 
    /// </summary>
    /// <returns>True if the action can be executed</returns>
    bool CanExecute(IReadOnlyPlayerData data, CellPossibility cp);
    
    /// <summary>
    /// 
    /// </summary>
    /// <returns>An event if a significant change was made, null if not</returns>
    IHistoricEvent? Execute(IPlayerData data, CellPossibility cp);
}

public interface IGlobalAction
{
    /// <summary>
    /// 
    /// </summary>
    /// <returns>True if the action can be executed</returns>
    bool CanExecute(IReadOnlyPlayerData data);
    
    /// <summary>
    /// 
    /// </summary>
    /// <returns>An event if a significant change was made, null if not</returns>
    IHistoricEvent? Execute(IPlayerData data);
}