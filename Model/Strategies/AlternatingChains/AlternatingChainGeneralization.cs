using System.Collections.Generic;
using Model.StrategiesUtil;
using Model.StrategiesUtil.LoopFinder;

namespace Model.Strategies.AlternatingChains;

public class AlternatingChainGeneralization<T> : IStrategy where T : ILoopElement, ILinkGraphElement
{
    public string Name { get; }
    public StrategyLevel Difficulty { get; }
    public int Score { get; set; }

    private readonly IAlternatingChainType<T> _chain;
    private readonly IAlternatingChainAlgorithm<T> _algorithm;

    public AlternatingChainGeneralization(IAlternatingChainType<T> chainType, IAlternatingChainAlgorithm<T> algo)
    {
        _chain = chainType;
        _chain.Strategy = this;
        Name = chainType.Name;
        Difficulty = chainType.Difficulty;
        _algorithm = algo;
    }
    
    public void ApplyOnce(ISolverView solverView)
    {
        foreach (var graph in _chain.GetGraphs(solverView))
        {
            _algorithm.Run(solverView, graph, _chain);
        }
    }
}

public interface IAlternatingChainType<T> where T : ILoopElement, ILinkGraphElement
{
    public string Name { get; }
    public StrategyLevel Difficulty { get; }
    
    IStrategy? Strategy { get; set; }
    
    IEnumerable<LinkGraph<T>> GetGraphs(ISolverView view);

    bool ProcessFullLoop(ISolverView view, Loop<T> loop);

    bool ProcessWeakInference(ISolverView view, T inference);

    bool ProcessStrongInference(ISolverView view, T inference);
}

public interface IAlternatingChainAlgorithm<T> where T : ILoopElement, ILinkGraphElement
{
    void Run(ISolverView view, LinkGraph<T> graph, IAlternatingChainType<T> chainType);
}

