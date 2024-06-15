namespace Model.Tectonics.Generator;

public class GeneratedTectonicPuzzle
{
    public ITectonic Tectonic { get; }
    
    public bool Evaluated { get; private set; }
    
    public double Rating { get; private set; }
    
    public TectonicStrategy? Hardest { get; private set; }
    
    public GeneratedTectonicPuzzle(ITectonic tectonic)
    {
        Tectonic = tectonic;
    }

    public void SetEvaluation(double rating, TectonicStrategy? hardest)
    {
        Hardest = hardest;
        Rating = rating;
        Evaluated = true;
    }

    public string AsString() =>
        TectonicTranslator.TranslateRdFormat(Tectonic);
}