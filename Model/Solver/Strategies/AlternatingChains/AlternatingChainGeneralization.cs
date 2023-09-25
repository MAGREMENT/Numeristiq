using System.Collections.Generic;
using Model.Solver.Helpers.Changes;
using Model.Solver.StrategiesUtil.LinkGraph;

namespace Model.Solver.Strategies.AlternatingChains;

public class AlternatingChainGeneralization<T> : AbstractStrategy where T : ILoopElement, ILinkGraphElement
{
    private readonly IAlternatingChainType<T> _chain;
    private readonly IAlternatingChainAlgorithm<T> _algorithm;

    public AlternatingChainGeneralization(IAlternatingChainType<T> chainType, IAlternatingChainAlgorithm<T> algo) : base(chainType.Name, chainType.Difficulty)
    {
        _chain = chainType;
        _chain.Strategy = this;
        _algorithm = algo;
    }
    
    public override void ApplyOnce(IStrategyManager strategyManager)
    {
        _algorithm.Run(strategyManager, _chain.GetGraph(strategyManager), _chain);
    }
}

public interface IAlternatingChainType<T> where T : ILoopElement, ILinkGraphElement
{
    public string Name { get; }
    public StrategyDifficulty Difficulty { get; }
    IStrategy? Strategy { set; }
    
    LinkGraph<T> GetGraph(IStrategyManager view);

    bool ProcessFullLoop(IStrategyManager view, Loop<T> loop);

    bool ProcessWeakInference(IStrategyManager view, T inference, Loop<T> loop);

    bool ProcessStrongInference(IStrategyManager view, T inference, Loop<T> loop);
}

public interface IAlternatingChainAlgorithm<T> where T : ILoopElement, ILinkGraphElement
{
    void Run(IStrategyManager view, LinkGraph<T> graph, IAlternatingChainType<T> chainType);
}

public class AlternatingChainReportBuilder<T> : IChangeReportBuilder where T : ILinkGraphElement
{
    private readonly Loop<T> _loop;

    public AlternatingChainReportBuilder(Loop<T> loop)
    {
        _loop = loop;
    }

    public ChangeReport Build(List<SolverChange> changes, IPossibilitiesHolder snapshot)
    {
        return new ChangeReport(IChangeReportBuilder.ChangesToString(changes), _loop.ToString(),
            lighter =>
            {
                int counter = 0;
                foreach (var element in _loop)
                {
                    lighter.HighlightLinkGraphElement(element, counter % 2 == 0 ? ChangeColoration.CauseOffOne : ChangeColoration.CauseOnOne);
                    counter++;
                }
                
                _loop.ForEachLink((one, two) => lighter.CreateLink(one, two, LinkStrength.Strong), LinkStrength.Strong);
                _loop.ForEachLink((one, two) => lighter.CreateLink(one, two, LinkStrength.Weak), LinkStrength.Weak);
                
                IChangeReportBuilder.HighlightChanges(lighter, changes);
            });
    }
}

