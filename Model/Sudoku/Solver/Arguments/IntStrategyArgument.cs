namespace Model.Sudoku.Solver.Arguments;

public class IntStrategyArgument : IStrategyArgument
{
    public string Name { get; }
    public IArgumentViewInterface Interface { get; }

    private readonly GetArgument<int> _getter;
    private readonly SetArgument<int> _setter;
    
    public IntStrategyArgument(string name, GetArgument<int> getter, SetArgument<int> setter, IArgumentViewInterface i)
    {
        Name = name;
        Interface = i;
        _getter = getter;
        _setter = setter;
    }

    public string Get()
    {
        return _getter().ToString();
    }

    public void Set(string value)
    {
        try
        {
            var asInt = int.Parse(value);
            _setter(asInt);
        }
        catch
        {
            // ignored
        }
    }
}