using System;
using System.Collections.Generic;

namespace Model.Utility;

public class ArgumentsReader
{
    private readonly Dictionary<string, ArgumentValueType> _allowedArguments = new();

    public void AddAllowedArgument(string name, ArgumentValueType type)
    {
        _allowedArguments.Add(name, type);
    }

    public IReadOnlyArguments Read(string[] args)
    {
        Arguments result = new();

        bool hasToBeValue = false;
        bool canBeValue = false;
        string argBuffer = "";

        foreach (var arg in args)
        {
            var dashCount = StartCount(arg, '-');
            if (dashCount > 2) throw new ArgumentException("Too many '-' at the start : " + arg);

            bool isArg = dashCount > 0;
            var s = arg.Substring(dashCount);
            if (!canBeValue && !isArg) throw new ArgumentException("Can't be a value : " + s);

            if (hasToBeValue && isArg) throw new ArgumentException("Has to be a value : " +s);

            var type = ArgumentValueType.None;
            if (isArg && !_allowedArguments.TryGetValue(s, out type))
                throw new ArgumentException("Not an allowed arg : " + s);

            switch (type)
            {
                case ArgumentValueType.None :
                    hasToBeValue = false;
                    canBeValue = false;
                    break;
                case ArgumentValueType.Allowed :
                    hasToBeValue = false;
                    canBeValue = true;
                    break;
                case ArgumentValueType.Mandatory :
                    hasToBeValue = true;
                    canBeValue = true;
                    break;
            }

            if (!argBuffer.Equals(""))
            {
                if (isArg) result.Add(argBuffer);
                else result.Add(argBuffer, s);
            }

            argBuffer = isArg ? s : "";
        }

        if (!argBuffer.Equals(""))
        {
            if (hasToBeValue) throw new ArgumentException("Value missing");

            result.Add(argBuffer);
        }

        return result;
    }

    private int StartCount(string s, char c)
    {
        int count = 0;
        while (s[count] == c)
        {
            count++;
        }

        return count;
    }
}

public class Arguments : IReadOnlyArguments
{
    private readonly List<string> _values = new();
    private readonly Dictionary<string, int> _arguments = new();

    public void Add(string arg, string value)
    {
        _values.Add(value);
        _arguments.Add(arg, _values.Count - 1);
    }

    public void Add(string arg)
    {
        _arguments.Add(arg, -1);
    }

    public bool Contains(string arg)
    {
        return _arguments.ContainsKey(arg);
    }

    public bool HasValue(string arg)
    {
        if (!_arguments.TryGetValue(arg, out var value)) throw new ArgumentException("No such arg");
        return value != -1;
    }

    public string GetValue(string arg)
    {
        if (!_arguments.TryGetValue(arg, out var value)) throw new ArgumentException("No such arg");
        return _values[value];
    }
}

public interface IReadOnlyArguments
{
    public bool Contains(string arg);

    public bool HasValue(string arg);

    public string GetValue(string arg);
}

public enum ArgumentValueType
{
    None, Allowed, Mandatory
}

