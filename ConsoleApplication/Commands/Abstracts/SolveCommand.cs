using Model.Core;
using Model.Core.Changes;
using Model.Core.Highlighting;

namespace ConsoleApplication.Commands.Abstracts;

public abstract class SolveCommand<TStrategy, TSolvingState, THighlighter> : Command
    where TStrategy : Strategy where TSolvingState : IUpdatableNumericSolvingState where THighlighter : INumericSolvingStateHighlighter
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

    protected abstract NumericStrategySolver<TStrategy, TSolvingState, THighlighter> GetSolverAndSetPuzzle(
        ArgumentInterpreter interpreter, string puzzle);

    protected abstract string PuzzleAsString(NumericStrategySolver<TStrategy, TSolvingState, THighlighter> solver);
    
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
                                  $" {ChangeReportHelper.ChangesToString(step.Changes)}\nExplanation : {explanation}");
            }
        }
    }
}