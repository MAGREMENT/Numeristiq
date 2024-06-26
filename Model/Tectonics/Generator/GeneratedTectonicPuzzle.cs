using Model.Core;
using Model.Core.Generators;

namespace Model.Tectonics.Generator;

public class GeneratedTectonicPuzzle : GeneratedPuzzle<ITectonic>
{ 
    public GeneratedTectonicPuzzle(ITectonic tectonic) : base(tectonic)
    {
    }
    
    public override string AsString() =>
        TectonicTranslator.TranslateRdFormat(Puzzle);
}