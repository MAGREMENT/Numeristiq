using Model.Helpers.Settings;
using Model.Sudoku.Solver;

namespace Repository;

public class SudokuStrategiesJSONRepository : JSONRepository<IReadOnlyList<SudokuStrategy>>
{
    public SudokuStrategiesJSONRepository(string fileName) : base(fileName)
    {
    }

    public override IReadOnlyList<SudokuStrategy>? Download()
    {
        var download = InternalDownload<StrategyDAO[]>();
        if (download is null) return null;

        var result = new List<SudokuStrategy>();
        foreach (var d in download)
        {
            var s = d.To();
            if (s is not null) result.Add(s);
        }

        return result;
    }

    public override bool Upload(IReadOnlyList<SudokuStrategy> DAO)
    {
        var toUpload = new StrategyDAO[DAO.Count];
        for (int i = 0; i < DAO.Count; i++)
        {
            toUpload[i] = StrategyDAO.From(DAO[i]);
        }

        return InternalUpload(toUpload);
    }
}

public record StrategyDAO(string Name, bool Enabled, bool Locked, OnCommitBehavior Behavior,
    Dictionary<string, string>? Settings)
{
    public static StrategyDAO From(SudokuStrategy strategy)
    {
        Dictionary<string, string> settings = new();

        foreach (var s in strategy.Settings)
        {
            settings.Add(s.Name, s.Get().ToString()!);
        }

        return new StrategyDAO(strategy.Name, strategy.Enabled, strategy.Locked, strategy.OnCommitBehavior, settings);
    }

    public SudokuStrategy? To()
    {
        var result = StrategyPool.CreateFrom(Name, Enabled, Locked, Behavior);
        if (result is null) return null;

        if (Settings is null) return result;
        
        foreach (var entry in Settings)
        {
            result.TrySetSetting(entry.Key, new StringSettingValue(entry.Value));
        }

        return result;
    }
}