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
    
    public ArgumentValue Get()
    {
        return new BoolArgumentValue(_getter());
    }
    
    public void Set(ArgumentValue s)
    {
        _setter(s.ToBool());
    }
}