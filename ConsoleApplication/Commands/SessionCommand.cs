using System.Text;

namespace ConsoleApplication.Commands;

public class SessionCommand : Command
{
    private static readonly string[] StopCommands = { "Stop", "s" };
    
    public override string Description =>
        "Starts a session allowing you to enter multiple commands without exiting the application";

    private bool _isRunning;
    
    public SessionCommand() : base("Session")
    {
    }
    
    public override void Execute(ArgumentInterpreter interpreter, IReadOnlyCallReport report)
    {
        if (_isRunning)
        {
            Console.WriteLine("Session already started");
            return;
        }
        
        Console.WriteLine($"Session start. (Enter {StopCommandsToString()} to exit)");
        _isRunning = true;

        while (true)
        {
            Console.Write("\nCommand : ");
            string? response;
            try
            {
                response = Console.ReadLine();
            }
            catch (Exception)
            {
                Console.WriteLine("\nAn Exception occured while reading the next command");
                break;
            }

            if (response is null) continue;

            if (StopCommands.Contains(response)) break;
            
            interpreter.Execute(GetArgs(response));
        }
        
        Console.WriteLine("\nSession closed");
        _isRunning = false;
    }

    private static List<string> GetArgs(string s)
    {
        List<string> result = new();
        var builder = new StringBuilder();

        bool isQuoted = false;
        foreach (var c in s)
        {
            if (c == '"')
            {
                isQuoted = !isQuoted;
                continue;
            }
            
            if (isQuoted || c != ' ')
            {
                builder.Append(c);
                continue;
            }
            
            if (builder.Length == 0) continue;
            
            result.Add(builder.ToString());
            builder.Clear();
        }
        
        if(builder.Length > 0) result.Add(builder.ToString());
        
        return result;
    }

    private static string StopCommandsToString()
    {
        if (StopCommands.Length == 0) return "";
        
        StringBuilder builder = new();

        builder.Append(StopCommands[0]);
        for (int i = 1; i < StopCommands.Length - 1; i++)
        {
            builder.Append(", " + StopCommands[i]);
        }

        builder.Append(" or " + StopCommands[^1]);

        return builder.ToString();
    }
}