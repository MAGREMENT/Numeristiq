using Model.Core.Generators;

namespace Model.Binairos;

public class GeneratedBinairoPuzzle : GeneratedPuzzle<Binairo>
{
    public GeneratedBinairoPuzzle(Binairo puzzle) : base(puzzle)
    {
    }

    public override string AsString()
    {
        return BinairoTranslator.TranslateLineFormat(Puzzle);
    }
}