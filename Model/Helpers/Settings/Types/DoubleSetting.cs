namespace Model.Helpers.Settings.Types;

public class DoubleSetting : ISetting
{
    public string Name { get; }
    public ISettingInteractionInterface InteractionInterface { get; }
    public double Value { get; private set; }

    public DoubleSetting(string name, ISettingInteractionInterface i, double defaultValue = 1)
    {
        Name = name;
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
    }
}