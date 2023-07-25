using System.Collections.Generic;
using LoopFinder.Strategies;
using Model.StrategiesUtil.LoopFinder;

namespace Model.LoopFinder.Types;

public class AICLoopsV3<T> : ILoopType<T> where T : notnull
{
    public void Apply(LoopFinder<T> manager)
    {
        Dictionary<T, int> explored = new Dictionary<T, int>();
        foreach (var start in manager)
        {
            if (manager.GetLinks(start, LinkStrength.Strong).Count == 0) continue;
            if (explored.TryGetValue(start, out var i) && i == 1) continue;
            Search(new LoopBuilder<T>(start), manager, explored);
        }
    }

    private void Search(LoopBuilder<T> path, LoopFinder<T> manager, Dictionary<T, int> explored)
    {
        var last = path.LastElement();

        LinkStrength nextLink = path.Count % 2 == 1 ? LinkStrength.Strong : LinkStrength.Weak;
        foreach (var friend in manager.GetLinks(last, LinkStrength.Strong))
        {
            int index = path.IndexOf(friend);
            if(index == -1) Search(path.Add(friend, nextLink), manager, explored);
            else if (path.Count - index >= 4)
            { 
                manager.AddLoop(path.Cut(index).End(nextLink));
            }
        }

        var weakLinks = manager.GetLinks(last, LinkStrength.Weak);
        if (weakLinks.Count == 0)
        {
            explored[last] = 1;
            return;
        }
        foreach (var friend in weakLinks)
        {
            int index = path.IndexOf(friend);
            if (index == -1)
            {
                if(path.Count % 2 == 0) Search(path.Add(friend, LinkStrength.Weak), manager, explored);
            }
            else if (path.Count - index >= 4)
            { 
                if(index % 2 == 0)manager.AddLoop(path.Cut(index).End(LinkStrength.Weak));
            }
        }
    }
}