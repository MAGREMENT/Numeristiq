namespace ConsoleApplication.Commands;

public class BufferCreateCommand : Command
{
    public override IReadOnlyList<string> Synonyms { get; } = ["crt"];

    public BufferCreateCommand() : 
        base("Create", new Argument("The name of the buffer", ValueType.String)) { }

    public override string Description => "Creates a buffer with a specific name";
    
    public override void Execute(ArgumentInterpreter interpreter, IReadOnlyCallReport report)
    {
        if (!interpreter.AddBuffer((string)report.GetArgumentValue(0)))
        {
            Console.WriteLine("A buffer with that name already exists");
        }
        else Console.WriteLine("Buffer created");
    }
}