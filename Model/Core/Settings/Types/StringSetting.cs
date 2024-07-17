namespace Model.Core.Settings.Types;

public class StringSetting : ISetting
{
    public event OnValueChange? ValueChanged;
    public string Name { get; }
    public string Description { get; }
    public ISettingInteractionInterface InteractionInterface { get; }
    public string Value { get; private set; }

    public StringSetting(string name, string description, ISettingInteractionInterface i, string defaultValue = "")
    {
        Name = name;
        Description = description;
        InteractionInterface = i;
        Value = defaultValue;
    }

    public SettingValue Get()
    {
        return new StringSettingValue(Value);
    }

    public void Set(SettingValue value, bool checkValidity = true)
    {
        value = checkValidity ? InteractionInterface.Verify(value) : value;
        Value = value.ToString()!;
        ValueChanged?.Invoke(value);
    }
}