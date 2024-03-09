namespace Model.Helpers.Settings.Types;

public class BooleanSetting : ISetting
{
    public string Name { get; }
    public ISettingInteractionInterface InteractionInterface { get; } = new BooleanInteractionInterface();
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
        var old = Value;
        Value = value.ToBool();
        
        if(old != Value) Changed?.Invoke();
    }

    public event OnSettingChange? Changed;
}