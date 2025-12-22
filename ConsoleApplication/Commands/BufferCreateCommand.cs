namespace ConsoleApplication.Commands;

public class BufferCreateCommand : Command
{
    public override IReadOnlyList<string> Synonyms { get; } = ["crt"];

    public BufferCreateCommand() : 
        base("Create", new Argument("The name of the buffer", ValueType.String)) { }

    public override string Description => "Creates a buffer with a specific name";
    
    public override CommandResult? Execute(ArgumentInterpreter interpreter, IReadOnlyCallReport report)
    {
        if (!interpreter.AddBuffer((string)report.GetArgumentValue(0)))
        {
            return CommandResult.Fail("A buffer with that name already exists");
        }
        
        Console.WriteLine("Buffer created");
        return CommandResult.Succes;
    }
}