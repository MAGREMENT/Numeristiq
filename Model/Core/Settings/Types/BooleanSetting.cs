namespace Model.Core.Settings.Types;

public class BooleanSetting : ISetting
{
    public event OnValueChange? ValueChanged;
    public string Name { get; }
    public string Description { get; }
    public ISettingInteractionInterface InteractionInterface { get; } = new CheckBoxInteractionInterface();
    public bool Value { get; private set; }

    public BooleanSetting(string name, string description, bool defaultValue = false)
    {
        Name = name;
        Description = description;
        Value = defaultValue;
    }
    
    public SettingValue Get()
    {
        return new BoolSettingValue(Value);
    }

    public void Set(SettingValue value, bool checkValidity = true)
    {
        value = checkValidity ? InteractionInterface.Verify(value) : value;
        Value = value.ToBool();
        ValueChanged?.Invoke(value);
    }
}