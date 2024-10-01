using Model.Sudokus.Solver.Position;
using Model.Sudokus.Solver.PossibilitySets;
using Model.Sudokus.Solver.Utility;
using Model.Sudokus.Solver.Utility.AlmostLockedSets;
using Model.Utility;
using Model.Utility.BitSets;

namespace Tests.Sudokus;

public class ISudokuElementTests
{
    [Test]
    public void HashCodeAndEqualsTest()
    {
        var cp1 = new CellPossibility(1, 2, 5);
        var cp2 = new CellPossibility(1, 2, 5);
        AssertSameHashCodeAndEquals(cp1, cp2);

        var csp1 = new CellsPossibility(3, new Cell(1, 2), new Cell(5, 4), new Cell(2, 8));
        var csp2 = new CellsPossibility(3, new Cell(5, 4), new Cell(1, 2), new Cell(2, 8));
        AssertSameHashCodeAndEquals(csp1, csp2);

        var ns1 = new ArrayPossibilitySet(new CellPossibilities(new Cell(5, 8), 3),
            new CellPossibilities(new Cell(2, 1), new ReadOnlyBitSet16(5, 9, 4)),
            new CellPossibilities(new Cell(8, 2), new ReadOnlyBitSet16(2, 8)),
            new CellPossibilities(new Cell(0, 0), new ReadOnlyBitSet16(5, 8, 3, 1)));
        var ns2 =  new ArrayPossibilitySet(new CellPossibilities(new Cell(0, 0), new ReadOnlyBitSet16(5, 8, 3, 1)),
            new CellPossibilities(new Cell(8, 2), new ReadOnlyBitSet16(2, 8)),
            new CellPossibilities(new Cell(2, 1), new ReadOnlyBitSet16(5, 9, 4)),
            new CellPossibilities(new Cell(5, 8), 3));
        AssertSameHashCodeAndEquals(ns1, ns2);

        var pc1 = new PointingColumn(3, 0, 2, 5, 7);
        var pc2 = new PointingColumn(3, 0, new LinePositions(2, 5, 7));
        AssertSameHashCodeAndEquals(pc1, pc2);
        
        var pr1 = new PointingRow(6, 7, 8, 2, 4);
        var pr2 = new PointingRow(6, 7, new LinePositions(8, 2, 4));
        AssertSameHashCodeAndEquals(pr1, pr2);
    }

    private static void AssertSameHashCodeAndEquals<T>(T one, T two) where T : notnull
    {
        Assert.That(one.GetHashCode(), Is.EqualTo(two.GetHashCode()));
        Assert.That(one, Is.EqualTo(two));
    }
}