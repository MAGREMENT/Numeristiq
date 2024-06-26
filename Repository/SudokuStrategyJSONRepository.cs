using Model;
using Model.Core;
using Model.Core.Settings;
using Model.Sudokus.Solver;

namespace Repository;

public class SudokuStrategiesJSONRepository : JSONRepository<IReadOnlyList<SudokuStrategy>>
{
    public SudokuStrategiesJSONRepository(string filePath) : base(filePath)
    {
    }

    public override IReadOnlyList<SudokuStrategy>? Download()
    {
        return To(InternalDownload<StrategyDAO[]>());
    }

    public override bool Upload(IReadOnlyList<SudokuStrategy> DAO)
    {
        return InternalUpload(To(DAO));
    }
    
    public IReadOnlyList<SudokuStrategy>? Download(Stream stream)
    {
        return To(InternalDownload<StrategyDAO[]>(stream));
    }

    public bool Upload(IReadOnlyList<SudokuStrategy> DAO, Stream stream)
    {
        return InternalUpload(To(DAO), stream);
    }

    private static List<SudokuStrategy>? To(StrategyDAO[]? download)
    {
        if (download is null) return null;

        var result = new List<SudokuStrategy>();
        foreach (var d in download)
        {
            var s = d.To();
            if (s is not null) result.Add(s);
        }

        return result;
    }

    private static StrategyDAO[] To(IReadOnlyList<SudokuStrategy> DAO)
    {
        var toUpload = new StrategyDAO[DAO.Count];
        for (int i = 0; i < DAO.Count; i++)
        {
            toUpload[i] = StrategyDAO.From(DAO[i]);
        }

        return toUpload;
    }
}

public record StrategyDAO(string Name, bool Enabled, bool Locked, InstanceHandling InstanceHandling,
    Dictionary<string, string>? Settings)
{
    public static StrategyDAO From(SudokuStrategy strategy)
    {
        Dictionary<string, string> settings = new();

        foreach (var s in strategy.Settings)
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