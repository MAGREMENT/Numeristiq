namespace Model.Core.Generators;

public abstract class GeneratedPuzzle<T>
{
    public T Puzzle { get;}
    
    public bool Evaluated { get; private set; }
    
    public double Rating { get; private set; }
    
    public Strategy? Hardest { get; private set; }

    protected GeneratedPuzzle(T puzzle)
    {
        Puzzle = puzzle;
    }

    public void SetEvaluation(double rating, Strategy? hardest)
    {
        Hardest = hardest;
        Rating = rating;
        Evaluated = true;
    }

    public abstract string AsString();
}