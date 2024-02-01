namespace Model.Sudoku.Solver.Arguments;

public class BooleanStrategyArgument : IStrategyArgument
{
    public string Name { get; }
    public IArgumentViewInterface Interface { get; }
    
    private readonly GetArgument<bool> _getter;
    private readonly SetArgument<bool> _setter;

    public BooleanStrategyArgument(string name, GetArgument<bool> getter, SetArgument<bool> setter)
    {
        Name = name;
        Interface = new BooleanViewInterface();
        _getter = getter;
        _setter = setter;
    }
    
    public string Get()
    {
        return _getter().ToString();
    }
    
    public void Set(string s)
    {
        bool val;
        switch (s.ToLower())
        {
            case "true" : val = true;
                break;
            case "false" : val = false;
                break;
            default: return;
        }

        _setter(val);
    }
}