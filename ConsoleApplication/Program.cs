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
            .AddCommand(new SessionCommand())
            .AddCommand(new ExecuteScriptCommand());

        instance.Root.AddDirectory(new Directory("Buffers", "buff"))
            .AddCommand(new HelpCommand(), true)
            .AddCommand(new BufferCreateCommand())
            .AddCommand(new BufferRemoveCommand())
            .AddCommand(new BufferEnumerateCommand())
            .AddCommand(new BufferWriteCommand())
            .AddCommand(new BufferDisplayCommand());

        var sDir = instance.Root.AddDirectory(new Directory("Sudoku", "sudoku", "sud"))
            .AddCommand(new HelpCommand(), true)
            .AddCommand(new SudokuSolveCommand())
            .AddCommand(new SudokuSolveBatchCommand())
            .AddCommand(new SudokuGenerateBatchCommand())
            .AddCommand(new SudokuStrategyListCommand())
            .AddCommand(new SudokuSolutionCountCommand())
            .AddCommand(new SudokuBackdoorCheckCommand());

        instance.Root.AddDirectory(new Directory("Tectonic", "tectonic", "tec"))
            .AddCommand(new HelpCommand(), true)
            .AddCommand(new TectonicSolveCommand())
            .AddCommand(new TectonicSolveBatchCommand())
            .AddCommand(new TectonicSolutionCountCommand())
            .AddCommand(new TectonicGenerateBatchCommand());

        instance.Root.AddDirectory(new Directory("Kakuro", "kakuro", "kak"))
            .AddCommand(new HelpCommand(), true)
            .AddCommand(new KakuroSolveCommand());
        
        instance.Root.AddDirectory(new Directory("Nonogram", "nonogram", "nono"))
            .AddCommand(new HelpCommand(), true)
            .AddCommand(new NonogramSolveCommand())
            .AddCommand(new NonogramSolveBatchCommand())
            .AddCommand(new NonogramGenerateBatchCommand())
            .AddCommand(new NonogramSolutionCount());

        instance.Root.AddDirectory(new Directory("Binairo", "binairo", "bin"))
            .AddCommand(new HelpCommand(), true)
            .AddCommand(new BinairoSolveBatchCommand())
            .AddCommand(new BinairoGenerateBatchCommand());

        instance.Root.AddDirectory(new Directory("CrossSum", "crosssum", "cs"))
            .AddCommand(new HelpCommand(), true)
            .AddCommand(new CrossSumSolveCommand());

        if (!IsForProduction)
        {
            sDir.AddDirectory(new Directory("Bank", "bank", "bk"))
                .AddCommand(new SudokuBankInitializeCommand())
                .AddCommand(new SudokuAddBatchInitializeCommand())
                .AddCommand(new SudokuBankClearCommand());
            
            instance.Root.AddDirectory(new Directory("Developer", "developer", "dev"))
                .AddCommand(new HelpCommand(), true)
                .AddCommand(new CompareStatisticsCommand())
                .AddCommand(new ThemeCommand());
        }

        return instance;
    }
}

