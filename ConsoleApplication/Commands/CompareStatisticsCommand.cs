using System.Text.Json;
using Model.Core.Trackers;
using Model.Utility;

namespace ConsoleApplication.Commands;

public class CompareStatisticsCommand : Command
{
    private const int FirstFileIndex = 0;
    private const int SecondFileIndex = 1;

    private const double Delta = 1.5;

    public override IReadOnlyList<string> Synonyms { get; } = ["cpst"];

    public CompareStatisticsCommand() : base("CompareStats", 
        new Argument("First file of stats", ValueType.ReadableStream),
        new Argument("Second file of stats", ValueType.ReadableStream))
    {
    }

    public override string Description => "Compares the stats of two files";
    
    public override CommandResult? Execute(ArgumentInterpreter interpreter, IReadOnlyCallReport report)
    {
        var r = new StreamReader((Stream)report.GetArgumentValue(FirstFileIndex));
        var s = r.ReadToEnd();
        var stats1 = JsonSerializer.Deserialize<StrategyStatistics[]>(s);
        if (stats1 == null)
        {
            return CommandResult.Fail("Could not parse first file");
        }

        r = new StreamReader((Stream)report.GetArgumentValue(SecondFileIndex));
        s = r.ReadToEnd();
        var stats2 = JsonSerializer.Deserialize<StrategyStatistics[]>(s);
        if (stats2 == null)
        {
            return CommandResult.Fail("Could not parse second file");
        }

        if (stats1.Length != stats2.Length)
        {
            Console.WriteLine("Different strategies used");
            return CommandResult.Succes;
        }

        for (int i = 0; i < stats1.Length; i++)
        {
            if (stats1[i].Name != stats2[i].Name)
            {
                Console.WriteLine("Different strategies used");
                return CommandResult.Succes;
            }
        }

        List<(StrategyStatistics, StrategyStatistics)> differences = new();
        List<(StrategyStatistics, StrategyStatistics)> improvements = new();
        
        for (int i = 0; i < stats1.Length; i++)
        {
            if (stats1[i].PossibilitiesRemoved != stats2[i].PossibilitiesRemoved
                || stats1[i].SolutionsAdded != stats2[i].SolutionsAdded)
            {
                differences.Add((stats1[i], stats2[i]));
            }
            else if(stats1[i].TotalTime != 0 && (stats1[i].AverageTime() >= stats2[i].AverageTime() * Delta 
                    || stats1[i].AverageTime() * Delta <= stats2[i].AverageTime()))
            {
                improvements.Add((stats1[i], stats2[i]));
            }
        }
        
        if (differences.Count > 0)
        {
            Console.WriteLine("\n/!\\ Some strategies did not produce the same result.\n");
            Console.WriteLine(differences.ToGridReport(5, s =>
            [
                s.Item1.Name,
                s.Item1.PossibilitiesRemoved.ToString(),
                s.Item1.SolutionsAdded.ToString(),
                s.Item2.PossibilitiesRemoved.ToString(),
                s.Item2.SolutionsAdded.ToString()
            ], ColumnTitle.Follow(("Strategy Name", 1), ("Stats 1", 2), ("Stats 2", 2)),
            ColumnTitle.Follow(1, ("Possibilities Removed", 1), ("Solutions Added", 1), ("Possibilities Removed", 1), ("Solutions Added", 1))));
        }
        else if(improvements.Count > 0)
        {
            Console.WriteLine("\nAll strategies did produce the same result.\n");
            Console.WriteLine(improvements.ToGridReport(3, s =>
                [
                    s.Item1.Name,
                    (s.Item1.AverageTime() < s.Item2.AverageTime() ? "v " : "") + s.Item1.AverageTime() + "ms",
                    (s.Item1.AverageTime() > s.Item2.AverageTime() ? "v " : "") + s.Item2.AverageTime() + "ms",
                ], ColumnTitle.Follow(("Strategy Name", 1), ("Stats 1", 1), ("Stats 2", 1)),
                ColumnTitle.Follow(1, ("Average Time", 1), ("Average Time", 1))));
        }
        else
        {
            Console.WriteLine("\nNo notable difference between stats");
        }
        
        return CommandResult.Succes;
    }
}