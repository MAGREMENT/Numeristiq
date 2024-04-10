namespace ConsoleApplication;

public abstract class Command //TODO add aliases
{
    public string Name { get; }
    public IReadOnlyList<Option> Options { get; }
    
    public abstract string Description { get; }
    
    protected Command(string name)
    {
        Name = name;
        Options = Array.Empty<Option>();
    }

    protected Command(string name, params Option[] options)
    {
        Name = name;
        Options = options;
    }

    public int IndexOf(string optionName)
    {
        for (int i = 0; i < Options.Count; i++)
        {
            if (Options[i].Name.Equals(optionName)) return i;
        }

        return -1;
    }

    public abstract void Execute(IReadOnlyArgumentInterpreter interpreter, IReadOnlyOptionsReport report);
}