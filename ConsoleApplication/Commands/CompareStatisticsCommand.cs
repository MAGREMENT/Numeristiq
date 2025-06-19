namespace ConsoleApplication.Commands;

public class CompareStatisticsCommand : Command
{
    public CompareStatisticsCommand() : base("CompareStats", 
        new Argument("First file of stats", ValueType.ReadFile),
        new Argument("Second file of stats", ValueType.ReadFile))
    {
    }

    public override string Description => "Compares the stats of two files";
    
    public override void Execute(ArgumentInterpreter interpreter, IReadOnlyCallReport report)
    {
        //TODO
    }
}