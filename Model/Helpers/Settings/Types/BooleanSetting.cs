namespace Model.Helpers.Settings.Types;

public class BooleanSetting : ISetting
{
    public string Name { get; }
    public ISettingInteractionInterface InteractionInterface { get; } = new CheckBoxInteractionInterface();
    public bool Value { get; set; }

    public BooleanSetting(string name, bool defaultValue = false)
    {
        Name = name;
        Value = defaultValue;
    }
    
    public SettingValue Get()
    {
        return new BoolSettingValue(Value);
    }
    
    public void Set(SettingValue value)
    {
        Value = value.ToBool();
    }
}