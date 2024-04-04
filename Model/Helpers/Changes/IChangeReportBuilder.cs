using System.Collections.Generic;
using System.Text;
using Model.Helpers.Highlighting;

namespace Model.Helpers.Changes;

public interface IChangeReportBuilder<in TVerifier, THighlighter> where TVerifier : ISolvingState where THighlighter : ISolvingStateHighlighter
{
    public ChangeReport<THighlighter> BuildReport(IReadOnlyList<SolverProgress> changes, TVerifier snapshot);
    
    public Clue<THighlighter> BuildClue(IReadOnlyList<SolverProgress> changes, TVerifier snapshot);
}

public class DefaultChangeReportBuilder<TVerifier, THighlighter> : IChangeReportBuilder<TVerifier, THighlighter> where TVerifier : ISolvingState where THighlighter : ISolvingStateHighlighter
{
    private static DefaultChangeReportBuilder<TVerifier, THighlighter>? _instance;

    public static DefaultChangeReportBuilder<TVerifier, THighlighter> Instance
    {
        get
        {
            _instance ??= new DefaultChangeReportBuilder<TVerifier, THighlighter>();
            return _instance;
        }
    }
    
    public ChangeReport<THighlighter> BuildReport(IReadOnlyList<SolverProgress> changes, TVerifier snapshot)
    {
        return new ChangeReport<THighlighter>("",
            lighter => { ChangeReportHelper.HighlightChanges(lighter, changes);});
    }

    public Clue<THighlighter> BuildClue(IReadOnlyList<SolverProgress> changes, TVerifier snapshot)
    {
        return Clue<THighlighter>.Default();
    }
}

public static class ChangeReportHelper
{
    public static void HighlightChanges(ISolvingStateHighlighter highlightable, IReadOnlyList<SolverProgress> changes)
    {
        foreach (var change in changes)
        {
            HighlightChange(highlightable, change);
        }
    }
    
    public static void HighlightChange(ISolvingStateHighlighter highlightable, SolverProgress progress)
    {
        if(progress.ProgressType == ProgressType.PossibilityRemoval)
            highlightable.HighlightPossibility(progress.Number, progress.Row, progress.Column, ChangeColoration.ChangeTwo);
        else highlightable.HighlightCell(progress.Row, progress.Column, ChangeColoration.ChangeOne);
    }

    public static string ChangesToString(IReadOnlyList<SolverProgress> changes)
    {
        if (changes.Count == 0) return "";
        
        var builder = new StringBuilder();
        foreach (var change in changes)
        {
            var action = change.ProgressType == ProgressType.PossibilityRemoval
                ? "<>"
                : "==";
            builder.Append($"r{change.Row + 1}c{change.Column + 1} {action} {change.Number}, ");
        }

        return builder.ToString()[..^2];
    }
}

public class Clue<T> : IHighlightable<T> where T : ISolvingStateHighlighter
{
    private readonly IHighlightable<T>? _highlightable;
    public string Text { get; }

    public static Clue<T> Default()
    {
        return new Clue<T>("Clue system not implemented for this instance");
    }
    
    public Clue(Highlight<T> highlight, string text)
    {
        Text = text;
        _highlightable = HighlightCompiler.For<T>().Compile(highlight);
    }

    public Clue(string text)
    {
        Text = text;
        _highlightable = null;
    }

    public void Highlight(T highlighter)
    {
        _highlightable?.Highlight(highlighter);
    }
}

public enum ChangeColoration
{
    None = 0, Neutral, ChangeOne, ChangeTwo, CauseOffOne, CauseOffTwo, CauseOffThree,
    CauseOffFour, CauseOffFive, CauseOffSix, CauseOffSeven, CauseOffEight, CauseOffNine,
    CauseOffTen, CauseOnOne
}

public static class ChangeColorationUtility
{
    public static bool IsOff(ChangeColoration coloration)
    {
        return (int)coloration is >= 4 and <= 13;
    }
}

