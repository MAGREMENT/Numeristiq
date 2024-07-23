namespace ConsoleApplication;

public class ArgumentInterpreter
{
    public Directory Root { get; } = new("Root");
    public Instantiator Instantiator { get; } = new();
    
    public void Execute(IReadOnlyList<string> args)
    {
        var d = Root;
        Command? c;
        
        if (args.Count == 0)
        {
            c = Root.DefaultCommand();
            
            if (c is null) Console.WriteLine("No default command set");
            else c.Execute(this, new CallReport(Root, c));

            return;
        }

        int cursor = 0;
        for (; cursor < args.Count; cursor++)
        {
            var buffer = d.FindDirectory(args[cursor]);
            if (buffer is null) break;

            d = buffer;
        }

        c = cursor < args.Count ? d.FindCommand(args[cursor++]) : d.DefaultCommand();

        if (c == null)
        {
            Console.WriteLine("No corresponding command found");
            return;
        }

        if (args.Count - cursor < c.Arguments.Count)
        {
            Console.WriteLine($"Not enough arguments for command : {c.Name}");
            return;
        }

        var report = new CallReport(d, c);

        var argumentCursor = 0;
        for (; cursor < args.Count; cursor++)
        {
            object? value;
            
            if (argumentCursor < c.Arguments.Count)
            {
                if (!CheckValueType(args[cursor], c.Arguments[argumentCursor].ValueType, out value))
                {
                    Console.WriteLine($"Value of the wrong type for argument {argumentCursor + 1}" +
                                      $"\nExpected type '{c.Arguments[argumentCursor]}'");
                    return;
                }

                report.SetArgumentValue(argumentCursor++, value);
                continue;
            }
            
            var index = c.IndexOfOption(args[cursor]);
            if (index == -1)
            {
                Console.WriteLine($"Command '{c.Name}' is not compatible with this option : {args[cursor]}");
                return;
            }

            var o = c.Options[index];
            value = null;
            
            switch (o.ValueRequirement)
            {
                case ValueRequirement.None : break;
                case ValueRequirement.Mandatory :
                    if (cursor == args.Count - 1)
                    {
                        Console.WriteLine($"Missing mandatory value for option '{o.Name}'");
                        return;
                    }

                    if (!CheckValueType(args[cursor + 1], o.ValueType, out value))
                    {
                        Console.WriteLine($"Value of the wrong type for : {args[cursor]}\nExpected type '{o.ValueType}'");
                        return;
                    }

                    cursor++;
                    break;
                case ValueRequirement.Optional :
                    if (cursor == args.Count - 1 || c.IndexOfOption(args[cursor + 1]) == -1) break;
                    
                    if (!CheckValueType(args[cursor + 1], o.ValueType, out value))
                    {
                        Console.WriteLine($"Value of the wrong type for : {args[cursor] + 1}\nExpected type '{o.ValueType}'");
                        return;
                    }

                    cursor++;
                    break;
            }

            if (value is null) report.AddUsedOption(index);
            else report.AddUsedOption(index, value);
        }

        c.Execute(this, report);
    }

    private bool CheckValueType(string value, ValueType type, out object objectValue)
    {
        objectValue = default!;

        switch (type)
        {
            case ValueType.None : return false;
            case ValueType.String :
                objectValue = value.Length > 2 && value[0] == '"' && value[^1] == '"'
                    ? value.Substring(1, value.Length - 2)
                    : value;
                return true;
            case ValueType.Int :
                try
                {
                    objectValue = int.Parse(value);
                    return true;
                }
                catch (Exception)
                {
                    return false;
                }
            case ValueType.File :
                objectValue = value;
                return File.Exists(value);
        }

        return false;
    }
}