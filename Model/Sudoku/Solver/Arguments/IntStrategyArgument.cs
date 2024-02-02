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

    public ArgumentValue Get()
    {
        return new IntArgumentValue(_getter());
    }

    public void Set(ArgumentValue value)
    {
        _setter(value.ToInt());
    }
}