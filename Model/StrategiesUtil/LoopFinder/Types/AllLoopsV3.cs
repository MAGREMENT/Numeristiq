using System;
using System.Collections.Generic;

namespace Model.StrategiesUtil.LoopFinder.Types;

public class AllLoopsV3<T> : ILoopType<T> where T : ILoopElement
{
    public void Apply(LoopFinder<T> manager)
    {
        HashSet<T> visited = new();
        foreach (var start in manager)
        {
            DepthFirstSearch(manager, new LoopBuilder<T>(start), visited);
        }
    }

    private void DepthFirstSearch(LoopFinder<T> manager, LoopBuilder<T> path, HashSet<T> visited)
    {
        var last = path.LastElement();
        if (visited.Contains(last)) return;

        foreach (var friend in manager.GetLinks(last, LinkStrength.Strong))
        {
            int index = path.IndexOf(friend);
            if(index == -1) DepthFirstSearch(manager, path.Add(friend, LinkStrength.Strong), visited);
            else if (path.Count - index > 2)
            {
                manager.AddLoop(path.Cut(index).End(LinkStrength.Strong));
            }
        }
        
        foreach (var friend in manager.GetLinks(last, LinkStrength.Weak))
        {
            int index = path.IndexOf(friend);
            if(index == -1) DepthFirstSearch(manager, path.Add(friend, LinkStrength.Weak), visited);
            else if (path.Count - index > 2)
            {
                manager.AddLoop(path.Cut(index).End(LinkStrength.Weak));
            }
        }

        visited.Add(last);
    }
}