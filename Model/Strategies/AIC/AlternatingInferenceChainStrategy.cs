namespace Model.Strategies.AIC;

public interface IAlternatingInferenceChainStrategy : IStrategy
{
    public long SearchCount { get; }
}