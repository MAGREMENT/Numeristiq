namespace Model.Core.Settings;

public interface ISetting : IReadOnlySetting
{
    public void Set(SettingValue value, bool checkValidity = true);
}

public interface IReadOnlySetting
{
    public string Name { get; }
    public ISettingInteractionInterface InteractionInterface { get; }
    public SettingValue Get();
}