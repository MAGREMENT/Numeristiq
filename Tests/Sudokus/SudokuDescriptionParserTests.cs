using Model.Core.Descriptions;
using Model.Sudokus.Solver.Descriptions;
using Model.Utility;

namespace Tests.Sudokus;

public class SudokuDescriptionParserTests
{
    [Test]
    public void Test()
    {
        var description = new DescriptionCollection<ISudokuDescriptionDisplayer>().Add(
                "An aligned pair exclusion is the removal of candidate(s) using the following logic : " +
                "Any two cells cannot have solutions that are candidates contained in an almost locked set they both see.")
            .Add(new SudokuStepDescription("Let's take these two boxes as an example where r45c1 is the target. There are 3 ways to make 5 fit into r4c1 : \r\n\r\n" +
                                           "    5 4 -> Impossible because of r56c3\r\n" +
                                           "    5 8 -> Impossible because of r12c1\r\n" +
                                           "    5 9 -> Impossible because of r12c1\r\n\r\n" +
                                           "We can therefore safely remove 5 from r4c1",
                "t0o803p00h60p02805tgoot0p00560p0280321o00503p009410hp0sgogs098p8032105l00511s0og21gg0903k00321090541h00hp0p0h8052141h88103h00hp0030h21h005p04109p841p01o03hg05p021",
                0, 5, 3, 5, 
                "bbaddaaabbaedaaaebaefaaaebaffaaafbaadaaafbabdaaadafddaaa", TextDisposition.Left));

        var path = PathFinder.Find(@"\Data\XML\Tests\Sudoku.xml", true, false);
        Assert.That(path, Is.Not.Null);

        var parsed = SudokuDescriptionParser.FromXML(path!);
        Assert.That(description, Is.EqualTo(parsed));
    }
}