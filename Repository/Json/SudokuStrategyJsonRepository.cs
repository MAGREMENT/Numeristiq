using Model.Core;
using Model.Core.Settings;
using Model.Repositories;
using Model.Sudokus.Solver;

namespace Repository.Json;

public class SudokuStrategyJsonRepository : JsonRepository, IStrategyRepository<SudokuStrategy>
{
    private List<StrategyDAO>? _buffer;
    
    public SudokuStrategyJsonRepository(string filePath, bool searchParentDirectories, bool createIfNotFound) 
        : base(filePath, searchParentDirectories, createIfNotFound)
    {
    }

    public void SetStrategies(IReadOnlyList<SudokuStrategy> list)
    {
        _buffer = From(list);
        Upload(_buffer);
    }

    public IEnumerable<SudokuStrategy> GetStrategies()
    {
        _buffer ??= Download<List<StrategyDAO>>();
        return _buffer is null ? Enumerable.Empty<SudokuStrategy>() : To(_buffer);
    }

    public void UpdateStrategy(SudokuStrategy strategy)
    {
        _buffer ??= Download<List<StrategyDAO>>();
        if (_buffer is null) return;

        var index = IndexOf(strategy.Name);
        if (index != -1)
        {
            _buffer[index] = StrategyDAO.From(strategy);
            Upload(_buffer);
        }
    }

    public void AddPreset(IReadOnlyList<SudokuStrategy> list, Stream stream)
    {
        Upload(From(list), stream);
    }

    public IEnumerable<SudokuStrategy> GetPreset(Stream stream)
    {
        _buffer = Download<List<StrategyDAO>>(stream);
        return _buffer is null ? Enumerable.Empty<SudokuStrategy>() : To(_buffer);
    }

    private int IndexOf(string name)
    {
        for (int i = 0; i < _buffer!.Count; i++)
        {
            if (_buffer[i].Name.Equals(name)) return i;
        }

        return -1;
    }

    private static List<SudokuStrategy> To(IEnumerable<StrategyDAO> download)
    {
        var result = new List<SudokuStrategy>();
        foreach (var d in download)
        {
            var s = d.To();
            if (s is not null) result.Add(s);
        }

        return result;
    }

    private static List<StrategyDAO> From(IReadOnlyList<SudokuStrategy> list)
    {
        var result = new List<StrategyDAO>(list.Count);
        foreach (var e in list) result.Add(StrategyDAO.From(e));
        return result;
    }
}

public record StrategyDAO(string Name, bool Enabled, bool Locked, InstanceHandling InstanceHandling,
    Dictionary<string, string>? Settings)
{
    public static StrategyDAO From(SudokuStrategy strategy)
    {
        Dictionary<string, string> settings = new();

        foreach (var s in strategy.EnumerateSettings())
        {
            settings.Add(s.Name, s.Get().ToString()!);
        }

        return new StrategyDAO(strategy.Name, strategy.Enabled, strategy.Locked, strategy.InstanceHandling, settings);
    }

    public SudokuStrategy? To()
    {
        var result = StrategyPool.CreateFrom(Name, Enabled, Locked, InstanceHandling);
        if (result is null) return null;

        if (Settings is null) return result;
        
        foreach (var entry in Settings)
        {
            result.TrySetSetting(entry.Key, new StringSettingValue(entry.Value));
        }

        return result;
    }
}