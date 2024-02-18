using Model.Utility;

namespace Model.Sudoku.Solver.Settings.Types;

public class MinMaxSetting : ISetting
{
    public string Name { get; }
    public ISettingViewInterface Interface { get; }
    
    private readonly GetSetting<int> _minGetter;
    private readonly SetSetting<int> _minSetter;
    private readonly GetSetting<int> _maxGetter;
    private readonly SetSetting<int> _maxSetter;

    public MinMaxSetting(string name, int minMin, int minMax, int maxMin, int maxMax, int tickFrequency, 
        GetSetting<int> minGetter, SetSetting<int> minSetter, GetSetting<int> maxGetter, SetSetting<int> maxSetter)
    {
        Name = name;
        _minSetter = minSetter;
        _maxGetter = maxGetter;
        _maxSetter = maxSetter;
        _minGetter = minGetter;
        Interface = new MinMaxSliderViewInterface(minMin, minMax, maxMin, maxMax, tickFrequency);
    }
    
    public SettingValue Get()
    {
        return new MinMaxSettingValue(new MinMax(_minGetter(), _maxGetter()));
    }

    public void Set(SettingValue s)
    {
        var mm = s.ToMinMax();
        _minSetter(mm.Min);
        _maxSetter(mm.Max);
    }
}