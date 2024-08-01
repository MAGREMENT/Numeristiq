using ConsoleApplication.Commands;

namespace ConsoleApplication;

public static class Program
{
    public const bool IsForProduction = false;
    
    public static void Main(string[] args)
    {
        Interpreter().Execute(args);
    }
    
    private static ArgumentInterpreter Interpreter()
    {
        var instance = new ArgumentInterpreter();
        
        instance.Root.AddCommand(new HelpCommand(), true)
            .AddCommand(new SessionCommand());

        var sDir = instance.Root.AddDirectory(new Directory("Sudoku"))
            .AddCommand(new HelpCommand(), true)
            .AddCommand(new SudokuSolveCommand())
            .AddCommand(new SudokuSolveBatchCommand())
            .AddCommand(new SudokuGenerateBatchCommand())
            .AddCommand(new SudokuStrategyListCommand())
            .AddCommand(new SudokuSolutionCountCommand())
            .AddCommand(new SudokuBackdoorCheckCommand());

        instance.Root.AddDirectory(new Directory("Tectonic"))
            .AddCommand(new HelpCommand(), true)
            .AddCommand(new TectonicSolveCommand())
            .AddCommand(new TectonicSolveBatchCommand())
            .AddCommand(new TectonicSolutionCountCommand())
            .AddCommand(new TectonicGenerateBatchCommand());

        instance.Root.AddDirectory(new Directory("Kakuro"))
            .AddCommand(new HelpCommand(), true)
            .AddCommand(new KakuroSolveCommand());
        
        instance.Root.AddDirectory(new Directory("Nonogram"))
            .AddCommand(new HelpCommand(), true)
            .AddCommand(new NonogramSolveCommand())
            .AddCommand(new NonogramSolveBatchCommand())
            .AddCommand(new NonogramGenerateBatchCommand())
            .AddCommand(new NonogramSolutionCount());

        if (!IsForProduction)
        {
            sDir.AddDirectory(new Directory("Bank"))
                .AddCommand(new SudokuBankInitializeCommand())
                .AddCommand(new SudokuAddBatchInitializeCommand())
                .AddCommand(new SudokuBankClearCommand());
            
            instance.Root.AddDirectory(new Directory("Developer"))
                .AddCommand(new ThemeCommand());
        }

        return instance;
    }
}

