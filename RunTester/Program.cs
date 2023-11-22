using Global.Enums;

namespace RunTester;

public static class Program
{
    public static void Main(string[] args)
    {
        var argReader = GetArgumentsReader();
        var argResult = argReader.Read(args);

        var runTester = new RunTester(OnInstanceFound.WaitForAll)
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
        
        return result;
    }
}