using Model;
using Repository;

namespace RunTester;

public static class Program
{
    public static void Main(string[] args)
    {
        var repo = new JSONStrategyRepository();
        try
        {
            repo.Initialize();
        }
        catch (StrategyRepositoryInitializationException e)
        {
            Console.WriteLine("Exception while initializing repository : " + e.Message);
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

        result.AddAllowedArgument("f", ArgumentValueType.Mandatory);
        result.AddAllowedArgument("r", ArgumentValueType.None);
        result.AddAllowedArgument("w", ArgumentValueType.None);
        
        return result;
    }
}