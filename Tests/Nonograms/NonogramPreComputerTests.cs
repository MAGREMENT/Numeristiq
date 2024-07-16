using Model.Core;
using Model.Core.Changes;
using Model.Core.Highlighting;
using Model.Nonograms;
using Model.Nonograms.Solver;

namespace Tests.Nonograms;

public class NonogramPreComputerTests
{
    [Test]
    public void TestValueSpaces()
    {
        var n = NonogramTranslator.TranslateLineFormat("1-3-1.1-2.1-2::1.1-4-1-1-3");
        n[0, 3] = true;
        var testData = new NonogramSolverTestData(n, (_, c) => c is not 2 and not 3);
        var pc = new NonogramPreComputer(testData);
        pc.AdaptToNewSize(n.RowCount, n.ColumnCount);

        var spaces = pc.HorizontalValueSpaces(0);

        Assert.That(spaces, Has.Count.EqualTo(2));
        Assert.That(spaces[0], Is.EqualTo(new ValueSpace(0, 1, 1)));
        Assert.That(spaces[1], Is.EqualTo(new ValueSpace(3, 3, 1)));

        n = NonogramTranslator.TranslateLineFormat("2-2-1.1.1-1.1-1::2-1-1.1-2.1-2");
        n[0, 2] = true;
        testData = new NonogramSolverTestData(n, (_, c) => c != 2);
        pc = new NonogramPreComputer(testData);
        pc.AdaptToNewSize(n.RowCount, n.ColumnCount);
        
        spaces = pc.HorizontalValueSpaces(0);
        
        Assert.That(spaces, Has.Count.EqualTo(1));
        Assert.That(spaces[0], Is.EqualTo(new ValueSpace(1, 3, 2)));
        
        spaces = pc.HorizontalValueSpaces(3);
        
        Assert.That(spaces, Has.Count.EqualTo(2));
        Assert.That(spaces[0], Is.EqualTo(new ValueSpace(0, 1, 2)));
        Assert.That(spaces[1], Is.EqualTo(new ValueSpace(3, 4, 1)));
    }
}

public class NonogramSolverTestData : INonogramSolverData
{
    private readonly IsAvailable _availability;
    private readonly Nonogram _nonogram;

    public NonogramSolverTestData(Nonogram nonogram, IsAvailable availability)
    {
        _availability = availability;
        _nonogram = nonogram;
    }

    public DichotomousChangeBuffer<INonogramSolvingState, INonogramHighlighter> ChangeBuffer => null!;
    public IReadOnlyNonogram Nonogram => _nonogram;
    public bool IsAvailable(int row, int col)
    {
        return _availability(row, col);
    }
    public NonogramPreComputer PreComputer => null!;
}

public delegate bool IsAvailable(int Throws, int col);