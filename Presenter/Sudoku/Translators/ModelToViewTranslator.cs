using Model.Helpers.Changes;
using Model.Helpers.Logs;
using Model.Helpers.Settings;
using Model.Sudoku.Solver;

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
    
    public static ViewStrategy Translate(SudokuStrategy info)
    {
        List<ViewStrategyArgument> interfaces = new(info.Settings.Count);
        foreach (var arg in info.Settings)
        {
            interfaces.Add(Translate(arg));
        }
        return new ViewStrategy(info.Name, (Intensity)info.Difficulty, info.Enabled, info.Locked, info.OnCommitBehavior, interfaces);
    }

    private static ViewStrategyArgument Translate(ISetting argument)
    {
        return new ViewStrategyArgument(argument.Name, argument.Interface, argument.Get());
    }

    public static IReadOnlyList<ViewStrategy> Translate(IReadOnlyList<SudokuStrategy> infos)
    {
        var result = new List<ViewStrategy>(infos.Count);

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