namespace Model.Sudoku.Solver.Settings.Types;

public class IntSetting : ISetting
{
    public string Name { get; }
    public ISettingViewInterface Interface { get; }

    private readonly GetSetting<int> _getter;
    private readonly SetSetting<int> _setter;
    
    public IntSetting(string name, GetSetting<int> getter, SetSetting<int> setter, ISettingViewInterface i)
    {
        Name = name;
        Interface = i;
        _getter = getter;
        _setter = setter;
    }

    public SettingValue Get()
    {
        return new IntSettingValue(_getter());
    }

    public void Set(SettingValue value)
    {
        _setter(value.ToInt());
    }
}