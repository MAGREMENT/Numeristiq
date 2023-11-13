using Model.Solver;

namespace RunTester;

public static class Program
{
    public static void Main(string[] args)
    {
        if (args.Length != 2 || !args[0].Equals("-f")) return;

        var runTester = new RunTester(OnInstanceFound.WaitForAll)
        {
            Path = args[1]
        };
        
        runTester.SolveDone += (number, line, success, fail) =>
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
}