namespace ConsoleApplication.Commands;

public class BufferRemoveCommand : Command
{
    public override IReadOnlyList<string> Synonyms { get; } = ["rmv"];
    
    public BufferRemoveCommand() : 
        base("Remove", new Argument("The name of the buffer", ValueType.String)) { }

    public override string Description => "Removes a buffer with a specific name";
    
    public override void Execute(ArgumentInterpreter interpreter, IReadOnlyCallReport report)
    {
        if (!interpreter.RemoveBuffer((string)report.GetArgumentValue(0)))
        {
            Console.WriteLine("A buffer with that name does not exists");
        }
        else Console.WriteLine("Buffer removed");
    }
}