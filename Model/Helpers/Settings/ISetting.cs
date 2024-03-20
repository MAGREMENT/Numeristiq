namespace Model.Helpers.Settings;

public interface ISetting : IReadOnlySetting
{
    public void Set(SettingValue s);
}

public interface IReadOnlySetting
{
    public string Name { get; }
    public ISettingInteractionInterface InteractionInterface { get; }
    public SettingValue Get();
}