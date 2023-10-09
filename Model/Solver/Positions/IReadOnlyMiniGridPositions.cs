using System.Collections.Generic;
using Model.Solver.StrategiesUtil;

namespace Model.Solver.Positions;

public interface IReadOnlyMiniGridPositions : IEnumerable<Cell>
{
    public int Count { get; }
    public Cell GetFirst();
    public Cell Next(ref int cursor);
    
    public delegate void HandleCombination(Cell one, Cell two);
    public void ForEachCombination(HandleCombination handler);

    public LinePositions OnGridRow(int gridRow);
    public LinePositions OnGridColumn(int gridCol);
    public bool AreAllInSameRow();
    public bool AreAllInSameColumn();
    
    public bool Peek(int gridRow, int gridCol);
    public bool Peek(int gridNumber);
    
    public MiniGridPositions Or(IReadOnlyMiniGridPositions pos);

    public MiniGridPositions Copy();

    public Cell[] ToCellArray();

    public int MiniGridNumber();
    
    public static MiniGridPositions DefaultOr(IReadOnlyMiniGridPositions one, IReadOnlyMiniGridPositions two)
    {
        var result = new MiniGridPositions(one.MiniGridNumber() / 3, one.MiniGridNumber() % 3);
        for (int i = 0; i < 3; i++)
        {
            for (int j = 0; j < 3; j++)
            {
                if (one.Peek(i, j) || two.Peek(i, j)) result.Add(i, j);
            }
        }

        return result;
    }
}