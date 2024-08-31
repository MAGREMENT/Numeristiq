using System.Text;
using Model.Core;
using Model.Core.Trackers;

namespace ConsoleApplication.Commands.Abstracts;

public abstract class SolveBatchCommand<TState> : Command where TState : class
{
    private const int FileIndex = 0;
    private const int FeedbackIndex = 0;
    private const int FailsIndex = 1;
    private const int InstancesIndex = 2;
    private const int AbsencesIndex = 3;
    private const int LimitIndex = 4;
    private const int OutputIndex = 5;
    
    public override string Description { get; }

    private readonly StatisticsTracker<TState> _statistics = new();
    private string? _line;

    protected SolveBatchCommand(string name) : base("SolveBatch",
        new[]
        {
            new Argument($"Text file containing the {name}'s", ValueType.ReadFile)
        },
        new[]
        {
            new Option("--feedback", $"Gives feedback for each {name}"),
            new Option("--list-fails", "Lists all solver fails"),
            new Option("--list-instances", $"Lists all {name}'s that presented the strategy in their solution path",
                ValueRequirement.Mandatory, ValueType.String),
            new Option("--list-absences", $"Lists all {name}'s that did not present the strategy in their solution path",
                ValueRequirement.Mandatory, ValueType.String),
            new Option("--limit", $"Limits the number of {name} solved", ValueRequirement.Mandatory,
                ValueType.Int),
            new Option("--output", "File to write the output to", 
                ValueRequirement.Mandatory, ValueType.WriteFile)
        })
    {
        Description = $"Solves all the {name}'s in a text file";
    }

    protected abstract ITrackerAttachableSolver<TState> GetSolver(Instantiator instantiator);
 
    protected abstract bool Set(ISolver solver, string asString);

    protected abstract string CurrentToString(ISolver solver);
    
    public override void Execute(ArgumentInterpreter interpreter, IReadOnlyCallReport report)
    {
        var solver = GetSolver(interpreter.Instantiator);
        solver.FastMode = true;

        var limit = report.IsOptionUsed(LimitIndex) ? (int)report.GetOptionValue(LimitIndex)! : int.MaxValue;
        _statistics.AttachTo(solver);

        if (report.IsOptionUsed(FeedbackIndex)) solver.SolveDone += OnSolveDone;

        List<string> fails = new();
        List<string> instances = new();
        List<string> absences = new();
        var writer = report.IsOptionUsed(OutputIndex) ? (StreamWriter)report.GetOptionValue(OutputIndex)! : null;
        UsedStrategiesTracker? usedTracker = null;

        if (report.IsOptionUsed(InstancesIndex) || report.IsOptionUsed(AbsencesIndex))
        {
            usedTracker = new UsedStrategiesTracker();
            usedTracker.AttachTo(solver);
        }
        
        var reader = (StreamReader)report.GetArgumentValue(FileIndex);

        int count = 0;
        while ((_line = reader.ReadLine()) is not null)
        {
            int commentStart = _line.IndexOf('#');
            var s = commentStart == -1 ? _line : _line[..commentStart];

            if (!Set(solver, s))
            {
                Console.WriteLine("Couldn't parse : \"" + s + "\"");    
            }
            
            solver.Solve();

            if (report.IsOptionUsed(FailsIndex) && solver.HasSolverFailed())
            {
                fails.Add(s);
            }

            if (usedTracker is not null)
            {
                if(usedTracker.WasUsed((string)report.GetOptionValue(InstancesIndex)!)) instances.Add(s);
                if(!usedTracker.WasUsed((string)report.GetOptionValue(AbsencesIndex)!)) absences.Add(s);
            }

            writer?.WriteLine(CurrentToString(solver));

            if (++count >= limit) break;
        }

        Console.WriteLine(_statistics);
        _statistics.Detach();

        if (report.IsOptionUsed(FailsIndex))
        {
            if(fails.Count == 0) Console.WriteLine("\nNo fail detected");
            else
            {
                Console.WriteLine($"\n{fails.Count} fails detected");
                for (int i = 0; i < fails.Count; i++)
                {
                    Console.WriteLine($"   {i + 1}: {fails[i]}");
                }
            }
        }
        
        if (report.IsOptionUsed(InstancesIndex))
        {
            if(instances.Count == 0) Console.WriteLine("\nNo instance detected");
            else
            {
                Console.WriteLine($"\n{instances.Count} instances detected");
                foreach (var i in instances)
                {
                    Console.WriteLine(i);
                }
            }
        }
        
        if (report.IsOptionUsed(AbsencesIndex))
        {
            if(absences.Count == 0) Console.WriteLine("\nNo absence detected");
            else
            {
                Console.WriteLine($"\n{absences.Count} absences detected");
                foreach (var a in absences)
                {
                    Console.WriteLine(a);
                }
            }
        }
    }

    private void OnSolveDone(ISolveResult<TState> result)
    {
        Console.Write($"#{_statistics.Count} ");
        if(result.IsResultCorrect()) Console.WriteLine("Ok !");
        else
        {
            Console.Write(result.HasSolverFailed() ? "Solver failed" : "Solver did not find solution");
            Console.WriteLine($" => {_line}");
        }
    }
}