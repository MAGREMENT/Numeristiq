using System.Collections.Generic;
using Model.StrategiesUtil.LoopFinder;

namespace Model.Strategies.AlternatingChains.ChainAlgorithms;

public class AlternatingChainAlgorithmV2<T> : IAlternatingChainAlgorithm<T> where T : ILoopElement
{
    private readonly int _maxLoopSize;

    public AlternatingChainAlgorithmV2(int maxLoopSize)
    {
        _maxLoopSize = maxLoopSize;
    }

    public void Run(ISolverView view, Graph<T> graph, IAlternatingChainType<T> chainType)
    {
        HashSet<T> explored = new();
        HashSet<Loop<T>> loopsProcessed = new();
        foreach (var start in graph.EachVerticesWith(LinkStrength.Strong))
        {
            if (explored.Contains(start)) continue;
            Search(view, new LoopBuilder<T>(start), graph, chainType, explored, loopsProcessed);
        }
    }

    private void Search(ISolverView view, LoopBuilder<T> path, Graph<T> graph, IAlternatingChainType<T> chainType,
        HashSet<T> explored, HashSet<Loop<T>> loopsProcessed)
    {
        if (path.Count > _maxLoopSize) return;
        var last = path.LastElement();
        
        LinkStrength nextLink = path.Count % 2 == 1 ? LinkStrength.Strong : LinkStrength.Weak;
        foreach (var friend in graph.GetLinks(last, LinkStrength.Strong))
        {
            int index = path.IndexOf(friend);
            if (index == -1) Search(view, path.Add(friend, nextLink), graph, chainType, explored, loopsProcessed);
            else if (path.Count - index >= 4)
            {
                var cut = path.Cut(index);
                if (cut.Count % 2 == 0)
                {
                    Loop<T> final = cut.End(nextLink);
                    if (!loopsProcessed.Contains(final))
                    {
                        chainType.ProcessFullLoop(view, final);
                        loopsProcessed.Add(final);
                    }
                }
                else
                {
                    if (cut.FirstLink() == LinkStrength.Weak) chainType.ProcessWeakInference(view, cut.FirstElement());
                    else chainType.ProcessStrongInference(view, cut.FirstElement());
                }
            }
        }

        bool a = true;
        foreach (var friend in graph.GetLinks(last, LinkStrength.Weak))
        {
            int index = path.IndexOf(friend);
            if (index == -1)
            {
                if(path.Count % 2 == 0) Search(view, path.Add(friend, LinkStrength.Weak), graph, chainType, explored, loopsProcessed);
                if (a && graph.GetLinks(friend, LinkStrength.Weak).Count > 0) a = false;
            }
            else if(path.Count - index >= 4)
            {
                var cut = path.Cut(index);
                if (cut.Count % 2 == 0 && cut.FirstLink() == LinkStrength.Strong)
                {
                    var final = cut.End(LinkStrength.Weak);
                    if (!loopsProcessed.Contains(final))
                    {
                        chainType.ProcessFullLoop(view, final);
                        loopsProcessed.Add(final);
                    }
                }
                else if (cut.Count % 2 == 1 && cut.FirstLink() == LinkStrength.Weak)
                {
                    chainType.ProcessWeakInference(view, cut.FirstElement());
                }
            }
        }

        if (a) explored.Add(last);
    }
}