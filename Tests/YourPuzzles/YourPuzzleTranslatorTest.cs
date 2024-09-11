using Model.Utility;
using Model.YourPuzzles;
using Model.YourPuzzles.Rules;

namespace Tests.YourPuzzles;

public class YourPuzzleTranslatorTest
{
    [Test]
    public void Test()
    {
        var cells = new Cell[] { new(5, 6), new(5, 7), new(4, 7) };
        
        var puzzle = new NumericYourPuzzle(7, 8);
        puzzle.AddRuleUnchecked(new NonRepeatingLinesNumericPuzzleRule());
        puzzle.AddRuleUnchecked(new GreaterThanNumericPuzzleRule(new Cell(1, 1), new Cell(1, 2)));
        puzzle.AddRuleUnchecked(new NonRepeatingBatchNumericPuzzleRule(cells));

        var s = YourPuzzleTranslator.TranslateLineFormat(puzzle);
        Console.WriteLine(s);

        var back = YourPuzzleTranslator.TranslateLineFormat(s);
        
        Assert.That(back.RowCount, Is.EqualTo(7));
        Assert.That(back.ColumnCount, Is.EqualTo(8));
        Assert.That(back.GlobalRules, Has.Count.EqualTo(1));
        Assert.That(back.LocalRules, Has.Count.EqualTo(2));

        var sBack = YourPuzzleTranslator.TranslateLineFormat(back);
        Console.WriteLine(sBack);
        Assert.That(sBack, Is.EqualTo(s));
    }
}