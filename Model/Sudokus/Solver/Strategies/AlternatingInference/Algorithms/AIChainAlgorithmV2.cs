﻿using System.Collections.Generic;
using Model.Core.Graphs;
using Model.Sudokus.Solver.Utility.Graphs;

namespace Model.Sudokus.Solver.Strategies.AlternatingInference.Algorithms;

public class AIChainAlgorithmV2<T> : IAlternatingInferenceAlgorithm<T> where T : ISudokuElement
{
    public AlgorithmType Type => AlgorithmType.Chain;
    public void Run(ISudokuSolverData solverData, IAlternatingInferenceType<T> type)
    {
        var graph = type.GetGraph(solverData);
        HashSet<T> processed = new();

        foreach (var start in graph)
        {
            if (Search(solverData, graph, type, new ChainBuilder<T, LinkStrength>(start),
                    new HashSet<T> { start }, new HashSet<T>(), new HashSet<T> {start}, processed)) return;
            processed.Add(start);
        }
    }

    private bool Search(ISudokuSolverData user, IGraph<T, LinkStrength> graph, IAlternatingInferenceType<T> type,
        ChainBuilder<T, LinkStrength> builder, HashSet<T> current, HashSet<T> onExplored, HashSet<T> offExplored, HashSet<T> processed)
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
                Check(user, graph, type, builder.ToChain())) return true;
            if (Search(user, graph, type, builder, current, onExplored, offExplored, processed)) return true;
            
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
                
                if (Search(user, graph, type, builder, current, onExplored, offExplored, processed)) return true;
            
                builder.RemoveLast();
                current.Remove(friend);
            }
        }
        
        return false;
    }

    private bool Check(ISudokuSolverData user, IGraph<T, LinkStrength> graph, IAlternatingInferenceType<T> type,
        Chain<T, LinkStrength> chain)
    {
        return type.ProcessChain(user, chain, graph);
    }
}