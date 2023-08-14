using System.Collections.Generic;
using Model.Positions;
using Model.Possibilities;
using Model.StrategiesUtil;

namespace Model;

public interface IStrategyManager
{
    bool AddDefinitiveNumber(int number, int row, int col, IStrategy strategy);

    bool RemovePossibility(int possibility, int row, int col, IStrategy strategy);

    ChangeBuffer ChangeBuffer { get; }

    LinePositions PossibilityPositionsInColumn(int col, int number);

    LinePositions PossibilityPositionsInRow(int row, int number);

    MiniGridPositions PossibilityPositionsInMiniGrid(int miniRow, int miniCol, int number);

    public List<AlmostLockedSet> AllAls();
    
    public LinkGraph<ILinkGraphElement> LinkGraph();

    public Dictionary<ILinkGraphElement, Coloring> OnColoring(int row, int col, int possibility);

    public Dictionary<ILinkGraphElement, Coloring> OffColoring(int row, int col, int possibility);

    public Sudoku Sudoku { get; }

    public IPossibilities[,] Possibilities { get; }

    public Solver Copy();
}





