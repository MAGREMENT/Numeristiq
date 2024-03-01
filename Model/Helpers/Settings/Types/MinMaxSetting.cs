using Model.Utility;

namespace Model.Helpers.Settings.Types;

public class MinMaxSetting : ISetting
{
    public string Name { get; }
    public ISettingViewInterface Interface { get; } 
    public MinMax Value { get; set; }

    public MinMaxSetting(string name, int minMin, int minMax, int maxMin, int maxMax, int tickFrequency, int minDefault = 0, int maxDefault = 1)
    {
        Name = name;
        Interface = new MinMaxSliderViewInterface(minMin, minMax, maxMin, maxMax, tickFrequency);
        Value = new MinMax(minDefault, maxDefault);
    }
    
    public SettingValue Get()
    {
        return new MinMaxSettingValue(Value);
    }

    public void Set(SettingValue s)
    {
        Value = s.ToMinMax();
    }
}