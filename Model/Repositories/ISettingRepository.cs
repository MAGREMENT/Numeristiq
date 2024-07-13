using System.Collections.Generic;
using Model.Core.Settings;

namespace Model.Repositories;

public interface ISettingRepository
{
    Dictionary<string, SettingValue> GetSettings();
    void UpdateSetting(ISetting setting);
    void UpdateSettings(IEnumerable<ISetting> setting);
}