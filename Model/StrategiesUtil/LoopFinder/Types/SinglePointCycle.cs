using System;
using System.Collections.Generic;
using System.IO;

namespace Model.StrategiesUtil.LoopFinder.Types;

public class SinglePointCycle<T> : ILoopType<T> where T : ILoopElement, ILinkGraphElement
{
    private readonly T _point;

    public SinglePointCycle(T point)
    {
        _point = point;
    }

    public void Apply(LoopFinder<T> manager)
    {
        foreach (var start in manager)
        {
            if (start.Equals(_point))
            {
                DepthFirstSearch(manager, new LoopBuilder<T>(start), new HashSet<T>());
                return;
            }
        }
    }

    private void DepthFirstSearch(LoopFinder<T> manager, LoopBuilder<T> path, HashSet<T> visited)
    {
        var last = path.LastElement();
        if (visited.Contains(last)) return;

        foreach (var friend in manager.GetLinks(last, LinkStrength.Strong))
        {
            switch (path.Contains(friend))
            { 
                case ContainedStatus.First : manager.AddLoop(path.End(LinkStrength.Strong));
                    break;
                case ContainedStatus.NotContained : DepthFirstSearch(manager, path.Add(friend, LinkStrength.Strong), visited);
                    break;
            }
        }
        
        foreach (var friend in manager.GetLinks(last, LinkStrength.Weak))
        {
            switch (path.Contains(friend))
            { 
                case ContainedStatus.First : manager.AddLoop(path.End(LinkStrength.Weak));
                    break;
                case ContainedStatus.NotContained : DepthFirstSearch(manager, path.Add(friend, LinkStrength.Weak), visited);
                    break;
            }
        }

        visited.Add(last);
    }
}