namespace ConsoleApplication;

public class OptionsReport : IReadOnlyOptionsReport
{
    private readonly Report[] _reports;

    public OptionsReport(Command command)
    {
        _reports = new Report[command.Options.Count];
        for (int i = 0; i < _reports.Length; i++)
        {
            _reports[i] = new Report();
        }
    }

    public void AddUsedOption(int index)
    {
        _reports[index].IsUsed = true;
    }

    public void AddUsedOption(int index, object value)
    {
        var r = _reports[index];
        r.IsUsed = true;
        r.Value = value;
    }

    public bool IsUsed(int index)
    {
        return _reports[index].IsUsed;
    }

    public object? GetArgument(int index)
    {
        return _reports[index].Value;
    }
}

public interface IReadOnlyOptionsReport
{
    public bool IsUsed(int index);
    public object? GetArgument(int index);
}

public struct Report
{
    public bool IsUsed { get; set; }
    public object? Value { get; set; } 
}