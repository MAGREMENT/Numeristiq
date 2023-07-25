using Model.LoopFinder;
using Model.Strategies.AIC;
using Model.StrategiesUtil.LoopFinder;

namespace Model.Strategies.AlternatingChains.ChainAlgorithms;

public class AlternatingChainAlgorithmV1<T> : IAlternatingChainAlgorithm<T> where T : notnull
{
    private readonly int _maxLoopSize;
    
    public AlternatingChainAlgorithmV1(int maxLoopSize)
    {
        _maxLoopSize = maxLoopSize;
    }

    public void Run(ISolverView view, Graph<T> graph, IAlternatingChainType<T> chainType)
    {
        foreach (var start in graph.EachVerticesWith(LinkStrength.Strong))
        {
            Search(graph, new LoopBuilder<T>(start), chainType, view);
        }
    }

    private void Search(Graph<T> graph, LoopBuilder<T> path, IAlternatingChainType<T> chainType, ISolverView view)
    {
        if (path.Count > _maxLoopSize) return;
        var last = path.LastElement();

        if (path.Count % 2 == 1)
        {
            foreach (var friend in graph.GetLinks(last, LinkStrength.Strong))
            {
                switch (path.Contains(friend))
                {
                    case ContainedStatus.First :
                        if (path.Count >= 4) chainType.ProcessStrongInference(view, path.FirstElement());
                        break;
                    case ContainedStatus.NotContained :
                        Search(graph, path.Add(friend, LinkStrength.Strong), chainType, view);
                        break;
                } 
            }
        }
        else
        {
            if (path.Count >= 4)
            {
                var weakFromFirst = graph.GetLinks(path.FirstElement(), LinkStrength.Weak);
                foreach (var weakFromLast in graph.GetLinks(last, LinkStrength.Weak))
                {
                    if (weakFromFirst.Contains(weakFromLast))
                    {
                        if(path.Contains(weakFromLast) == ContainedStatus.Contained) continue;
                        chainType.ProcessWeakInference(view, weakFromLast);
                    }
                }
            }
            
            foreach (var friend in graph.GetLinks(last, LinkStrength.Weak))
            {
                switch (path.Contains(friend))
                {
                    case ContainedStatus.First :
                        if (path.Count >= 4) chainType.ProcessFullLoop(view, path.End(LinkStrength.Weak));
                        break;
                    case ContainedStatus.NotContained :
                        Search(graph, path.Add(friend, LinkStrength.Weak), chainType, view);
                        break;
                } 
            }
            
            foreach (var friend in graph.GetLinks(last, LinkStrength.Strong))
            {
                switch (path.Contains(friend))
                {
                    case ContainedStatus.First :
                        if (path.Count >= 4) chainType.ProcessFullLoop(view, path.End(LinkStrength.Weak));
                        break;
                    case ContainedStatus.NotContained :
                        Search(graph, path.Add(friend, LinkStrength.Weak), chainType, view);
                        break;
                } 
            }
        }
    }
}