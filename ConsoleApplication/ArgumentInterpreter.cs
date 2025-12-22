namespace ConsoleApplication;

public class ArgumentInterpreter
{
    private readonly Dictionary<string, Stream> _buffers = new();
    public Directory Root { get; } = new("Root");
    public Instantiator Instantiator { get; } = new();

    public bool AddBuffer(string name)
    {
        return _buffers.TryAdd(name, new MemoryStream());
    }

    public bool RemoveBuffer(string name)
    {
        if (!_buffers.TryGetValue(name, out var b)) return false;
        
        b.Dispose();
        _buffers.Remove(name);

        return true;
    }

    public IEnumerable<string> EnumerateBuffers()
    {
        return _buffers.Keys;
    }
    
    public CommandResult? Execute(IReadOnlyList<string> args)
    {
        var d = Root;
        Command? c;
        
        if (args.Count == 0)
        {
            c = Root.DefaultCommand();
            if (c is null) return CommandResult.Fail("No default command set");
            
            c.Execute(this, new CallReport(Root, c));
            return CommandResult.Succes;
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
            return CommandResult.Fail("No corresponding command found");
        }

        if (args.Count - cursor < c.Arguments.Count)
        {
            return CommandResult.Fail($"Not enough arguments for command : {c.Name}");
        }

        using var report = new CallReport(d, c);

        var argumentCursor = 0;
        for (; cursor < args.Count; cursor++)
        {
            object? value;
            
            if (argumentCursor < c.Arguments.Count)
            {
                if (!CheckValueType(args[cursor], c.Arguments[argumentCursor].ValueType, out value))
                {
                    return CommandResult.Fail($"Value of the wrong type for argument {argumentCursor + 1}" +
                                              $"\nExpected type '{c.Arguments[argumentCursor]}'");
                }

                report.SetArgumentValue(argumentCursor++, value);
                continue;
            }
            
            var index = c.IndexOfOption(args[cursor]);
            if (index == -1)
            {
                return CommandResult.Fail($"Command '{c.Name}' is not compatible with this option : {args[cursor]}");
            }

            var o = c.Options[index];
            value = null;
            
            switch (o.ValueRequirement)
            {
                case ValueRequirement.None : break;
                case ValueRequirement.Mandatory :
                    if (cursor == args.Count - 1)
                    {
                        return CommandResult.Fail($"Missing mandatory value for option '{o.Name}'");
                    }

                    if (!CheckValueType(args[cursor + 1], o.ValueType, out value))
                    {
                        return CommandResult.Fail($"Value of the wrong type for : {args[cursor]}\nExpected type '{o.ValueType}'");
                    }

                    cursor++;
                    break;
                case ValueRequirement.Optional :
                    if (cursor == args.Count - 1 || c.IndexOfOption(args[cursor + 1]) == -1) break;
                    
                    if (!CheckValueType(args[cursor + 1], o.ValueType, out value))
                    {
                        return CommandResult.Fail($"Value of the wrong type for : {args[cursor] + 1}\nExpected type '{o.ValueType}'");
                    }

                    cursor++;
                    break;
            }

            if (value is null) report.AddUsedOption(index);
            else report.AddUsedOption(index, value);
        }

        return c.Execute(this, report);
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
                var r = int.TryParse(value, out var n);
                objectValue = n;
                return r;
            case ValueType.ReadableStream :
                if (value.Length == 0) return false;

                if (value[0] == '$')
                {
                    var result = _buffers.TryGetValue(value[1..], out var stream1);
                    stream1!.Position = 0;
                    objectValue = stream1;
                    return result;
                }
                
                try
                {
                    objectValue = new FileStream(value, new FileStreamOptions
                    {
                        Share = FileShare.Read,
                        Access = FileAccess.Read,
                        Mode = FileMode.Open
                    });
                    
                    return true;
                }
                catch
                {
                    return false;
                }
            case ValueType.WritableStream :
                if (value.Length == 0) return false;

                if (value[0] == '$')
                {
                    var result = _buffers.TryGetValue(value[1..], out var stream2);
                    objectValue = stream2!;
                    return result;
                }
                
                try
                {
                    objectValue = new FileStream(value, new FileStreamOptions
                    {
                        Share = FileShare.Write,
                        Access = FileAccess.Write,
                        Mode = FileMode.OpenOrCreate
                    });

                    return true;
                }
                catch
                {
                    return false;
                } 
        }

        return false;
    }
}