namespace Model.Helpers.Settings.Types;

public class IntSetting : ISetting
{
    public string Name { get; }
    public ISettingViewInterface Interface { get; }
    public int Value { get; set; }

    public IntSetting(string name, ISettingViewInterface i, int defaultValue = 1)
    {
        Name = name;
        Interface = i;
        Value = defaultValue;
    }

    public SettingValue Get()
    {
        return new IntSettingValue(Value);
    }

    public void Set(SettingValue value)
    {
        Value = (value.ToInt());
    }
}