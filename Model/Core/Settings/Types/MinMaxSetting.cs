using Model.Utility;

namespace Model.Core.Settings.Types;

public class MinMaxSetting : ISetting
{
    public event OnValueChange? ValueChanged;
    public string Name { get; }
    public string Description { get; }
    public ISettingInteractionInterface InteractionInterface { get; } 
    public MinMax Value { get; private set; }

    public MinMaxSetting(string name, string description, int minMin, int minMax, int maxMin, int maxMax,
        int tickFrequency, int minDefault = 0, int maxDefault = 1)
    {
        Name = name;
        Description = description;
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
        ValueChanged?.Invoke(value);
    }
}