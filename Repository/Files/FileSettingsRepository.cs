using Model.Core.Settings;
using Model.Repositories;

namespace Repository.Files;

public class FileSettingsRepository : FileRepository<Dictionary<string, string>>, ISettingRepository
{
    private Dictionary<string, string>? _buffer;
    
    public FileSettingsRepository(string name, bool searchParentDirectories, bool createIfNotFound,
        IFileType<Dictionary<string, string>> type) : base(name, searchParentDirectories, createIfNotFound, type)
    {
    }
    
    public Dictionary<string, SettingValue> GetSettings()
    {
        _buffer ??= Download();
        return _buffer is null ? new Dictionary<string, SettingValue>() : To(_buffer);
    }

    public void UpdateSetting(ISetting setting)
    {
        _buffer ??= Download();
        if (_buffer is null) return;
 
        _buffer[setting.Name] = setting.Get().ToString()!;
        Upload(_buffer);
    }

    public void UpdateSettings(IEnumerable<ISetting> settings)
    {
        _buffer ??= Download();
        if (_buffer is null) return;

        foreach (var setting in settings)
        {
            _buffer[setting.Name] = setting.Get().ToString()!;
        }
        
        Upload(_buffer);
    }

    private static Dictionary<string, SettingValue> To(Dictionary<string, string> dic)
    {
        var result = new Dictionary<string, SettingValue>(dic.Count);
        foreach (var entry in dic) result.Add(entry.Key, new StringSettingValue(entry.Value));
        return result;
    }
}