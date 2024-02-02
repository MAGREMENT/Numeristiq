namespace Model.Sudoku.Solver.Arguments;

public interface IStrategyArgument
{
    public string Name { get; }
    public IArgumentViewInterface Interface { get; }
    public ArgumentValue Get();
    public void Set(ArgumentValue s);
}

public delegate T GetArgument<out T>();
public delegate void SetArgument<in T>(T value);