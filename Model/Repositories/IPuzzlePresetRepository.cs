using System.Collections.Generic;

namespace Model.Repositories;

public interface IPuzzlePresetRepository
{
    IEnumerable<(string, string)> GetPresets();
}