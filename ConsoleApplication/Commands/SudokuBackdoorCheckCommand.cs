using Model.Sudokus;
using Model.Sudokus.Generator;
using Model.Sudokus.Solver;
using Model.Sudokus.Solver.Strategies;
using Model.Sudokus.Solver.Strategies.AlternatingInference.Types;
using Model.Utility;

namespace ConsoleApplication.Commands;

public class SudokuBackdoorCheckCommand : Command
{
    private const int StringIndex = 0;
    private const int SetIndex = 0;
    
    public SudokuBackdoorCheckCommand() : base("Backdoor", 
        new []
        {
            new Argument("Sudoku string", ValueType.String)
        },
        new[]
        {
            new Option("--strategy-set", "Sets the strategies used to determine the backdoor (singles or ssts)",
                ValueRequirement.Mandatory, ValueType.String)
        }) { }

    public override string Description => "Checks if the Sudoku has a possible backdoor";
    public override void Execute(ArgumentInterpreter interpreter, IReadOnlyCallReport report)
    {
        var solver = new SudokuSolver();
        if (report.IsOptionUsed(SetIndex))
        {
            switch (((string)report.GetOptionValue(SetIndex)!).ToLower())
            {
                case "singles" :
                    AddSinglesStrategies(solver);
                    break;
                case "ssts" :
                    AddSSTSStrategies(solver);
                    break;
                default:
                    Console.WriteLine("The strategy set value must be either 'singles' or 'ssts'");
                    return;
            }
        }
        else AddSinglesStrategies(solver);

        var sudoku = SudokuTranslator.TranslateLineFormat((string)report.GetArgumentValue(StringIndex));

        var solutions = BackTracking.Solutions(sudoku, ConstantPossibilitiesGiver.Instance, int.MaxValue);
        if (solutions.Count == 0)
        {
            Console.WriteLine("The sudoku has no solution");
            return;
        }

        int count = 0;
        for (int row = 0; row < 9; row++)
        {
            for (int col = 0; col < 9; col++)
            {
                if (sudoku[row, col] != 0) continue;

                foreach (var solution in solutions)
                {
                    var copy = sudoku.Copy();
                    copy[row, col] = solution[row, col];

                    solver.SetSudoku(copy);
                    solver.Solve();

                    if (copy.IsCorrect())
                    {
                        Console.WriteLine($"Backdoor #{++count} : {solution[row, col]}r{row + 1}c{col + 1}");
                    }
                }
            }
        }
        
        if(count == 0) Console.WriteLine("This sudoku has no backdoor");
    }

    private static void AddSinglesStrategies(SudokuSolver solver)
    {
        solver.StrategyManager.AddStrategies(new NakedSingleStrategy(), new HiddenSingleStrategy());
    }

    private static void AddSSTSStrategies(SudokuSolver solver)
    {
        solver.StrategyManager.AddStrategies(new NakedSingleStrategy(), new HiddenSingleStrategy(),
            new NakedDoublesStrategy(), new HiddenDoublesStrategy(), new NakedSetStrategy(3),
            new HiddenSetStrategy(3), new NakedSetStrategy(4), new HiddenSetStrategy(4),
            new PointingSetStrategy(), new ClaimingSetStrategy(), new XWingStrategy(), new GridFormationStrategy(3),
            new GridFormationStrategy(4), new SimpleColoringStrategy(), StrategyPool.CreateFrom(XType.OfficialLoopName)!,
            new XYWingStrategy());
    }
}