using Model.Core.BackTracking;

namespace ConsoleApplication.Commands.Abstracts;

public abstract class SolutionCountCommand<TPuzzle, TData> : Command where TPuzzle : ICopyable<TPuzzle>
{
    private const int StringIndex = 0;
    
    protected abstract BackTracker<TPuzzle, TData> BackTracker { get; }
    public override string Description { get; }
    
    protected SolutionCountCommand(string name) : base("SolutionCount",
        new Argument(name + " string", ValueType.String))
    {
        Description = "Counts the number of solutions for a given " + name;
    }
    
    public override void Execute(ArgumentInterpreter interpreter, IReadOnlyCallReport report)
    {
        SetBackTracker((string)report.GetArgumentValue(StringIndex));
        var result = BackTracker.Solutions();
        
        Console.WriteLine($"Number of solutions : {result.Count}\n");

        foreach (var s in result)
        {
            Console.WriteLine(ToString(s) + "\n");
        }
    }

    protected abstract void SetBackTracker(string s);
    protected abstract string ToString(TPuzzle puzzle);
}