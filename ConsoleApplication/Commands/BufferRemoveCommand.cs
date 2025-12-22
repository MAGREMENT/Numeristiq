namespace ConsoleApplication.Commands;

public class BufferRemoveCommand : Command
{
    public override IReadOnlyList<string> Synonyms { get; } = ["rmv"];
    
    public BufferRemoveCommand() : 
        base("Remove", new Argument("The name of the buffer", ValueType.String)) { }

    public override string Description => "Removes a buffer with a specific name";
    
    public override CommandResult? Execute(ArgumentInterpreter interpreter, IReadOnlyCallReport report)
    {
        if (!interpreter.RemoveBuffer((string)report.GetArgumentValue(0)))
        {
            CommandResult.Fail("A buffer with that name does not exists");
        }
        
        Console.WriteLine("Buffer removed");
        return CommandResult.Succes;
    }
}