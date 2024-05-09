using System.Collections.Generic;
using Model.Sudokus.Solver.Position;
using Model.Utility;
using Model.Utility.BitSets;

namespace Model.Sudokus.Solver.Utility.Exocet;

public abstract class DoubleTargetExocet : Exocet
{
    public Cell Target1 { get; }
    public Cell Target2 { get; }

    protected DoubleTargetExocet(Cell base1, Cell base2, Cell target1, Cell target2,
        ReadOnlyBitSet16 baseCandidates, Dictionary<int, GridPositions> sCells) : base(base1, base2, baseCandidates, sCells)
    {
        Target1 = target1;
        Target2 = target2;
    }
}