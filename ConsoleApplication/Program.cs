namespace ConsoleApplication;

public static class Program
{
    public static void Main(string[] args)
    {
        Interpreter().Execute(args);
    }

    private static ArgumentInterpreter Interpreter()
    {
        var result = new ArgumentInterpreter();

        return result;
    }
}

