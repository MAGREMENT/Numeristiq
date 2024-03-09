namespace Model.Helpers.Settings;

public interface ISetting
{
    public string Name { get; }
    public ISettingInteractionInterface InteractionInterface { get; }
    public SettingValue Get();
    public void Set(SettingValue s);

    public event OnSettingChange? Changed;
}

public delegate void OnSettingChange();