using System.Collections.Generic;

namespace Model.Positions;

public interface IReadOnlyLinePositions : IEnumerable<int>
{
    public int Count { get; }
}