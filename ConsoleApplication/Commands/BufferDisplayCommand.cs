namespace ConsoleApplication.Commands;

public class BufferDisplayCommand : Command
{
    public override IReadOnlyList<string> Synonyms { get; } = ["dsp"];

    public BufferDisplayCommand() : 
        base("Display", new Argument("$ + The name of the buffer", ValueType.ReadableStream)) { }

    public override string Description => "Displays the content of a buffer";
    
    public override CommandResult? Execute(ArgumentInterpreter interpreter, IReadOnlyCallReport report)
    {
        var r = new StreamReader((Stream)report.GetArgumentValue(0));
        Console.WriteLine(r.ReadToEnd());
        
        return CommandResult.Succes;
    }
}