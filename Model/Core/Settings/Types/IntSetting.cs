namespace Model.Core.Settings.Types;

public class IntSetting : ISetting
{
    public string Name { get; }
    public ISettingInteractionInterface InteractionInterface { get; }
    public int Value { get; private set; }

    public IntSetting(string name, ISettingInteractionInterface i, int defaultValue = 1)
    {
        Name = name;
        InteractionInterface = i;
        Value = defaultValue;
    }

    public SettingValue Get()
    {
        return new IntSettingValue(Value);
    }

    public void Set(SettingValue value, bool checkValidity = true)
    {
        value = checkValidity ? InteractionInterface.Verify(value) : value;
        Value = value.ToInt();
    }
}