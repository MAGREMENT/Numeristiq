using System.Text;

namespace ConsoleApplication.Commands;

public class ExecuteScriptCommand : Command
{
    public override IReadOnlyList<string> Synonyms { get; } = ["exec"];

    public ExecuteScriptCommand() : base("ExecuteScript", 
        new Argument("Script to execute", ValueType.ReadableStream)) { }

    public override string Description => "Executes a script";
    
    public override CommandResult? Execute(ArgumentInterpreter interpreter, IReadOnlyCallReport report)
    {
        var reader = new StreamReader((Stream)report.GetArgumentValue(0));
        var script = reader.ReadToEnd();

        List<string> args = new();
        var builder = new StringBuilder();
        var isOpen = false;

        CommandResult? result;
        foreach (var c in script)
        {
            switch (c)
            {
                case ' ' :
                    if (isOpen)
                    {
                        builder.Append(c);
                        continue;
                    }
                    
                    if(builder.Length == 0) continue;
                    
                    args.Add(builder.ToString());
                    builder.Clear();
                    
                    break;
                case '\n' :
                    if (builder.Length > 0)
                    {
                        args.Add(builder.ToString());
                        builder.Clear();
                    }

                    isOpen = false;
                    if(args.Count == 0) continue;

                    result = interpreter.Execute(args);
                    result.WriteLine();
                    if (result != CommandResult.Succes) return CommandResult.Fail("One command in the script failed. The script was interrupted");
                    
                    args.Clear();
                    
                    break;
                case '"' :
                    if (!isOpen)
                    {
                        isOpen = true;
                        continue;
                    }
                    
                    args.Add(builder.ToString());
                    builder.Clear();
                    
                    break;
                case '\t' :
                case '\r' :
                        break;
                default :
                    builder.Append(c);
                    break;
            }
        }

        if (builder.Length > 0)
        {
            args.Add(builder.ToString());
            builder.Clear();
        }
        
        result = interpreter.Execute(args);
        result.WriteLine();
        return result != CommandResult.Succes 
            ? CommandResult.Fail("One command in the script failed. The script was interrupted") 
            : CommandResult.Succes;
    }
}