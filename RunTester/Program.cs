using Model;
using Model.Sudoku;
using Model.Utility;
using Repository;

namespace RunTester;

public static class Program
{
    public static void Main(string[] args)
    {
        var repo = new SudokuStrategiesJSONRepository("strategies.json");
        if (!repo.Initialize(false))
        {
            Console.WriteLine("Exception while initializing repository : ");
            return;
        }
        
        var argReader = GetArgumentsReader();
        var argResult = argReader.Read(args);

        var runTester = new RunTester(repo, argResult.Contains("w"))
        {
            Path = argResult.GetValue("f")
        };
        
        if(argResult.Contains("r")) runTester.SolveDone += (number, line, success, fail) =>
        {
            Console.Write($"#{number} ");
            if(success) Console.WriteLine("Ok !");
            else
            {
                if (fail) Console.Write("Solver failed");
                else Console.Write("Solver did not find solution");
                
                Console.WriteLine($" => '{line}'");
            }
        };
        
        runTester.Start();

        Console.WriteLine(runTester.LastRunResult);
    }

    private static ArgumentsReader GetArgumentsReader()
    {
        var result = new ArgumentsReader();

        result.AddAllowedArgument("f", ArgumentValueType.Mandatory); //File name
        result.AddAllowedArgument("r", ArgumentValueType.None); //Feedback for each Sudoku
        result.AddAllowedArgument("w", ArgumentValueType.None); //All strategies to wait for all
        
        return result;
    }
}