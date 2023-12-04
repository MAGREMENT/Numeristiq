using Global;
using Global.Enums;
using Model.Solver;
using Model.Solver.Helpers;
using Model.Solver.Helpers.Logs;

namespace Presenter.Translators;

public static class ModelToViewTranslator
{
    public static ViewLog Translate(ISolverLog log)
    {
        return new ViewLog(log.Id, log.Title, log.Changes, log.Intensity,
            log.HighlightManager.CursorPosition(), log.HighlightManager.Count);
    }
    
    public static IReadOnlyList<ViewLog> Translate(IReadOnlyList<ISolverLog> logs)
    {
        var result = new List<ViewLog>(logs.Count);

        foreach (var log in logs)
        {
            result.Add(Translate(log));
        }

        return result;
    }
    
    public static ViewStrategy Translate(StrategyInformation info)
    {
        List<ViewStrategyArgument> interfaces = new(info.Arguments.Count);
        foreach (var arg in info.Arguments)
        {
            interfaces.Add(Translate(arg));
        }
        return new ViewStrategy(info.StrategyName, (Intensity)info.Difficulty, info.Used, info.Locked, info.Behavior, interfaces);
    }

    private static ViewStrategyArgument Translate(IStrategyArgument argument)
    {
        return new ViewStrategyArgument(argument.Name, argument.Interface, argument.Get());
    }

    public static IReadOnlyList<ViewStrategy> Translate(StrategyInformation[] infos)
    {
        var result = new List<ViewStrategy>(infos.Length);

        foreach (var info in infos)
        {
            result.Add(Translate(info));
        }
        
        return result;
    }
}