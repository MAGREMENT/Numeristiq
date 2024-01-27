using System.Collections.Generic;
using Global.Enums;
using Model.SudokuSolving.Solver.StrategiesUtility.Graphs;

namespace Model.SudokuSolving.Solver.Strategies.AlternatingInference.Algorithms;

public class AIChainAlgorithmV2<T> : IAlternatingInferenceAlgorithm<T> where T : ISudokuElement
{
    public AlgorithmType Type => AlgorithmType.Chain;
    public void Run(IStrategyManager strategyManager, IAlternatingInferenceType<T> type)
    {
        var graph = type.GetGraph(strategyManager);
        HashSet<T> processed = new();

        foreach (var start in graph)
        {
            if (Search(strategyManager, graph, type, new LinkGraphChainBuilder<T>(start),
                    new HashSet<T> { start }, new HashSet<T>(), new HashSet<T> {start}, processed)) return;
            processed.Add(start);
        }
    }

    private bool Search(IStrategyManager manager, ILinkGraph<T> graph, IAlternatingInferenceType<T> type,
        LinkGraphChainBuilder<T> builder, HashSet<T> current, HashSet<T> onExplored, HashSet<T> offExplored, HashSet<T> processed)
    {
        var next = builder.LastLink() == LinkStrength.Strong ? LinkStrength.Weak : LinkStrength.Strong;
        var explored = next == LinkStrength.Strong ? onExplored : offExplored;
        var last = builder.LastElement();

        foreach (var friend in graph.Neighbors(last, next))
        {
            if (current.Contains(friend) || explored.Contains(friend)) continue;

            builder.Add(next, friend);
            current.Add(friend);
            explored.Add(friend);
            
            if (builder.Count >= 3 && !processed.Contains(friend) && next == LinkStrength.Strong &&
                Check(manager, graph, type, builder.ToChain())) return true;
            if (Search(manager, graph, type, builder, current, onExplored, offExplored, processed)) return true;
            
            builder.RemoveLast();
            current.Remove(friend);
        }

        if (next == LinkStrength.Weak)
        {
            foreach (var friend in graph.Neighbors(last, LinkStrength.Strong))
            {
                if (current.Contains(friend)) continue;

                builder.Add(next, friend);
                current.Add(friend);
                offExplored.Add(friend);
                
                if (Search(manager, graph, type, builder, current, onExplored, offExplored, processed)) return true;
            
                builder.RemoveLast();
                current.Remove(friend);
            }
        }
        
        return false;
    }

    private bool Check(IStrategyManager manager, ILinkGraph<T> graph, IAlternatingInferenceType<T> type,
        LinkGraphChain<T> chain)
    {
        return type.ProcessChain(manager, chain, graph);
    }
}