using System.Collections.Generic;
using Model.Changes;
using Model.Positions;
using Model.Possibilities;
using Model.StrategiesUtil;
using Model.StrategiesUtil.LinkGraph;

namespace Model.Solver;

public interface IStrategyManager : IPossibilitiesHolder
{
    bool AddDefinitiveNumber(int number, int row, int col, IStrategy strategy);

    bool RemovePossibility(int possibility, int row, int col, IStrategy strategy);

    ChangeBuffer ChangeBuffer { get; }

    LinePositions ColumnPositions(int col, int number);

    LinePositions RowPositions(int row, int number);

    MiniGridPositions MiniGridPositions(int miniRow, int miniCol, int number);

    public List<AlmostLockedSet> AlmostLockedSets();
    
    public LinkGraph<ILinkGraphElement> LinkGraph();

    public Dictionary<ILinkGraphElement, Coloring> OnColoring(int row, int col, int possibility);

    public Dictionary<ILinkGraphElement, Coloring> OffColoring(int row, int col, int possibility);

    public Sudoku Sudoku { get; }

    public IPossibilities[,] Possibilities { get; }

    public Model.Solver.Solver Copy();
}





