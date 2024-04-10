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
            _instance.AddCommand(new HelpCommand(), true);
            _instance.AddCommand(new SessionCommand());
            _instance.AddCommand(new SudokuSolveCommand());
            _instance.AddCommand(new SudokuSolveBatchCommand());
            _instance.AddCommand(new SudokuGenerateBatchCommand());
            _instance.AddCommand(new TectonicSolveCommand());
            _instance.AddCommand(new TectonicSolveBatchCommand());
            _instance.AddCommand(new SudokuStrategyListCommand());
        }

        return _instance;
    }
}

