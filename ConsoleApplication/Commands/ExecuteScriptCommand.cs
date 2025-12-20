using System.Text;

namespace ConsoleApplication.Commands;

public class ExecuteScriptCommand : Command //TODO make command fails to prevent execution after problem
{
    public override IReadOnlyList<string> Synonyms { get; } = ["exec"];

    public ExecuteScriptCommand() : base("ExecuteScript", 
        new Argument("Script to execute", ValueType.ReadableStream)) { }

    public override string Description => "Executes a script";
    
    public override void Execute(ArgumentInterpreter interpreter, IReadOnlyCallReport report)
    {
        var reader = new StreamReader((Stream)report.GetArgumentValue(0));
        var script = reader.ReadToEnd();

        List<string> args = new();
        var builder = new StringBuilder();
        var isOpen = false;

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

                    interpreter.Execute(args);
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
        
        interpreter.Execute(args);
    }
}