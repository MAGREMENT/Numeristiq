using System.Collections.Generic;
using Global.Enums;
using Model.Solver.StrategiesUtility.LinkGraph;

namespace Model.Solver.Strategies.AlternatingChains.ChainAlgorithms;

public class AlternatingChainAlgorithmV3<T> : IAlternatingChainAlgorithm<T> where T : ILinkGraphElement
{
    private readonly int _maxSearchRadius;

    public AlternatingChainAlgorithmV3(int maxSearchRadius)
    {
        _maxSearchRadius = maxSearchRadius;
    }

    public void Run(IStrategyManager view, LinkGraph<T> graph, IAlternatingChainType<T> chainType)
    {
        foreach (var start in graph)
        {
            Dictionary<T, LoopBuilder<T>> paths = new();
            LoopBuilder<T> builder = new(start);

            foreach (var friend in graph.GetLinks(start, LinkStrength.Strong))
            {
                AddToPaths(graph, paths, builder.Add(friend, LinkStrength.Strong));
            }

            foreach (var friend in graph.GetLinks(start, LinkStrength.Weak))
            {
                TryMakeLoop(graph, view, chainType, paths, builder.Add(friend, LinkStrength.Weak));
            }
        }
    }
    
    private void AddToPaths(LinkGraph<T> graph, Dictionary<T, LoopBuilder<T>> paths, LoopBuilder<T> builder)
    {
        if (builder.Count > _maxSearchRadius) return;
        
        var current = builder.LastElement();
        if (!paths.TryAdd(current, builder)) return;

        var nextLink = builder.LastLink() == LinkStrength.Strong ? LinkStrength.Weak : LinkStrength.Strong;
        foreach (var friend in graph.GetLinks(current, nextLink))
        {
            if(builder.ContainsElement(friend)) continue;
            AddToPaths(graph, paths, builder.Add(friend, nextLink));
        }

        if (nextLink == LinkStrength.Weak)
        {
            foreach (var friend in graph.GetLinks(current, LinkStrength.Strong))
            {
                if(builder.ContainsElement(friend)) continue;
                AddToPaths(graph, paths, builder.Add(friend, LinkStrength.Weak));
            }
        }
    }

    private void TryMakeLoop(LinkGraph<T> graph, IStrategyManager strategyManager, IAlternatingChainType<T> chainType,
        Dictionary<T, LoopBuilder<T>> paths, LoopBuilder<T> builder)
    {
        if (builder.Count > _maxSearchRadius) return;

        var current = builder.LastElement();
        if (paths.TryGetValue(current, out var otherBuilder))
        {
            var merged = builder.TryMerging(otherBuilder);
            if (merged == null || merged.Count < 4) return;

            var ll1 = builder.LastLink();
            var ll2 = otherBuilder.LastLink();

            switch (ll1, ll2)
            {
                case(LinkStrength.Strong, LinkStrength.Weak) :
                case(LinkStrength.Weak, LinkStrength.Strong) :
                    chainType.ProcessFullLoop(strategyManager, merged);
                    break;
                case(LinkStrength.Strong, LinkStrength.Strong) :
                    chainType.ProcessStrongInference(strategyManager, current, merged);
                    break;
                case(LinkStrength.Weak, LinkStrength.Weak) :
                    chainType.ProcessWeakInference(strategyManager, current, merged);
                    break;
            }
            
            return;
        }

        var nextLink = builder.LastLink() == LinkStrength.Strong ? LinkStrength.Weak : LinkStrength.Strong;
        foreach (var friend in graph.GetLinks(current, nextLink))
        {
            if(builder.ContainsElement(friend)) continue;
            TryMakeLoop(graph, strategyManager, chainType, paths, builder.Add(friend, nextLink));
        }

        if (nextLink == LinkStrength.Weak)
        {
            foreach (var friend in graph.GetLinks(current, LinkStrength.Strong))
            {
                if(builder.ContainsElement(friend)) continue;
                TryMakeLoop(graph, strategyManager, chainType, paths, builder.Add(friend, LinkStrength.Weak));
            }
        }
    }
}