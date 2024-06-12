namespace Model.Core.Settings;

public interface ISettingCollection
{
    public void Set(int index, SettingValue value, bool checkValidity);
}