namespace ConsoleApplication.Commands;

public class BufferEnumerateCommand : Command
{
    public override IReadOnlyList<string> Synonyms { get; } = ["enum"];
    
    public BufferEnumerateCommand() : 
        base("Enumerate") { }

    public override string Description => "Enumerate all buffers";
    
    public override CommandResult? Execute(ArgumentInterpreter interpreter, IReadOnlyCallReport report)
    {
        bool atleastOne = false;
        foreach (var buffer in interpreter.EnumerateBuffers())
        {
            Console.WriteLine(buffer);
            atleastOne = true;
        }
        
        if(!atleastOne) Console.WriteLine("No buffer found !");
        return CommandResult.Succes;
    }
}