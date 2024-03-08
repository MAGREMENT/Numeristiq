using Model.Helpers.Settings;

namespace Repository;

public class SettingsJSONRepository : JSONRepository<Dictionary<string, SettingValue>>
{
    public SettingsJSONRepository(string fileName) : base(fileName)
    {
    }

    public override Dictionary<string, SettingValue>? Download()
    {
        var download = InternalDownload<SettingDAO[]>();
        if (download is null) return null;

        Dictionary<string, SettingValue> result = new();
        foreach (var dao in download)
        {
            result.Add(dao.Name, new StringSettingValue(dao.Value));
        }

        return result;
    }

    public override bool Upload(Dictionary<string, SettingValue> DAO)
    {
        var toUpload = new SettingDAO[DAO.Count];
        int cursor = 0;

        foreach (var entry in DAO)
        {
            toUpload[cursor++] = new SettingDAO(entry.Key, entry.Value.ToString()!);
        }

        return InternalUpload(toUpload);
    }
}

public record SettingDAO(string Name, string Value);