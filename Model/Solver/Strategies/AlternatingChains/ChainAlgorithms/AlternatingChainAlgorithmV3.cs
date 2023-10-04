using System.Collections.Generic;
using Model.Solver.StrategiesUtil.LinkGraph;

namespace Model.Solver.Strategies.AlternatingChains.ChainAlgorithms;

public class AlternatingChainAlgorithmV3<T> : IAlternatingChainAlgorithm<T> where T : ILinkGraphElement
{
    public void Run(IStrategyManager view, LinkGraph<T> graph, IAlternatingChainType<T> chainType)
    {
        foreach (var start in graph)
        {
            Dictionary<T, ElementInfo<T>> tree = new();
            tree.Add(start, new ElementInfo<T>());

            Queue<T> queue = new();

            foreach (var friend in graph.GetLinks(start, LinkStrength.Strong))
            {
                AddNewNodeToTree(tree, queue, friend, start, LinkStrength.Strong);
            }
            
            foreach (var friend in graph.GetLinks(start, LinkStrength.Weak))
            {
                AddNewNodeToTree(tree, queue, friend, start, LinkStrength.Weak);
            }

            while (queue.Count > 0)
            {
                var current = queue.Dequeue();

                var lastLink = tree[current].ParentLinkAt(0);
                var opposite = lastLink == LinkStrength.Strong ? LinkStrength.Weak : LinkStrength.Strong;
                
                foreach (var friend in graph.GetLinks(current, opposite))
                {
                    if (tree.TryGetValue(friend, out var ei))
                    {
                        for (int i = 0; i < ei.ParentCount; i++)
                        {
                            if (ei.ParentLinkAt(i) == lastLink)
                            {
                                //Nice loop
                                foreach (var loop in MessyBacktrack(tree, friend, lastLink,
                                             current, opposite))
                                {
                                    chainType.ProcessFullLoop(view, loop);
                                }
                            }
                            else
                            {
                                //Strong inference
                                if(opposite == LinkStrength.Strong) chainType.ProcessStrongInference(view, friend,
                                    StraightForwardBacktrack(tree, friend, opposite, current, opposite));
                                //Weak inference
                                else chainType.ProcessWeakInference(view, friend, StraightForwardBacktrack(tree,
                                    friend, opposite, current, opposite));
                               
                            } 
                        }
                        
                        ei.AddParent(current, opposite);
                    }
                    else
                    {
                        AddNewNodeToTree(tree, queue, friend, current, LinkStrength.Strong);
                    }
                }
            }
        }
    }

    private static void AddNewNodeToTree(Dictionary<T, ElementInfo<T>> tree, Queue<T> queue,
        T toAdd, T parent, LinkStrength parentLink)
    {
        var ei = new ElementInfo<T>();
        ei.AddParent(parent, parentLink);
                
        tree[toAdd] = ei;
        queue.Enqueue(toAdd);
    }

    private Loop<T> StraightForwardBacktrack(Dictionary<T, ElementInfo<T>> tree, T startPoint1, LinkStrength assignedLink,
        T startPoint2, LinkStrength linkBetween)
    {
        List<T> elements = new() {startPoint1};
        List<LinkStrength> links = new();

        ElementInfo<T> ei;
        var nextLink = assignedLink;

        while ((ei = tree[elements[^1]]).ParentCount > 0)
        {
            for (int i = 0; i < ei.ParentCount; i++)
            {
                if (ei.ParentLinkAt(i) == nextLink)
                {
                    elements.Add(ei.ParentAt(i));
                    links.Add(ei.ParentLinkAt(i));
                    nextLink = nextLink == LinkStrength.Strong ? LinkStrength.Weak : LinkStrength.Strong;
                    break;
                }
            }
        }

        nextLink = linkBetween == LinkStrength.Strong ? LinkStrength.Weak : LinkStrength.Strong;

        List<T> elementsToReverse = new() { startPoint2 };
        List<LinkStrength> linksToReverse = new();
        
        while ((ei = tree[elementsToReverse[^1]]).ParentCount > 0)
        {
            for (int i = 0; i < ei.ParentCount; i++)
            {
                if (ei.ParentLinkAt(i) == nextLink)
                {
                    elementsToReverse.Add(ei.ParentAt(i));
                    linksToReverse.Add(ei.ParentLinkAt(i));
                    nextLink = nextLink == LinkStrength.Strong ? LinkStrength.Weak : LinkStrength.Strong;
                    break;
                }
            }
        }

        for (int i = elementsToReverse.Count - 1; i > 0; i--)
        {
            elements.Add(elementsToReverse[i]);
            links.Add(linksToReverse[i - 1]);
        }

        links.Add(linkBetween);

        return new Loop<T>(elements.ToArray(), links.ToArray());
    }

    private List<Loop<T>> MessyBacktrack(Dictionary<T, ElementInfo<T>> tree, T startPoint1, LinkStrength assignedLink,
        T startPoint2, LinkStrength linkBetween)
    {
        return new List<Loop<T>>(); //TODO
    }
}

public class ElementInfo<T>
{
    private readonly List<T> _parents = new();
    private readonly List<LinkStrength> _parentLinks = new();
    
    public int ParentCount => _parents.Count;

    public void AddParent(T element, LinkStrength link)
    {
        _parents.Add(element);
        _parentLinks.Add(link);
    }

    public T ParentAt(int index)
    {
        return _parents[index];
    }

    public LinkStrength ParentLinkAt(int index)
    {
        return _parentLinks[index];
    }
}