namespace Model.Core.Generators;

public interface IPuzzleGenerator<out T>
{
    public event OnNextStep? StepDone;
    
    public bool KeepSymmetry { get; set; }
    
    public bool KeepUniqueness { get; set; }
    
    public T Generate();

    public T[] Generate(int count);
}

public delegate void OnNextStep();