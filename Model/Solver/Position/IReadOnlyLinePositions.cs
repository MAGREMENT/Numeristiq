using System.Collections.Generic;
using Global;
using Model.Solver.StrategiesUtil;

namespace Model.Solver.Position;

public interface IReadOnlyLinePositions : IEnumerable<int>
{
    public int Count { get; }
    public int First();
    public int First(int except);
    public int Next(ref int cursor);
    
    public delegate void HandleCombination(int one, int two);
    public void ForEachCombination(HandleCombination handler);

    public bool Peek(int i);
    public LinePositions Or(IReadOnlyLinePositions pos);
    public bool AreAllInSameMiniGrid();
    public int MiniGridCount();
    public LinePositions Copy();
    public string ToString(Unit unit, int unitNumber);
    public Cell[] ToCellArray(Unit unit, int unitNumber);

    public static LinePositions DefaultOr(IReadOnlyLinePositions one, IReadOnlyLinePositions two)
    {
        var result = new LinePositions();
        for (int i = 0; i < 9; i++)
        {
            if (one.Peek(i) || two.Peek(i)) result.Add(i);
        }

        return result;
    }
}