using Model.Core;
using Model.Tectonics.Solver;

namespace Model.Tectonics.Generator;

public class GeneratedTectonicPuzzle
{
    public ITectonic Tectonic { get; }
    
    public bool Evaluated { get; private set; }
    
    public double Rating { get; private set; }
    
    public Strategy<ITectonicSolverData>? Hardest { get; private set; }
    
    public GeneratedTectonicPuzzle(ITectonic tectonic)
    {
        Tectonic = tectonic;
    }

    public void SetEvaluation(double rating, Strategy<ITectonicSolverData>? hardest)
    {
        Hardest = hardest;
        Rating = rating;
        Evaluated = true;
    }

    public string AsString() =>
        TectonicTranslator.TranslateRdFormat(Tectonic);
}