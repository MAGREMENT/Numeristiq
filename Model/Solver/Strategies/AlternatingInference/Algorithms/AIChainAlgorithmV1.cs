using System.Collections.Generic;
using Global.Enums;
using Model.Solver.StrategiesUtility.Graphs;

namespace Model.Solver.Strategies.AlternatingInference.Algorithms;

public class AIChainAlgorithmV1<T> : IAlternatingInferenceAlgorithm<T> where T : ILinkGraphElement //TODO v2
{
    public AlgorithmType Type => AlgorithmType.Chain;
    public void Run(IStrategyManager strategyManager, LinkGraph<T> graph, IAlternatingInferenceType<T> type)
    {
        HashSet<T> processed = new();

        foreach (var start in graph)
        {
            if (Search(strategyManager, graph, type, new LinkGraphChainBuilder<T>(start),
                    new HashSet<T> { start }, processed)) return;
            processed.Add(start);
        }
    }

    private bool Search(IStrategyManager manager, LinkGraph<T> graph, IAlternatingInferenceType<T> type,
        LinkGraphChainBuilder<T> builder, HashSet<T> explored, HashSet<T> processed)
    {
        var next = builder.LastLink() == LinkStrength.Strong ? LinkStrength.Weak : LinkStrength.Strong;
        var last = builder.LastElement();

        foreach (var friend in graph.GetLinks(last, next))
        {
            if (explored.Contains(friend)) continue;

            builder.Add(next, friend);
            explored.Add(friend);
            
            if (builder.Count >= 3 && !processed.Contains(friend) && next == LinkStrength.Strong &&
                Check(manager, graph, type, builder.ToChain())) return true;
            if (Search(manager, graph, type, builder, explored, processed)) return true;
            
            builder.RemoveLast();
        }

        if (next == LinkStrength.Weak)
        {
            foreach (var friend in graph.GetLinks(last, LinkStrength.Strong))
            {
                if (explored.Contains(friend)) continue;

                builder.Add(next, friend);
                explored.Add(friend);
                
                if (Search(manager, graph, type, builder, explored, processed)) return true;
            
                builder.RemoveLast();
            }
        }
        
        return false;
    }

    private bool Check(IStrategyManager manager, LinkGraph<T> graph, IAlternatingInferenceType<T> type,
        LinkGraphChain<T> chain)
    {
        return type.ProcessChain(manager, chain, graph);
    }
}