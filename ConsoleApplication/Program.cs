using ConsoleApplication.Commands;

namespace ConsoleApplication;

public static class Program
{
    public static void Main(string[] args)
    {
        Interpreter().Execute(args);
    }

    private static ArgumentInterpreter? _instance;
    
    private static ArgumentInterpreter Interpreter()
    {
        if (_instance is null)
        {
            _instance = new ArgumentInterpreter();
            _instance.AddCommand(new HelpCommand(), true);
            _instance.AddCommand(new SessionCommand());
            _instance.AddCommand(new SGenerateBatchCommand());
            _instance.AddCommand(new SSolveBatchCommand());
        }

        return _instance;
    }
}

