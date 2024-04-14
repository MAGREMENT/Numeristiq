namespace ConsoleApplication;

public abstract class Command //TODO add aliases
{
    public string Name { get; }
    public IReadOnlyList<Argument> Arguments { get; }
    public IReadOnlyList<Option> Options { get; }
    public abstract string Description { get; }
    
    protected Command(string name)
    {
        Name = name;
        Arguments = Array.Empty<Argument>();
        Options = Array.Empty<Option>();
    }

    protected Command(string name, IReadOnlyList<Argument> arguments, IReadOnlyList<Option> options)
    {
        Name = name;
        Arguments = arguments;
        Options = options;
    }
    
    protected Command(string name, params Argument[] arguments)
    {
        Name = name;
        Arguments = arguments;
        Options = Array.Empty<Option>();
    }

    protected Command(string name, params Option[] options)
    {
        Name = name;
        Arguments = Array.Empty<Argument>();
        Options = options;
    }

    public int IndexOfOption(string optionName)
    {
        for (int i = 0; i < Options.Count; i++)
        {
            if (Options[i].Name.Equals(optionName)) return i;
        }

        return -1;
    }

    public abstract void Execute(IReadOnlyArgumentInterpreter interpreter, IReadOnlyCallReport report);
}