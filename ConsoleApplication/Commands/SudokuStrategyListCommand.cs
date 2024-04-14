using Model.Sudoku.Solver;

namespace ConsoleApplication.Commands;

public class SudokuStrategyListCommand : Command
{
    public SudokuStrategyListCommand() : base("StrategyList")
    {
    }

    public override string Description => "Gets the list of every Sudoku strategy implemented";
    
    public override void Execute(IReadOnlyArgumentInterpreter interpreter, IReadOnlyCallReport report)
    {
        int count = 1;
        foreach (var s in StrategyPool.EnumerateStrategies())
        {
            Console.WriteLine($"#{count++} {s}");
        }
    }
}