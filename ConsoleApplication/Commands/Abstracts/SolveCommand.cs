using Model.Core;

namespace ConsoleApplication.Commands.Abstracts;

public abstract class SolveCommand : Command
{
    private const int StringIndex = 0;
    private const int PathIndex = 0;
    
    public override string Description { get; }

    protected SolveCommand(string name) : base("Solve",
        new[]
        {
            new Argument($"{name} string", ValueType.String)
        },
        new[]
        {
            new Option("-p", "Shows solving path")
        })
    {
        Description = $"Solves a {name}";
    }

    protected abstract ISolver GetSolverAndSetPuzzle(ArgumentInterpreter interpreter, string puzzle);

    protected abstract string PuzzleAsString(ISolver solver);
    
    public override void Execute(ArgumentInterpreter interpreter, IReadOnlyCallReport report)
    {
        var solver = GetSolverAndSetPuzzle(interpreter, (string)report.GetArgumentValue(StringIndex));
        if (!report.IsOptionUsed(PathIndex)) solver.FastMode = true;

        Console.WriteLine($"Before : \n{PuzzleAsString(solver)}");
        solver.Solve();
        Console.WriteLine($"After : \n{PuzzleAsString(solver)}");

        if (report.IsOptionUsed(PathIndex))
        {
            Console.WriteLine("\nPath :");
            foreach (var step in solver.Steps)
            {
                var explanation = step.Explanation is null ? "None" : step.Explanation.FullExplanation();
                Console.WriteLine($"{step.Id}. {step.Title}\nDescription : {step.Description}\nChanges :" +
                                  $" {step.ChangesToString()}\nExplanation : {explanation}");
            }
        }
    }
}