using System.Collections.Generic;
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
            lighter => ChangeReportHelper.HighlightChanges(lighter, changes));
    }

    public Clue<THighlighter> BuildClue(IReadOnlyList<NumericChange> changes, TVerifier snapshot)
    {
        return Clue<THighlighter>.Default();
    }
}

public class DefaultDichotomousChangeReportBuilder<TVerifier, THighlighter> : IChangeReportBuilder<DichotomousChange, TVerifier, THighlighter>
    where TVerifier : IDichotomousSolvingState
{
    private static DefaultDichotomousChangeReportBuilder<TVerifier, THighlighter>? _instance;

    public static DefaultDichotomousChangeReportBuilder<TVerifier, THighlighter> Instance
    {
        get
        {
            _instance ??= new DefaultDichotomousChangeReportBuilder<TVerifier, THighlighter>();
            return _instance;
        }
    }
    
    public ChangeReport<THighlighter> BuildReport(IReadOnlyList<DichotomousChange> changes, TVerifier snapshot)
    {
        return new ChangeReport<THighlighter>("",
            _ => {});
    }

    public Clue<THighlighter> BuildClue(IReadOnlyList<DichotomousChange> changes, TVerifier snapshot)
    {
        return Clue<THighlighter>.Default();
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
        _highlightable = new DelegateHighlightable<T>(highlight);
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

public enum StepColor
{
    Neutral = 1, Change1, Change2, Cause1, Cause2, Cause3,
    Cause4, Cause5, Cause6, Cause7, Cause8, Cause9, Cause10, On
}

public static class ChangeColorationUtility
{
    public static bool IsOff(StepColor color)
    {
        return (int)color is >= 4 and <= 13;
    }
}

