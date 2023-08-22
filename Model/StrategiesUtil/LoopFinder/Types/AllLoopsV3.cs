using System;
using System.Collections.Generic;

namespace Model.StrategiesUtil.LoopFinder.Types;

public class AllLoopsV3<T> : ILoopType<T> where T : ILoopElement, ILinkGraphElement
{
    public void Apply(LoopFinder<T> manager)
    {
        List<Loop<T>> loops = new();
        FindCycleBasis(manager, loops);

        return;
        for (int i = 0; i < loops.Count; i++)
        {
            for (int j = i + 1; j < loops.Count; j++)
            {
                var loop = loops[i] ^ loops[j];
                if (loop.Count > 0 && manager.AddLoop(loop))
                {
                    var toAvoid = new HashSet<int>() {i,  j };
                    ContinueXor(manager, loop, loops, toAvoid);
                }
            }
        }
    }

    private void ContinueXor(LoopFinder<T> manager, Loop<T> current, List<Loop<T>> loops, HashSet<int> toAvoid)
    {
        for (int i = 0; i < loops.Count; i++)
        {
            if (toAvoid.Contains(i)) continue;
            var loop = current ^ loops[i];
            if (loop.Count > 0 && manager.AddLoop(loop))
            {
                toAvoid.Add(i);
                ContinueXor(manager, loop, loops, toAvoid);
            }
        }
    }

    private void FindCycleBasis(LoopFinder<T> manager, List<Loop<T>> loops)
    {
        Dictionary<T, int> visits = new();
        Dictionary<T, LinkToNode<T>> parents = new();
        HashSet<T> used = new();
        foreach (var start in manager)
        {
            if(visits.ContainsKey(start)) continue;
            
            visits[start] = 0;

            Queue<T> queue = new();
            queue.Enqueue(start);

            while (queue.Count > 0)
            {
                var current = queue.Dequeue();
                int nextLevel = visits[current] + 1;

                foreach (var friend in manager.GetLinks(current, LinkStrength.Strong))
                {
                    var visited = visits.TryGetValue(friend, out var n);

                    if (visited)
                    {
                        if (!used.Contains(friend))
                        {
                            List<T> path1 = new();
                            List<T> path2 = new();

                            int currentLevel = visits[current];
                            int friendLevel = visits[friend];

                            T[] array = { current, friend };
                            int highest = currentLevel > friendLevel ? 0 : 1;
                            int number = Math.Abs(currentLevel - friendLevel);
                            
                            for (int i = 0; i < number; i++)
                            {
                                path1.Add(array[highest]);
                                var ltn = parents[array[highest]];
                                array[highest] = ltn.Node;
                            }

                            int enzf = 0;

                            do
                            {
                                if (array[0].Equals(array[1]))
                                {
                                    path1.Add(array[0]);
                                    for (int i = path2.Count - 1; i >= 0; i--)
                                    {
                                        path1.Add(path2[i]);
                                    }
                                    break;
                                }

                                path1.Add(array[0]);
                                path2.Add(array[1]);

                                array[0] = parents[array[0]].Node;
                                array[1] = parents[array[1]].Node;
                            } while (true);

                            int b = 0;
                        }
                        
                        continue;
                    }
                    
                    visits.Add(friend, nextLevel);
                    parents.Add(friend, new LinkToNode<T>(LinkStrength.Strong, current));
                    queue.Enqueue(friend);
                }
                
                foreach (var friend in manager.GetLinks(current, LinkStrength.Weak))
                {
                    var visited = visits.TryGetValue(friend, out var n);

                    if (visited)
                    {
                        continue;
                    }
                    
                    visits.Add(friend, nextLevel);
                    parents.Add(friend, new LinkToNode<T>(LinkStrength.Weak, current));
                    queue.Enqueue(friend);
                }

                used.Add(current);
            }
        }
    }

    private void DepthFirstSearch(LoopFinder<T> manager, LoopBuilder<T> path, HashSet<T> visited, List<Loop<T>> loops) //TODO redo DFS
    {
        var last = path.LastElement();
        if (visited.Contains(last)) return;

        foreach (var friend in manager.GetLinks(last, LinkStrength.Strong))
        {
            int index = path.IndexOf(friend);
            if(index == -1) DepthFirstSearch(manager, path.Add(friend, LinkStrength.Strong), visited, loops);
            else if (path.Count - index > 2)
            {
                var loop = path.Cut(index).End(LinkStrength.Strong);
                manager.AddLoop(loop);
                loops.Add(loop);
            }
        }
        
        foreach (var friend in manager.GetLinks(last, LinkStrength.Weak))
        {
            int index = path.IndexOf(friend);
            if(index == -1) DepthFirstSearch(manager, path.Add(friend, LinkStrength.Weak), visited, loops);
            else if (path.Count - index > 2)
            {
                var loop = path.Cut(index).End(LinkStrength.Weak);
                manager.AddLoop(loop);
                loops.Add(loop);
            }
        }

        visited.Add(last);
    }

    

    private void DepthFirstSearch(LoopFinder<T> manager, T current, T parent, LinkStrength link, Dictionary<T, VisitStatus> status, Dictionary<T, LinkToNode<T>> parents,
        List<Loop<T>> loops)
    {
        bool visited = status.TryGetValue(current, out var s);
        if (visited && s == VisitStatus.Visited)
        {
            return;
        }

        if (visited && s == VisitStatus.Seen)
        {
            List<T> elements = new();
            List<LinkStrength> links = new();

            T start = parent;
            elements.Add(start);

            while (!start.Equals(current))
            {
                var ltn = parents[start];
                start = ltn.Node;
                elements.Add(ltn.Node);
                links.Add(ltn.Link);
            }

            links.Add(link);

            var loop = new Loop<T>(elements.ToArray(), links.ToArray());
            manager.AddLoop(loop);
            loops.Add(loop);
            
            return;
        }

        parents[current] = new LinkToNode<T>(link, parent);
        status[current] = VisitStatus.Seen;

        foreach (var friend in manager.GetLinks(current, LinkStrength.Strong))
        {
            if (friend.Equals(parent)) continue;
            DepthFirstSearch(manager, friend, current, LinkStrength.Strong, status, parents, loops);
        }
        
        foreach (var friend in manager.GetLinks(current, LinkStrength.Weak))
        {
            if (friend.Equals(parent)) continue;
            DepthFirstSearch(manager, friend, current, LinkStrength.Weak, status, parents, loops);
        }

        status[current] = VisitStatus.Visited;
    }

    private void PatonAlgorithm(LoopFinder<T> manager)
    {
        Dictionary<T, Dictionary<T, LinkToNode<T>>> used = new();
        Dictionary<T, LinkToNode<T>?> parent = new();
        Queue<T> stack = new();

        List<Loop<T>> loops = new();

        foreach (var start in manager)
        {
            if (parent.ContainsKey(start)) continue;
            
            used.Clear();

            parent.Add(start, null);
            used.Add(start, new Dictionary<T, LinkToNode<T>>());
            stack.Enqueue(start);

            while (stack.Count > 0)
            {
                var current = stack.Dequeue();
                var currentUsed = new Dictionary<T, LinkToNode<T>>();

                foreach (var friend in manager.GetLinks(current, LinkStrength.Strong))
                {
                    if (!used.ContainsKey(friend))
                    {
                        parent.Add(friend, new LinkToNode<T>(LinkStrength.Strong, current));
                        
                    }
                }
            }
        }

    }
    
}

public enum VisitStatus
{
    Seen, Visited
}

public class LinkToNode<T>
{
    public LinkToNode(LinkStrength link, T node)
    {
        Link = link;
        Node = node;
    }

    public LinkStrength Link { get; }
    public T Node { get; }

}