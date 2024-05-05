using System.Collections.Generic;
using Model.Sudokus.Solver.Position;
using Model.Utility;
using Model.Utility.BitSets;

namespace Model.Sudokus.Solver.Utility.Exocet;

public abstract class Exocet
{
    public Cell Base1 { get; }
    public Cell Base2 { get; }
    public Cell Target1 { get; }
    public Cell Target2 { get; }
    public ReadOnlyBitSet16 BaseCandidates { get; }
    public Dictionary<int, GridPositions> SCells { get; }
    
    
    protected Exocet(Cell base1, Cell base2, Cell target1, Cell target2, ReadOnlyBitSet16 baseCandidates, Dictionary<int, GridPositions> sCells)
    {
        Base1 = base1;
        Target1 = target1;
        Target2 = target2;
        BaseCandidates = baseCandidates;
        SCells = sCells;
        Base2 = base2;
    }

    public abstract List<Cell> AllPossibleSCells();
    
    public Unit GetUnit()
    {
        return Base1.Row == Base2.Row ? Unit.Row : Unit.Column;
    }
    
    public Dictionary<int, List<House>> ComputeAllCoverHouses()
    {
        var result = new Dictionary<int, List<House>>();

        foreach (var possibility in BaseCandidates.EnumeratePossibilities())
        {
            result.Add(possibility, ComputeCoverHouses(possibility));
        }

        return result;
    }

    public List<House> ComputeCoverHouses(int possibility)
    {
        if (!BaseCandidates.Contains(possibility)) return new List<House>();

        return SCells[possibility].BestCoverHouses(MethodsInPriorityOrder());
    }

    private IUnitMethods[] MethodsInPriorityOrder()
    {
        return GetUnit() == Unit.Row
            ? new IUnitMethods[] { new RowMethods(), new ColumnMethods() } 
            : new IUnitMethods[] { new ColumnMethods(), new RowMethods() };
    }
}