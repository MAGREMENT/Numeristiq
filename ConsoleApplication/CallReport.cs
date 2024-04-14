namespace ConsoleApplication;

public class CallReport : IReadOnlyCallReport
{
    private readonly OptionValue[] _options;
    private readonly object[] _arguments;

    public Directory Directory { get; }

    public CallReport(Directory directory, Command command)
    {
        Directory = directory;
        _arguments = new object[command.Arguments.Count];
        _options = new OptionValue[command.Options.Count];
        for (int i = 0; i < _options.Length; i++)
        {
            _options[i] = new OptionValue();
        }
    }

    public void AddUsedOption(int index)
    {
        _options[index].IsUsed = true;
    }

    public void AddUsedOption(int index, object value)
    {
        _options[index].IsUsed = true;
        _options[index].Value = value;
    }

    public void SetArgumentValue(int index, object value)
    {
        _arguments[index] = value;
    }

    public bool IsOptionUsed(int index)
    {
        return _options[index].IsUsed;
    }

    public object? GetOptionValue(int index)
    {
        return _options[index].Value;
    }

    public object GetArgumentValue(int index) => _arguments[index];
}

public interface IReadOnlyCallReport
{
    public Directory Directory { get; }
    public bool IsOptionUsed(int index);
    public object? GetOptionValue(int index);
    public object GetArgumentValue(int index);
}

public struct OptionValue
{
    public bool IsUsed { get; set; }
    public object? Value { get; set; } 
}