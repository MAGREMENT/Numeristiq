using Model.Core;
using Model.Sudokus;
using Model.Sudokus.Solver;
using Model.Sudokus.Solver.Position;
using Model.Sudokus.Solver.Utility;
using Model.Utility;

namespace Tests;

public class TruthAndLinksLogicTests
{
    [Test]
    public void XWingTest()
    {
        var state = SudokuTranslator.TranslateBase32Format(
            "t0o803p00h60p02805tgoot0p00560p0280321o00503p009410hp0sgogs088p8032105l00511s0og21gg0903k00321090541h00hp0p0h8052141h88103h00hp0030h21h005p04109p841p01o03hg05p021",
            new AlphabeticalBase32Translator());
        var solver = new SudokuSolver();
        solver.SetState(state);

        var bank = new DefaultTruthAndLinkBank<Cell, HouseCellsSudokuTruthOrLink>();
        SudokuTruthAndLinksLogic.AddTruthAndLinks(solver, bank, 5, true);
        var result = TruthAndLinksLogic.FindRank0(bank, 2, true,
            () => new GridPositions());

        Console.WriteLine("Result count : " + result.Count());
        Assert.That(FindXWing(result), Is.True);
    }

    private static bool FindXWing(IEnumerable<(HouseCellsSudokuTruthOrLink[], HouseCellsSudokuTruthOrLink[])> result)
    {
        foreach (var (truths, links) in result)
        {
            if(truths.Length != 2 || links.Length != 2) continue;

            HashSet<House> truthSet = new();
            HashSet<House> linkSet = new();
            foreach (var truth in truths) truthSet.Add(truth.House);
            foreach (var link in links) linkSet.Add(link.House);

            if (truthSet.Contains(new House(Unit.Row, 2)) && truthSet.Contains(new House(Unit.Row, 3))
                                                          && linkSet.Contains(new House(Unit.Column, 4)) &&
                                                          linkSet.Contains(new House(Unit.Column, 8))) return true;
        }

        return false;
    }
}