namespace Model.Sudoku.Solver.Settings;

public interface ISetting
{
    public string Name { get; }
    public ISettingViewInterface Interface { get; }
    public SettingValue Get();
    public void Set(SettingValue s);
}

public delegate T GetSetting<out T>();
public delegate void SetSetting<in T>(T value);