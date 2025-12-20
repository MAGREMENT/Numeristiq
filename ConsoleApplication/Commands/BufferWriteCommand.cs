namespace ConsoleApplication.Commands;

public class BufferWriteCommand : Command
{
    public override IReadOnlyList<string> Synonyms { get; } = ["wrt"];

    public BufferWriteCommand() : 
        base("Write", new Argument("$ + The name of the buffer", ValueType.WritableStream),
            new Argument("What to write", ValueType.String)) { }

    public override string Description => "Writes to a buffer";
    
    public override void Execute(ArgumentInterpreter interpreter, IReadOnlyCallReport report)
    {
        var writer = new StreamWriter((Stream)report.GetArgumentValue(0));
        writer.Write((string)report.GetArgumentValue(1));
        writer.Flush();
    }
}