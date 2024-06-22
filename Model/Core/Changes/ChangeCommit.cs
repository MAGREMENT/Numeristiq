using Model.Core.Highlighting;

namespace Model.Core.Changes;

public interface IChangeCommit<out T>
{
    T[] Changes { get; }

    bool TryGetBuilder<TBuilderType>(out TBuilderType builder) where TBuilderType : class;
}

public class ChangeCommit<TChange, TVerifier, THighlighter> : IChangeCommit<TChange>
    where TVerifier : INumericSolvingState where THighlighter : INumericSolvingStateHighlighter
{
    public TChange[] Changes { get; }

    public IChangeReportBuilder<TChange, TVerifier, THighlighter> Builder { get; }

    public ChangeCommit(TChange[] changes, IChangeReportBuilder<TChange, TVerifier, THighlighter> builder)
    {
        Changes = changes;
        Builder = builder;
    }
    
    public bool TryGetBuilder<TBuilderType>(out TBuilderType builder) where TBuilderType : class
    {
        builder = Builder as TBuilderType;
        return builder is not null;
    }
}

public class BuiltChangeCommit<TChange, THighlighter>
{
    public BuiltChangeCommit(Strategy maker, TChange[] changes, ChangeReport<THighlighter> report)
    {
        Maker = maker;
        Changes = changes;
        Report = report;
    }
    
    public Strategy Maker { get; }
    public TChange[] Changes { get; }
    public ChangeReport<THighlighter> Report { get; }
}