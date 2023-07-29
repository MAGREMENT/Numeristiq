using Model.StrategiesUtil;
using Model.StrategiesUtil.LoopFinder;

namespace Model.Strategies.AlternatingChains.ChainAlgorithms;

public class AlternatingChainAlgorithmV1<T> : IAlternatingChainAlgorithm<T> where T : ILoopElement, ILinkGraphElement
{
    private readonly int _maxLoopSize;
    
    public AlternatingChainAlgorithmV1(int maxLoopSize)
    {
        _maxLoopSize = maxLoopSize;
    }

    public void Run(ISolverView view, LinkGraph<T> graph, IAlternatingChainType<T> chainType)
    {
        foreach (var start in graph.EachVerticesWith(LinkStrength.Strong))
        {
            Search(graph, new LoopBuilder<T>(start), chainType, view);
        }
    }

    private void Search(LinkGraph<T> graph, LoopBuilder<T> path, IAlternatingChainType<T> chainType, ISolverView view)
    {
        if (path.Count > _maxLoopSize) return;
        var last = path.LastElement();

        if (path.Count % 2 == 1)
        {
            foreach (var friend in graph.GetLinks(last, LinkStrength.Strong))
            {
                if (path.FirstElement().Equals(friend))
                {
                    if (path.Count >= 4) chainType.ProcessStrongInference(view, path.FirstElement());
                }
                else if(!path.IsAlreadyPresent(friend)) Search(graph, path.Add(friend, LinkStrength.Strong), chainType, view);
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
                        if(path.IsAlreadyPresent(weakFromLast)) continue;
                        chainType.ProcessWeakInference(view, weakFromLast);
                    }
                }
            }
            
            foreach (var friend in graph.GetLinks(last, LinkStrength.Weak))
            {
                if (path.FirstElement().Equals(friend))
                {
                    if (path.Count >= 4) chainType.ProcessFullLoop(view, path.End(LinkStrength.Weak));       
                }
                else if (!path.IsAlreadyPresent(friend)) Search(graph, path.Add(friend, LinkStrength.Weak), chainType, view);
            }
            
            foreach (var friend in graph.GetLinks(last, LinkStrength.Strong))
            {
                if (path.FirstElement().Equals(friend))
                {
                    if (path.Count >= 4) chainType.ProcessFullLoop(view, path.End(LinkStrength.Weak));       
                }
                else if (!path.IsAlreadyPresent(friend)) Search(graph, path.Add(friend, LinkStrength.Weak), chainType, view);
            }
        }
    }
}