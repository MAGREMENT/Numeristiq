using System;
using System.Collections.Generic;
using Model.Solver.StrategiesUtil.LinkGraph;

namespace Model.Solver.Strategies.AlternatingChains.ChainAlgorithms;

public class AlternatingChainAlgorithmV3<T> : IAlternatingChainAlgorithm<T> where T : ILinkGraphElement
{
    public void Run(IStrategyManager view, LinkGraph<T> graph, IAlternatingChainType<T> chainType)
    {
        foreach (var start in graph)
        {
            Dictionary<T, ElementInfo> tree = new();

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

                foreach (var friend in graph.GetLinks(current, LinkStrength.Strong))
                {
                    if (tree.TryGetValue(friend, out var ei))
                    {
                        for (int i = 0; i < ei.ParentCount; i++)
                        {
                            if (ei.ParentLinkAt(i) == LinkStrength.Weak)
                            {
                                //Nice loop
                                foreach (var loop in MessyBacktrack(tree, friend,
                                             current, LinkStrength.Strong))
                                {
                                    chainType.ProcessFullLoop(view, loop);
                                }
                            }
                            else
                            {
                                //Strong inference
                                chainType.ProcessStrongInference(view, friend, StraightForwardBacktrack(tree,
                                    friend, current, LinkStrength.Strong));
                            } 
                        }
                        
                        ei.AddParent(current, LinkStrength.Strong);
                    }
                    else
                    {
                        AddNewNodeToTree(tree, queue, friend, current, LinkStrength.Strong);
                    }
                }
                
                foreach (var friend in graph.GetLinks(current, LinkStrength.Weak))
                {
                    if (tree.TryGetValue(friend, out var ei))
                    {
                        for (int i = 0; i < ei.ParentCount; i++)
                        {
                            if (ei.ParentLinkAt(i) == LinkStrength.Strong)
                            {
                                //Nice loop
                                foreach (var loop in MessyBacktrack(tree, friend,
                                             current, LinkStrength.Weak))
                                {
                                    chainType.ProcessFullLoop(view, loop);
                                }
                            }
                            else
                            {
                                //Weak inference
                                chainType.ProcessStrongInference(view, friend, StraightForwardBacktrack(tree,
                                    friend, current, LinkStrength.Weak));
                            } 
                        }
                        
                        ei.AddParent(current, LinkStrength.Weak);
                    }
                    else
                    {
                        AddNewNodeToTree(tree, queue, friend, current, LinkStrength.Weak);
                    }
                }
            }
        }
    }

    private static void AddNewNodeToTree(Dictionary<T, ElementInfo> tree, Queue<T> queue,
        T toAdd, T parent, LinkStrength parentLink)
    {
        var ei = new ElementInfo();
        ei.AddParent(parent, parentLink);
                
        tree[toAdd] = ei;
        queue.Enqueue(toAdd);
    }

    private Loop<T> StraightForwardBacktrack(Dictionary<T, ElementInfo> tree, T startPoint1, T startPoint2,
        LinkStrength linkBetween)
    {
        return new Loop<T>(Array.Empty<T>(), Array.Empty<LinkStrength>()); //TODO
    }

    private List<Loop<T>> MessyBacktrack(Dictionary<T, ElementInfo> tree, T startPoint1, T startPoint2,
        LinkStrength linkBetween)
    {
        return new List<Loop<T>>(); //TODO
    }
}

public class ElementInfo
{
    private readonly List<ILinkGraphElement> _parents = new();
    private readonly List<LinkStrength> _parentLinks = new();
    
    public int ParentCount => _parents.Count;

    public void AddParent(ILinkGraphElement element, LinkStrength link)
    {
        _parents.Add(element);
        _parentLinks.Add(link);
    }

    public ILinkGraphElement ParentAt(int index)
    {
        return _parents[index];
    }

    public LinkStrength ParentLinkAt(int index)
    {
        return _parentLinks[index];
    }
}