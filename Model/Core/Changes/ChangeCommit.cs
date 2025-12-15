namespace Model.Core.Changes;

public interface IChangeCommit<out T>
{
    T[] Changes { get; }

    bool TryGetBuilder<TBuilderType>(out TBuilderType? builder) where TBuilderType : class;
}

public class ChangeCommit<TChange, TVerifier, THighlighter> : IChangeCommit<TChange>
{
    public TChange[] Changes { get; }

    public IChangeReportBuilder<TChange, TVerifier, THighlighter> Builder { get; }

    public ChangeCommit(TChange[] changes, IChangeReportBuilder<TChange, TVerifier, THighlighter> builder)
    {
        Changes = changes;
        Builder = builder;
    }
    
    public bool TryGetBuilder<TBuilderType>(out TBuilderType? builder) where TBuilderType : class
    {
        builder = Builder as TBuilderType;
        return builder is not null;
    }
}