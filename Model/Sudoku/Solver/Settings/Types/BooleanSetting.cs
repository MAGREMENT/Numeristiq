namespace Model.Sudoku.Solver.Settings.Types;

public class BooleanSetting : ISetting
{
    public string Name { get; }
    public ISettingViewInterface Interface { get; }
    
    private readonly GetSetting<bool> _getter;
    private readonly SetSetting<bool> _setter;

    public BooleanSetting(string name, GetSetting<bool> getter, SetSetting<bool> setter)
    {
        Name = name;
        Interface = new BooleanViewInterface();
        _getter = getter;
        _setter = setter;
    }
    
    public SettingValue Get()
    {
        return new BoolSettingValue(_getter());
    }
    
    public void Set(SettingValue s)
    {
        _setter(s.ToBool());
    }
}