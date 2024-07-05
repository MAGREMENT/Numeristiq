using System.Collections.Generic;
using Model.Utility;

namespace Model.Sudokus.Solver.Position;

public interface IReadOnlyMiniGridPositions : IEnumerable<Cell>
{
    public int Count { get; }
    public Cell First();
    public Cell First(Cell except);
    public Cell Next(ref int cursor);
    
    public delegate void HandleCombination(Cell one, Cell two);
    public void ForEachCombination(HandleCombination handler);

    public LinePositions OnGridRow(int gridRow);
    public LinePositions OnGridColumn(int gridCol);
    public bool AreAllInSameRow();
    public bool AreAllInSameColumn();
    
    public bool Contains(int gridRow, int gridCol);
    public bool Contains(int gridNumber);
    
    public MiniGridPositions Or(IReadOnlyMiniGridPositions pos);
    public MiniGridPositions Difference(IReadOnlyMiniGridPositions pos);

    public MiniGridPositions Copy();

    public Cell[] ToCellArray();

    public int GetNumber();
    
    public static MiniGridPositions DefaultOr(IReadOnlyMiniGridPositions one, IReadOnlyMiniGridPositions two)
    {
        var result = new MiniGridPositions(one.GetNumber() / 3, one.GetNumber() % 3);
        for (int i = 0; i < 3; i++)
        {
            for (int j = 0; j < 3; j++)
            {
                if (one.Contains(i, j) || two.Contains(i, j)) result.Add(i, j);
            }
        }

        return result;
    }

    public static MiniGridPositions DefaultDifference(IReadOnlyMiniGridPositions one, IReadOnlyMiniGridPositions two)
    {
        var result = one.Copy();
        foreach (var c in two)
        {
            result.Remove(c.Row % 3, c.Column % 3);
        }

        return result;
    }
}