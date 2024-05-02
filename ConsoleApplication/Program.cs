using ConsoleApplication.Commands;

namespace ConsoleApplication;

public static class Program
{
    public static void Main(string[] args)
    {
        Interpreter().Execute(args);
    }

    private static ArgumentInterpreter? _instance;
    
    private static ArgumentInterpreter Interpreter()
    {
        if (_instance is null)
        {
            _instance = new ArgumentInterpreter();
            _instance.Root.AddCommand(new HelpCommand(), true)
                .AddCommand(new SessionCommand());

            _instance.Root.AddDirectory(new Directory("Sudoku"))
                .AddCommand(new HelpCommand(), true)
                .AddCommand(new SudokuSolveCommand())
                .AddCommand(new SudokuSolveBatchCommand())
                .AddCommand(new SudokuGenerateBatchCommand())
                .AddCommand(new SudokuStrategyListCommand())
                .AddCommand(new SudokuSolutionCountCommand())
                .AddCommand(new SudokuBackdoorCheckCommand());

            _instance.Root.AddDirectory(new Directory("Tectonic"))
                .AddCommand(new HelpCommand(), true)
                .AddCommand(new TectonicSolveCommand())
                .AddCommand(new TectonicSolveBatchCommand())
                .AddCommand(new TectonicSolutionCountCommand());
        }

        return _instance;
    }
}

