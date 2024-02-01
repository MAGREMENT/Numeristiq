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
    
    public string Get()
    {
        return $"{_minGetter()},{_maxGetter()}";
    }

    public void Set(string s)
    {
        var i = s.IndexOf(',');
        if (i == -1) return;

        try
        {
            var min = int.Parse(s[..i]);
            var max = int.Parse(s[(i + 1)..]);
            _minSetter(min);
            _maxSetter(max);
        }
        catch
        {
            // ignored
        }
    }
}