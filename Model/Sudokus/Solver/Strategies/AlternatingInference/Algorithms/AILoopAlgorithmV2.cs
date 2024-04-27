using System.Collections.Generic;
using Model.Sudokus.Solver.Utility.Graphs;

namespace Model.Sudokus.Solver.Strategies.AlternatingInference.Algorithms;

public class AILoopAlgorithmV2<T> : IAlternatingInferenceAlgorithm<T> where T : ISudokuElement
{
    private readonly int _maxLoopSize;
    private readonly HashSet<LinkGraphLoop<T>> _loopsProcessed = new();
    
    public AlgorithmType Type => AlgorithmType.Loop;

    public AILoopAlgorithmV2(int maxLoopSize)
    {
        _maxLoopSize = maxLoopSize;
    }

    public void Run(ISudokuStrategyUser strategyUser, IAlternatingInferenceType<T> type)
    {
        var graph = type.GetGraph(strategyUser);
        
        Dictionary<T, HashSet<T>> globallySearched = new();
        Dictionary<T, HashSet<T>> locallySearched = new();
        foreach (var start in graph)
        {
            if (Search(strategyUser, graph, type, new LinkGraphChainBuilder<T>(start), globallySearched, locallySearched)) return;
            locallySearched.Clear();
        }
    }

    private bool Search(ISudokuStrategyUser view, ILinkGraph<T> graph, IAlternatingInferenceType<T> inferenceType, LinkGraphChainBuilder<T> builder,
        Dictionary<T, HashSet<T>> globallySearched, Dictionary<T, HashSet<T>> locallySearched)
    {
        if (builder.Count > _maxLoopSize) return false;

        var last = builder.LastElement();
        var before = builder.BeforeLastElement();
        bool isPair = builder.Count % 2 == 0;
        HashSet<T>? globalFriends = globallySearched.TryGetValue(last, out var a) ? a : null;
        HashSet<T>? localFriends = locallySearched.TryGetValue(last, out var b) ? b : null;

        foreach (var friend in graph.Neighbors(last, LinkStrength.Strong))
        {
            if (builder.Count == 1 && globalFriends is not null && globalFriends.Contains(friend)) continue;
            if (localFriends is not null && localFriends.Contains(friend)) continue;
            if (before is not null && friend.Equals(before)) continue;

            var index = builder.IndexOf(friend);
            var linkStrength = !isPair ? LinkStrength.Strong : LinkStrength.Weak;
            
            if (index == -1)
            {
                builder.Add(linkStrength, friend);
                Search(view, graph, inferenceType, builder, globallySearched, locallySearched);
                builder.RemoveLast();

                if (globallySearched.TryGetValue(last, out var to)) to.Add(friend);
                else globallySearched.Add(last, new HashSet<T> {friend});
            }
            else
            {
                var cut = builder.Cut(index);
                if (cut.FirstLink() == LinkStrength.Strong && cut.Count >= 4)
                {
                    if (cut.Count % 2 == 0)
                    {
                        var loop = cut.ToLoop(LinkStrength.Weak);
                        if (_loopsProcessed.Contains(loop)) continue;

                        if (inferenceType.ProcessFullLoop(view, loop) &&
                            inferenceType.Strategy!.StopOnFirstPush) return true;
                        _loopsProcessed.Add(loop);
                    }
                    else
                    {
                        if (inferenceType.ProcessStrongInferenceLoop(view, cut.FirstElement(), cut.ToLoop(LinkStrength.Strong))
                            && inferenceType.Strategy!.StopOnFirstPush) return true;
                    }
                }
                AddToSearched(locallySearched, cut.FirstElement(), last);
            }
        }
        
        if (builder.Count % 2 == 1 && builder.Count < 4) return false;
        foreach (var friend in graph.Neighbors(last, LinkStrength.Weak))
        {
            if (localFriends is not null && localFriends.Contains(friend)) continue;
            if (before is not null && friend.Equals(before)) continue;
            
            var index = builder.IndexOf(friend);

            if (index == -1)
            {
                if(builder.Count % 2 == 0)
                {
                    builder.Add(LinkStrength.Weak, friend);
                    Search(view, graph, inferenceType, builder, globallySearched, locallySearched);
                    builder.RemoveLast();
                }
            }
            else
            {
                var cut = builder.Cut(index);
                if (cut.FirstLink() == LinkStrength.Strong && cut.Count >= 4)
                {
                    if (cut.Count % 2 == 0)
                    {
                        var loop = cut.ToLoop(LinkStrength.Weak);
                        if (_loopsProcessed.Contains(loop)) continue;

                        if (inferenceType.ProcessFullLoop(view, loop) &&
                            inferenceType.Strategy!.StopOnFirstPush) return true;
                        _loopsProcessed.Add(loop);
                    }
                    else
                    {
                        if (inferenceType.ProcessWeakInferenceLoop(view, cut.LastElement(), cut.ToLoop(LinkStrength.Weak))
                            && inferenceType.Strategy!.StopOnFirstPush) return true;
                    }
                }

                AddToSearched(locallySearched, cut.FirstElement(), last);
            }
        }

        return false;
    }
    
    private void AddToSearched(Dictionary<T, HashSet<T>> searched, T from, T to)
    {
        if (searched.TryGetValue(from, out var hashSet)) hashSet.Add(to);
        else searched.Add(from, new HashSet<T>() {to});
    }
}