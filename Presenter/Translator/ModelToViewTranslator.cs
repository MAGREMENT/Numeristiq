using Global;
using Global.Enums;
using Model.Solver;
using Model.Solver.Helpers.Logs;

namespace Presenter.Translator;

public static class ModelToViewTranslator
{
    public static IReadOnlyList<ViewLog> Translate(IReadOnlyList<ISolverLog> logs)
    {
        var result = new List<ViewLog>(logs.Count);

        foreach (var log in logs)
        {
            result.Add(new ViewLog(log.Id, log.Title, log.Changes, log.Intensity, log.HighlightManager.Count));
        }

        return result;
    }

    public static IReadOnlyList<ViewStrategy> Translate(StrategyInfo[] infos)
    {
        var result = new List<ViewStrategy>(infos.Length);

        foreach (var info in infos)
        {
            result.Add(new ViewStrategy(info.StrategyName, (Intensity)info.Difficulty, info.Used, info.Locked));
        }
        
        return result;
    }
}