using System.Collections.Generic;
using System.Linq;

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
        Dictionary<T, NodeInfo<T>> from = new();
        from.Add(start, new NodeInfo<T>(LinkStrength.None, start, 0));

        foreach (var friend in manager.GetLinks(start, LinkStrength.Strong))
        {
            from.Add(friend, new NodeInfo<T>(LinkStrength.Strong, start, 1));
            queue.Enqueue(friend);
        }

        foreach (var friend in manager.GetLinks(start, LinkStrength.Weak))
        {
            from.Add(friend, new NodeInfo<T>(LinkStrength.Weak, start, 1));
            queue.Enqueue(friend);
        }

        while (queue.Count > 0)
        {
            T current = queue.Dequeue();
            var nextLevel = from[current].Level + 1;
            var linkTo = from[current].ParentLink == LinkStrength.Strong ? LinkStrength.Weak : LinkStrength.Strong;

            foreach (var friend in manager.GetLinks(current, linkTo))
            {
                if (friend.Equals(from[current].Parent)) continue;
                if(from.ContainsKey(friend)) Process(current, friend, linkTo, from, manager);
                else
                {
                    from.Add(friend, new NodeInfo<T>(linkTo, current, nextLevel));
                    queue.Enqueue(friend);
                }
            }
            
            if(linkTo == LinkStrength.Strong) continue;
            foreach (var friend in manager.GetLinks(current, LinkStrength.Strong))
            {
                if (friend.Equals(from[current].Parent)) continue;
                if(from.ContainsKey(friend)) Process(current, friend, linkTo, from, manager);
                else
                {
                    from.Add(friend, new NodeInfo<T>(linkTo, current, nextLevel));
                    queue.Enqueue(friend);
                }
            }
        }
    }

    private void Process(T current, T inCommon, LinkStrength lastLink, Dictionary<T, NodeInfo<T>> from, LoopFinder<T> manager)
    {
        T one = inCommon;
        T two = current;

        List<T> oneElements = new() {inCommon};
        List<T> twoElements = new() {current};
        List<LinkStrength> oneLinks = new();
        List<LinkStrength> twoLinks = new();

        var infoOne = from[inCommon];
        var infoTwo = from[current];

        if (infoOne.Level > infoTwo.Level)
        {
            int n = infoOne.Level - infoTwo.Level;
            while (n-- > 0)
            {
                oneElements.Add(infoOne.Parent);
                oneLinks.Add(infoOne.ParentLink);
                one = infoOne.Parent;
                infoOne = from[one];
            }
        }
        else if (infoOne.Level < infoTwo.Level)
        {
            int n = infoTwo.Level - infoOne.Level;
            while (n-- > 0)
            {
                twoElements.Add(infoTwo.Parent);
                twoLinks.Add(infoTwo.ParentLink);
                two = infoTwo.Parent;
                infoTwo = from[two];
            }
        }

        //if (one.Equals(two)) return;

        while (!infoOne.Parent.Equals(infoTwo.Parent))
        {
            oneElements.Add(infoOne.Parent);
            oneLinks.Add(infoOne.ParentLink);
            one = infoOne.Parent;
            infoOne = from[one];
            
            twoElements.Add(infoTwo.Parent);
            twoLinks.Add(infoTwo.ParentLink);
            two = infoTwo.Parent;
            infoTwo = from[two];
        }

        if (from[infoOne.Parent].Level != 0 || infoOne.ParentLink == infoTwo.ParentLink) return;

        oneElements.Add(infoOne.Parent);
        for (int i = twoElements.Count - 1; i >= 0; i--)
        {
            oneElements.Add(twoElements[i]);
        }

        oneLinks.Add(infoOne.ParentLink);
        oneLinks.Add(infoTwo.ParentLink);
        for (int i = twoLinks.Count - 1; i >= 0; i--)
        {
            oneLinks.Add(twoLinks[i]);
        }
        oneLinks.Add(lastLink);

        Loop<T> loop = new Loop<T>(oneElements.ToArray(), oneLinks.ToArray());
        if (loop.Count < 4) return;

        manager.AddLoop(loop);
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