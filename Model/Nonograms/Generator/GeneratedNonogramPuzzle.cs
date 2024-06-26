using Model.Core.Generators;

namespace Model.Nonograms.Generator;

public class GeneratedNonogramPuzzle : GeneratedPuzzle<Nonogram>
{
    public GeneratedNonogramPuzzle(Nonogram puzzle) : base(puzzle)
    {
    }

    public override string AsString()
    {
        return NonogramTranslator.TranslateLineFormat(Puzzle);
    }
}