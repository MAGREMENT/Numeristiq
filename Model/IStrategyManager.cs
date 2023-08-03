using System.Collections.Generic;
using Model.Positions;
using Model.Possibilities;
using Model.StrategiesUtil;
using Model.StrategiesUtil.LoopFinder;

namespace Model;

public interface IStrategyManager
{
    bool AddDefinitiveNumber(int number, int row, int col, IStrategy strategy);

    bool RemovePossibility(int possibility, int row, int col, IStrategy strategy);

    ChangeBuffer CreateChangeBuffer(IStrategy current, IChangeCauseFactory causeFactory);

    LinePositions PossibilityPositionsInColumn(int col, int number);

    LinePositions PossibilityPositionsInRow(int row, int number);

    MiniGridPositions PossibilityPositionsInMiniGrid(int miniRow, int miniCol, int number);
    
    public LinkGraph<ILinkGraphElement> LinkGraph();

    public Sudoku Sudoku { get; }

    public IPossibilities[,] Possibilities { get; }

    public Solver Copy();
}





