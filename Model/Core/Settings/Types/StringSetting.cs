namespace Model.Core.Settings.Types;

public class StringSetting : ISetting
{
    public string Name { get; }
    public ISettingInteractionInterface InteractionInterface { get; }
    public string Value { get; private set; }

    public StringSetting(string name, ISettingInteractionInterface i, string defaultValue = "")
    {
        Name = name;
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
    }
}