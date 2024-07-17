namespace Model.Core.Settings;

public interface ISetting : IReadOnlySetting
{
    public void Set(SettingValue value, bool checkValidity = true);
}

public interface IReadOnlySetting
{
    public event OnValueChange? ValueChanged;
    
    public string Name { get; }
    public string Description { get; }
    public ISettingInteractionInterface InteractionInterface { get; }
    public SettingValue Get();
}

public delegate void OnValueChange(SettingValue setting);