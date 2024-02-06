using Model.Helpers;
using Model.Helpers.Changes;
using Model.Helpers.Logs;
using Model.Sudoku.Solver.Arguments;

namespace Presenter.Sudoku.Translators;

public static class ModelToViewTranslator
{
    public static ViewLog Translate(ISolverLog log)
    {
        return new ViewLog(log.Id, log.Title, log.Description, IChangeReportBuilder.ChangesToString(log.Changes), log.Intensity,
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

    public static ViewCommit[] Translate(BuiltChangeCommit[] commits)
    {
        var result = new ViewCommit[commits.Length];

        for (int i = 0; i < commits.Length; i++)
        {
            result[i] = new ViewCommit(commits[i].Maker.Name, (Intensity)commits[i].Maker.Difficulty);
        }

        return result;
    }

    public static ViewCommitInformation Translate(BuiltChangeCommit commit)
    {
        return new ViewCommitInformation(commit.Maker.Name, (Intensity)commit.Maker.Difficulty,
            IChangeReportBuilder.ChangesToString(commit.Changes) , commit.Report.HighlightManager.CursorPosition(),
            commit.Report.HighlightManager.Count);
    }
}