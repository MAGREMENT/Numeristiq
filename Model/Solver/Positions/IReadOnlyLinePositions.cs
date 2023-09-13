using System.Collections.Generic;

namespace Model.Solver.Positions;

public interface IReadOnlyLinePositions : IEnumerable<int>
{
    public int Count { get; }
    public int GetFirst();
    public int Next(ref int cursor);
    
    public delegate void HandleCombination(int one, int two);
    public void ForEachCombination(HandleCombination handler);

    public bool Peek(int i);
    public LinePositions Or(IReadOnlyLinePositions pos);
    public bool AreAllInSameMiniGrid();
    public LinePositions Copy();
    public string ToString(Unit unit, int unitNumber);

    public static LinePositions DefaultOr(IReadOnlyLinePositions one, IReadOnlyLinePositions two)
    {
        return new LinePositions(); //TODO
    }
}