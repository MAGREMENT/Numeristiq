using Model.Utility;

namespace Model.Sudoku.Solver.Arguments;

public class MinMaxStrategyArgument : IStrategyArgument
{
    public string Name { get; }
    public IArgumentViewInterface Interface { get; }
    
    private readonly GetArgument<int> _minGetter;
    private readonly SetArgument<int> _minSetter;
    private readonly GetArgument<int> _maxGetter;
    private readonly SetArgument<int> _maxSetter;

    public MinMaxStrategyArgument(string name, int minMin, int minMax, int maxMin, int maxMax, int tickFrequency, 
        GetArgument<int> minGetter, SetArgument<int> minSetter, GetArgument<int> maxGetter, SetArgument<int> maxSetter)
    {
        Name = name;
        _minSetter = minSetter;
        _maxGetter = maxGetter;
        _maxSetter = maxSetter;
        _minGetter = minGetter;
        Interface = new MinMaxSliderViewInterface(minMin, minMax, maxMin, maxMax, tickFrequency);
    }
    
    public ArgumentValue Get()
    {
        return new MinMaxArgumentValue(new MinMax(_minGetter(), _maxGetter()));
    }

    public void Set(ArgumentValue s)
    {
        var mm = s.ToMinMax();
        _minSetter(mm.Min);
        _maxSetter(mm.Max);
    }
}