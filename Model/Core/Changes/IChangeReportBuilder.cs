using System.Collections.Generic;
using System.Text;
using Model.Core.Highlighting;

namespace Model.Core.Changes;

public interface IChangeReportBuilder<in TChange, in TVerifier, THighlighter> 
{
    public ChangeReport<THighlighter> BuildReport(IReadOnlyList<TChange> changes, TVerifier snapshot);
    
    public Clue<THighlighter> BuildClue(IReadOnlyList<TChange> changes, TVerifier snapshot);
}

public class DefaultNumericChangeReportBuilder<TVerifier, THighlighter> : IChangeReportBuilder<NumericChange, TVerifier, THighlighter>
    where TVerifier : INumericSolvingState where THighlighter : INumericSolvingStateHighlighter
{
    private static DefaultNumericChangeReportBuilder<TVerifier, THighlighter>? _instance;

    public static DefaultNumericChangeReportBuilder<TVerifier, THighlighter> Instance
    {
        get
        {
            _instance ??= new DefaultNumericChangeReportBuilder<TVerifier, THighlighter>();
            return _instance;
        }
    }
    
    public ChangeReport<THighlighter> BuildReport(IReadOnlyList<NumericChange> changes, TVerifier snapshot)
    {
        return new ChangeReport<THighlighter>("",
            lighter => { ChangeReportHelper.HighlightChanges(lighter, changes);});
    }

    public Clue<THighlighter> BuildClue(IReadOnlyList<NumericChange> changes, TVerifier snapshot)
    {
        return Clue<THighlighter>.Default();
    }
}

public static class ChangeReportHelper
{
    public static void HighlightChanges(INumericSolvingStateHighlighter highlightable, IReadOnlyList<NumericChange> changes)
    {
        foreach (var change in changes)
        {
            HighlightChange(highlightable, change);
        }
    }
    
    public static void HighlightChange(INumericSolvingStateHighlighter highlightable, NumericChange progress)
    {
        if(progress.Type == ChangeType.PossibilityRemoval)
            highlightable.HighlightPossibility(progress.Number, progress.Row, progress.Column, ChangeColoration.ChangeTwo);
        else highlightable.HighlightCell(progress.Row, progress.Column, ChangeColoration.ChangeOne);
    }

    public static string ChangesToString(IReadOnlyList<NumericChange> changes)
    {
        if (changes.Count == 0) return "";
        
        var builder = new StringBuilder();
        foreach (var change in changes)
        {
            var action = change.Type == ChangeType.PossibilityRemoval
                ? "<>"
                : "==";
            builder.Append($"r{change.Row + 1}c{change.Column + 1} {action} {change.Number}, ");
        }

        return builder.ToString()[..^2];
    }
}

public class Clue<T> : IHighlightable<T>
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
    None, Neutral, ChangeOne, ChangeTwo, CauseOffOne, CauseOffTwo, CauseOffThree,
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

