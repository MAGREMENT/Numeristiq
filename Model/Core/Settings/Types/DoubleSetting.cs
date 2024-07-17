namespace Model.Core.Settings.Types;

public class DoubleSetting : ISetting
{
    public event OnValueChange? ValueChanged;
    public string Name { get; }
    public string Description { get; }
    public ISettingInteractionInterface InteractionInterface { get; }
    public double Value { get; private set; }

    public DoubleSetting(string name, string description, ISettingInteractionInterface i, double defaultValue = 1)
    {
        Name = name;
        Description = description;
        InteractionInterface = i;
        Value = defaultValue;
    }

    public SettingValue Get()
    {
        return new DoubleSettingValue(Value);
    }

    public void Set(SettingValue value, bool checkValidity = true)
    {
        value = checkValidity ? InteractionInterface.Verify(value) : value;
        Value = value.ToDouble();
        ValueChanged?.Invoke(value);
    }
}