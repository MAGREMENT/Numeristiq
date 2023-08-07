using System.Collections.Generic;

namespace Model.StrategiesUtil.LoopFinder.Types;

public class AICLoopsV4<T> : ILoopType<T> where T : ILoopElement, ILinkGraphElement //TODO debug
{
    public void Apply(LoopFinder<T> manager)
    {
        foreach (var start in manager)
        {
            StartSearch(manager, start);
        }
    }

    private void StartSearch(LoopFinder<T> manager, T start)
    {
        Queue<T> queue = new();
        Dictionary<T, NodeInfo<T>> infos = new();
        infos.Add(start, new NodeInfo<T>(LinkStrength.None, start, 0));

        foreach (var friend in manager.GetLinks(start, LinkStrength.Strong))
        {
            infos.Add(friend, new NodeInfo<T>(LinkStrength.Strong, start, 1));
            queue.Enqueue(friend);
        }

        foreach (var friend in manager.GetLinks(start, LinkStrength.Weak))
        {
            infos.Add(friend, new NodeInfo<T>(LinkStrength.Weak, start, 1));
            queue.Enqueue(friend);
        }

        while (queue.Count > 0)
        {
            T current = queue.Dequeue();
            var nextLevel = infos[current].Level + 1;
            var linkTo = infos[current].ParentLink == LinkStrength.Strong ? LinkStrength.Weak : LinkStrength.Strong;

            foreach (var friend in manager.GetLinks(current, linkTo))
            {
                if (friend.Equals(infos[current].Parent)) continue;
                if(infos.ContainsKey(friend)) Process(current, friend, linkTo, infos, manager);
                else
                {
                    infos.Add(friend, new NodeInfo<T>(linkTo, current, nextLevel));
                    queue.Enqueue(friend);
                }
            }
            
            if(linkTo == LinkStrength.Strong) continue;
            foreach (var friend in manager.GetLinks(current, LinkStrength.Strong))
            {
                if (friend.Equals(infos[current].Parent)) continue;
                if(infos.ContainsKey(friend)) Process(current, friend, linkTo, infos, manager);
                else
                {
                    infos.Add(friend, new NodeInfo<T>(linkTo, current, nextLevel));
                    queue.Enqueue(friend);
                }
            }
        }
    }

    private void Process(T current, T inCommon, LinkStrength lastLink, Dictionary<T, NodeInfo<T>> infos, LoopFinder<T> manager)
    {
        //Check if at start
        NodeInfo<T> inCommonInfo;
        if ((inCommonInfo = infos[inCommon]).Level == 0)
        {
            ProcessAtStart(current, lastLink, infos, manager);
            return;
        }

        NodeInfo<T> currentInfo;
        if ((currentInfo = infos[current]).Level == 0)
        {
            ProcessAtStart(inCommon, lastLink, infos, manager);
            return;
        }

        List<T> elCurrent = new();
        List<T> elInCommon = new();
        List<LinkStrength> liCurrent = new();
        List<LinkStrength> liInCommon = new();
        
        //Put to same level
        if (currentInfo.Level > inCommonInfo.Level)
        {
            int n = currentInfo.Level - inCommonInfo.Level;
            for (int i = 0; i < n; i++)
            {
                elCurrent.Add(current);
                liCurrent.Add(currentInfo.ParentLink);

                current = currentInfo.Parent;
                currentInfo = infos[current];
            }
        }
        else if (currentInfo.Level < inCommonInfo.Level)
        {
            int n = inCommonInfo.Level - currentInfo.Level;
            for (int i = 0; i < n; i++)
            {
                elInCommon.Add(inCommon);
                liInCommon.Add(inCommonInfo.ParentLink);

                inCommon = inCommonInfo.Parent;
                inCommonInfo = infos[inCommon];
            }
        }

        //Gard this cuz case where one of them is the start has been treated earlier
        if (current.Equals(inCommon)) return;
        
        //Go up tree
        while (!currentInfo.Parent.Equals(inCommonInfo.Parent))
        {
            elCurrent.Add(current);
            liCurrent.Add(currentInfo.ParentLink);

            current = currentInfo.Parent;
            currentInfo = infos[current];
            
            
            elInCommon.Add(inCommon);
            liInCommon.Add(inCommonInfo.ParentLink);

            inCommon = inCommonInfo.Parent;
            inCommonInfo = infos[inCommon];
        }
        elCurrent.Add(current);
        liCurrent.Add(currentInfo.ParentLink);

        elInCommon.Add(inCommon);
        liInCommon.Add(inCommonInfo.ParentLink);

        //Both parent is now the same => if not start node, ignore
        if (currentInfo.Level != 1) return;
        
        //If start links are different, return
        if (currentInfo.ParentLink == inCommonInfo.ParentLink) return;
        
        //Loop can be added
        elCurrent.Add(currentInfo.Parent);

        for (int i = elInCommon.Count - 1; i >= 0; i--)
        {
            elCurrent.Add(elInCommon[i]);
        }
        for (int i = liInCommon.Count - 1; i >= 0; i--)
        {
            liCurrent.Add(liInCommon[i]);
        }

        liCurrent.Add(lastLink);

        var loop = new Loop<T>(elCurrent.ToArray(), liCurrent.ToArray());
        if (loop.Count < 4) return;
        
        manager.AddLoop(loop);
    }

    private void ProcessAtStart(T deepest, LinkStrength lastLink, Dictionary<T, NodeInfo<T>> infos, LoopFinder<T> manager)
    {
        var currentInfo = infos[deepest];
        //If loop is long enough
        if (currentInfo.Level >= 3)
        {
            var elements = new T[currentInfo.Level + 1];
            var links = new LinkStrength[currentInfo.Level + 1];

            elements[0] = deepest;
            for (int i = 0; i < currentInfo.Level; i++)
            {
                links[i] = currentInfo.ParentLink;
                elements[i + 1] = currentInfo.Parent;

                currentInfo = infos[currentInfo.Parent];
            }

            if (links[^2] == lastLink) return;

            links[^1] = lastLink;
            manager.AddLoop(new Loop<T>(elements, links));
        }
    }
}

public class NodeInfo<T>
{
    public NodeInfo(LinkStrength parentLink, T parent, int level)
    {
        ParentLink = parentLink;
        Parent = parent;
        Level = level;
    }

    public LinkStrength ParentLink { get; }
    public T Parent { get; }
    public int Level { get; }

    public override string ToString()
    {
        if (ParentLink == LinkStrength.None) return "Start";
        return ParentLink == LinkStrength.Strong ? $"[{Level}] =" + Parent : $"[{Level}] -" + Parent;
    }
}