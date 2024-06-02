using Model.Core;
using Model.Helpers.Highlighting;

namespace Model.Helpers.Changes;

public interface IChangeCommit
{
    SolverProgress[] Changes { get; }

    bool TryGetBuilder<TBuilderType>(out TBuilderType builder) where TBuilderType : class;
}

public class ChangeCommit<TVerifier, THighlighter> : IChangeCommit 
    where TVerifier : ISolvingState where THighlighter : ISolvingStateHighlighter
{
    public SolverProgress[] Changes { get; }

    public IChangeReportBuilder<TVerifier, THighlighter> Builder { get; }

    public ChangeCommit(SolverProgress[] changes, IChangeReportBuilder<TVerifier, THighlighter> builder)
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

public class BuiltChangeCommit<THighlighter>
{
    public BuiltChangeCommit(Strategy maker, SolverProgress[] changes, ChangeReport<THighlighter> report)
    {
        Maker = maker;
        Changes = changes;
        Report = report;
    }
    
    public Strategy Maker { get; }
    public SolverProgress[] Changes { get; }
    public ChangeReport<THighlighter> Report { get; }
}