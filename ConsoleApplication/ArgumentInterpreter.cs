namespace ConsoleApplication;

public class ArgumentInterpreter : IReadOnlyArgumentInterpreter
{
    private readonly List<Command> _commands = new();
    private int _defaultCommand = -1;
    
    public IReadOnlyList<Command> Commands => _commands;

    public void AddCommand(Command command, bool isDefault = false)
    {
        if (isDefault) _defaultCommand = _commands.Count;
        _commands.Add(command);
    }

    public void Execute(string[] args)
    {
        Command? c = null;
        
        if (args.Length == 0)
        {
            if (_defaultCommand == -1) Console.WriteLine("No default command set");
            else
            {
                c = _commands[_defaultCommand];
                c.Execute(this, new OptionsReport(c));
            }
            
            return;
        }

        foreach (var candidate in _commands)
        {
            if (candidate.Name == args[0])
            {
                c = candidate;
                break;
            }
        }

        if (c == null)
        {
            Console.WriteLine($"Command does not exist : {args[0]}");
            return;
        }

        var report = new OptionsReport(c);

        for (int i = 1; i < args.Length; i++)
        {
            var index = c.IndexOf(args[i]);
            if (index == -1)
            {
                Console.WriteLine($"Command '{c.Name}' is not compatible with this option : {args[i]}");
            }

            var o = c.Options[index];
            object? value = null;
            
            switch (o.ValueRequirement)
            {
                case OptionValueRequirement.None : break;
                case OptionValueRequirement.Mandatory :
                    if (i == args.Length - 1)
                    {
                        Console.WriteLine($"Missing mandatory value for option '{o.Name}'");
                        return;
                    }

                    if (!CheckValueType(args[i + 1], o.ValueType, out value))
                    {
                        Console.WriteLine($"Value of the wrong type for : {args[i] + 1}\nExpected type '{o.ValueType}'");
                        return;
                    }

                    i++;
                    break;
                case OptionValueRequirement.Optional :
                    if (i == args.Length - 1 || c.IndexOf(args[i + 1]) == -1) break;
                    
                    if (!CheckValueType(args[i + 1], o.ValueType, out value))
                    {
                        Console.WriteLine($"Value of the wrong type for : {args[i] + 1}\nExpected type '{o.ValueType}'");
                        return;
                    }

                    i++;
                    break;
            }

            if (value is null) report.AddUsedOption(i);
            else report.AddUsedOption(i, value);
        }

        c.Execute(this, report);
    }

    private bool CheckValueType(string value, OptionValueType type, out object? objectValue)
    {
        objectValue = null;

        switch (type)
        {
            case OptionValueType.None : return false;
            case OptionValueType.String :
                objectValue = value.Length > 2 && value[0] == '"' && value[^1] == '"'
                    ? value.Substring(1, value.Length - 2)
                    : value;
                return true;
            case OptionValueType.Int :
                try
                {
                    objectValue = int.Parse(value);
                    return true;
                }
                catch (Exception)
                {
                    return false;
                }
            case OptionValueType.File :
                return File.Exists(value);
        }

        return false;
    }
}

public interface IReadOnlyArgumentInterpreter
{
    public IReadOnlyList<Command> Commands { get; }
}