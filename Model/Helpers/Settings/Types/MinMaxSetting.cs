using Model.Utility;

namespace Model.Helpers.Settings.Types;

public class MinMaxSetting : ISetting
{
    public string Name { get; }
    public ISettingInteractionInterface InteractionInterface { get; } 
    public MinMax Value { get; private set; }

    public MinMaxSetting(string name, int minMin, int minMax, int maxMin, int maxMax, int tickFrequency, int minDefault = 0, int maxDefault = 1)
    {
        Name = name;
        InteractionInterface = new MinMaxSliderInteractionInterface(minMin, minMax, maxMin, maxMax, tickFrequency);
        Value = new MinMax(minDefault, maxDefault);
    }
    
    public SettingValue Get()
    {
        return new MinMaxSettingValue(Value);
    }

    public void Set(SettingValue value, bool checkValidity = true)
    {
        value = checkValidity ? InteractionInterface.Verify(value) : value;
        Value = value.ToMinMax();
    }
}