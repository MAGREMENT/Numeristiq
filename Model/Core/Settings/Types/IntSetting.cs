namespace Model.Core.Settings.Types;

public class IntSetting : ISetting
{
    public event OnValueChange? ValueChanged;
    public string Name { get; }
    public string Description { get; }
    public ISettingInteractionInterface InteractionInterface { get; }
    public int Value { get; private set; }

    public IntSetting(string name, string description, ISettingInteractionInterface i, int defaultValue = 1)
    {
        Name = name;
        Description = description;
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
        ValueChanged?.Invoke(value);
    }
}