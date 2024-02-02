using System.Collections.Generic;
using Model.Sudoku.Solver.StrategiesUtility.Graphs;

namespace Model.Sudoku.Solver.Strategies.AlternatingInference.Algorithms;

public class AIChainAlgorithmV1<T> : IAlternatingInferenceAlgorithm<T> where T : ISudokuElement
{
    public AlgorithmType Type => AlgorithmType.Chain;
    public void Run(IStrategyUser strategyUser, IAlternatingInferenceType<T> type)
    {
        var graph = type.GetGraph(strategyUser);
        HashSet<T> processed = new();

        foreach (var start in graph)
        {
            if (Search(strategyUser, graph, type, new LinkGraphChainBuilder<T>(start),
                    new HashSet<T> { start }, processed)) return;
            processed.Add(start);
        }
    }

    private bool Search(IStrategyUser user, ILinkGraph<T> graph, IAlternatingInferenceType<T> type,
        LinkGraphChainBuilder<T> builder, HashSet<T> explored, HashSet<T> processed)
    {
        var next = builder.LastLink() == LinkStrength.Strong ? LinkStrength.Weak : LinkStrength.Strong;
        var last = builder.LastElement();

        foreach (var friend in graph.Neighbors(last, next))
        {
            if (explored.Contains(friend)) continue;

            builder.Add(next, friend);
            explored.Add(friend);
            
            if (builder.Count >= 3 && !processed.Contains(friend) && next == LinkStrength.Strong &&
                Check(user, graph, type, builder.ToChain())) return true;
            if (Search(user, graph, type, builder, explored, processed)) return true;
            
            builder.RemoveLast();
        }

        if (next == LinkStrength.Weak)
        {
            foreach (var friend in graph.Neighbors(last, LinkStrength.Strong))
            {
                if (explored.Contains(friend)) continue;

                builder.Add(next, friend);
                explored.Add(friend);
                
                if (Search(user, graph, type, builder, explored, processed)) return true;
            
                builder.RemoveLast();
            }
        }
        
        return false;
    }

    private bool Check(IStrategyUser user, ILinkGraph<T> graph, IAlternatingInferenceType<T> type,
        LinkGraphChain<T> chain)
    {
        return type.ProcessChain(user, chain, graph);
    }
}