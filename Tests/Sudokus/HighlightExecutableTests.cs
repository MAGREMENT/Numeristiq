using Model.Core.Changes;
using Model.Core.Graphs;
using Model.Core.Highlighting;
using Model.Sudokus;
using Model.Sudokus.Solver;
using Model.Sudokus.Solver.PossibilitySets;
using Model.Sudokus.Solver.Utility;
using Model.Utility;
using Model.Utility.BitSets;

namespace Tests.Sudokus;

public class HighlightExecutableTests //TODO
{
    [Test]
    public void SimpleTests()
    {
        var compiler = new SudokuHighlightCompiler();
        Test(compiler, lighter =>
        {
            lighter.HighlightCell(1, 3, StepColor.Cause10);
            lighter.HighlightPossibility(4, 5, 7, StepColor.Cause4);
            lighter.EncircleHouse(new House(Unit.Box, 8), StepColor.On);
            lighter.HighlightElement(new PointingRow(5, 2, 3, 4, 5), StepColor.Change1);
            lighter.HighlightElement(new ArrayPossibilitySet(new CellPossibilities(2, 3, 4),
                new CellPossibilities(5, 7, ReadOnlyBitSet16.Filled(1, 9)),
                new CellPossibilities(6, 7, ReadOnlyBitSet16.Filled(5, 7))), StepColor.Cause8);
        });
    }
    
    private static void Test(IHighlightCompiler<ISudokuHighlighter> compiler, Highlight<ISudokuHighlighter> highlightable)
    {
        Test(compiler, new DelegateHighlightable<ISudokuHighlighter>(highlightable));
    }
    
    private static void Test(IHighlightCompiler<ISudokuHighlighter> compiler, IHighlightable<ISudokuHighlighter> highlightable)
    {
        var compiled = compiler.Compile(highlightable);
        var th1 = new TestHighlighter();
        highlightable.Highlight(th1);
        var th2 = new TestHighlighter();
        compiled.Highlight(th2);

        Assert.That(th1, Has.Count.EqualTo(th2.Count));
        foreach (var t in th1)
        {
            var index = th2.IndexOf(t);
            Assert.That(index, Is.Not.Negative);

            th2.RemoveAt(index);
        }
    }
}

public class TestHighlighter : List<object>, ISudokuHighlighter
{
    public void HighlightPossibility(int possibility, int row, int col, StepColor color)
    {
        Add((new CellPossibility(row, col, possibility), color));
    }

    public void HighlightCell(int row, int col, StepColor color)
    {
        Add((new Cell(row, col), color));
    }

    public void CreateLink(CellPossibility from, CellPossibility to, LinkStrength linkStrength)
    {
        Add((from, to, linkStrength));
    }

    public void EncirclePossibility(int possibility, int row, int col)
    {
        Add(new CellPossibility(row, col, possibility));
    }

    public void EncircleCell(int row, int col)
    {
        Add(new Cell(row, col));
    }

    public void EncircleHouse(House house, StepColor color)
    {
        Add((house, color));
    }

    public void HighlightElement(ISudokuElement element, StepColor color)
    {
        Add((element, color));
    }

    public void CreateLink(ISudokuElement from, ISudokuElement to, LinkStrength linkStrength)
    {
        Add((from, to, linkStrength));
    }

    public void CreateLink(IPossibilitySet from, IPossibilitySet to, int link)
    {
        Add((from, to, link));
    }
}