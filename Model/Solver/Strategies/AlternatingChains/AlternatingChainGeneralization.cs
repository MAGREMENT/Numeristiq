using System;
using System.Collections.Generic;
using Model.Solver.Helpers.Changes;
using Model.Solver.Helpers.Highlighting;
using Model.Solver.StrategiesUtil.LinkGraph;

namespace Model.Solver.Strategies.AlternatingChains;

//TODO : Add commit behavior to other algorithms than V2
public class AlternatingChainGeneralization<T> : AbstractStrategy where T : ILinkGraphElement
{
    private const OnCommitBehavior DefaultBehavior = OnCommitBehavior.Return;
    
    public override OnCommitBehavior DefaultOnCommitBehavior => DefaultBehavior;
    
    private readonly IAlternatingChainType<T> _chain;
    private readonly IAlternatingChainAlgorithm<T> _algorithm;

    public AlternatingChainGeneralization(IAlternatingChainType<T> chainType, IAlternatingChainAlgorithm<T> algo)
        : base(chainType.Name, chainType.Difficulty, DefaultBehavior)
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

public interface IAlternatingChainType<T> where T : ILinkGraphElement
{
    public string Name { get; }
    public StrategyDifficulty Difficulty { get; }
    IStrategy? Strategy { set; get; }
    
    LinkGraph<T> GetGraph(IStrategyManager view);

    bool ProcessFullLoop(IStrategyManager view, Loop<T> loop);

    bool ProcessWeakInference(IStrategyManager view, T inference, Loop<T> loop);

    bool ProcessStrongInference(IStrategyManager view, T inference, Loop<T> loop);
}

public interface IAlternatingChainAlgorithm<T> where T : ILinkGraphElement
{
    void Run(IStrategyManager view, LinkGraph<T> graph, IAlternatingChainType<T> chainType);
}

public class AlternatingChainReportBuilder<T> : IChangeReportBuilder where T : ILinkGraphElement
{
    private readonly Loop<T> _loop;
    private readonly LoopType _type;

    public AlternatingChainReportBuilder(Loop<T> loop, LoopType type)
    {
        _loop = loop;
        _type = type;
    }

    public ChangeReport Build(List<SolverChange> changes, IPossibilitiesHolder snapshot)
    {
        return new ChangeReport(IChangeReportBuilder.ChangesToString(changes), Explanation(),
            lighter =>
            {
                var coloring = _loop.Links[0] == LinkStrength.Strong
                    ? ChangeColoration.CauseOffOne
                    : ChangeColoration.CauseOnOne;
                
                foreach (var element in _loop)
                {
                    lighter.HighlightLinkGraphElement(element, coloring);
                    coloring = coloring == ChangeColoration.CauseOnOne
                        ? ChangeColoration.CauseOffOne
                        : ChangeColoration.CauseOnOne;
                }
                
                _loop.ForEachLink((one, two) => lighter.CreateLink(one, two, LinkStrength.Strong), LinkStrength.Strong);
                _loop.ForEachLink((one, two) => lighter.CreateLink(one, two, LinkStrength.Weak), LinkStrength.Weak);
                
                IChangeReportBuilder.HighlightChanges(lighter, changes);
            });
    }

    private string Explanation()
    {
        var result = _type switch
        {
            LoopType.NiceLoop => "Nice loop",
            LoopType.StrongInference => "Loop with a strong inference",
            LoopType.WeakInference => "Loop with a weak inference",
            _ => throw new ArgumentOutOfRangeException()
        };

        return result + $" found\nLoop :: {_loop}";
    }
}

public enum LoopType
{
    NiceLoop, WeakInference, StrongInference
}

