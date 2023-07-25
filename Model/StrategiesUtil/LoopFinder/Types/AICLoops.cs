using System.Collections.Generic;
using LoopFinder;
using LoopFinder.Strategies;
using Model.StrategiesUtil.LoopFinder;

namespace Model.LoopFinder.Types;

public class AICLoops<T> : ILoopType<T> where T : notnull
{
    private readonly bool _visitedExclusion = false;
    private readonly bool _oddLoopExclusion = false;
    private readonly bool _evenLoopExclusion = false;
    private readonly bool _loopFoundCancel = false;

    public AICLoops(params AICLoopSearchParam[] @params)
    {
        foreach (var p in @params)
        {
            switch (p)
            {
                case AICLoopSearchParam.OddLoopExclusion : _oddLoopExclusion = true;
                    break;
                case AICLoopSearchParam.VisitedExclusion : _visitedExclusion = true;
                    break;
                case AICLoopSearchParam.EvenLoopExclusion : _evenLoopExclusion = true;
                    break;
                case AICLoopSearchParam.LoopFoundCancel : _loopFoundCancel = true;
                    _evenLoopExclusion = true;
                    _oddLoopExclusion = true;
                    break;
            }
        }
    }

    public void Apply(LoopFinder<T> manager)
    {
        HashSet<T> visited = new();

        foreach (var start in manager)
        {
            if (Search(manager, visited, start, default(T), new LoopBuilder<T>(start))
                && _loopFoundCancel) return;
            if(_visitedExclusion) visited.Add(start);
        }
    }

    private bool Search(LoopFinder<T> manager, HashSet<T> visited, T current, T? from, LoopBuilder<T> path)
    {
        LinkStrength nextLink = path.LastLink() == LinkStrength.None
            ? LinkStrength.Strong
            : (path.LastLink() == LinkStrength.Strong ? LinkStrength.Weak : LinkStrength.Strong);
        foreach (var next in manager.GetLinks(current, LinkStrength.Strong))
        {
            if (visited.Contains(next) || (from is not null && next.Equals(from))) continue;
            switch (path.Contains(next))
            {
                case ContainedStatus.First :
                    if (path.Count % 2 == 0)
                    {
                        if (manager.AddLoop(path.End(nextLink)) && _evenLoopExclusion) return true;
                    }
                    else if(path.Count > 4 && path.FirstLink() == LinkStrength.Strong)
                    {
                        if (manager.AddLoop(path.End(nextLink)) && _oddLoopExclusion) return true;
                    }
                    break;
                case ContainedStatus.NotContained :
                    if (Search(manager, visited, next, current, path.Add(next, nextLink))) return true;
                    break;
            }
        }
        
        foreach (var next in manager.GetLinks(current, LinkStrength.Weak))
        {
            if (visited.Contains(next) || (from is not null && next.Equals(from))) continue;
            switch (path.Contains(next))
            {
                case ContainedStatus.First:
                    if (path.Count % 2 == 0)
                    {
                        if (path.FirstLink() != LinkStrength.Weak &&
                            path.LastLink() != LinkStrength.Weak)
                        {
                            if (manager.AddLoop(path.End(LinkStrength.Weak)) && _evenLoopExclusion) return true;
                        }
                    }
                    else if(path.Count > 4 && path.FirstLink() == LinkStrength.Weak)
                    {
                        if (manager.AddLoop(path.End(nextLink)) && _oddLoopExclusion) return true;
                    }
                    break;
                case ContainedStatus.NotContained :
                    if (path.LastLink() == LinkStrength.Weak) continue;
                    if (Search(manager, visited, next, current, path.Add(next, LinkStrength.Weak))) return true;
                    break;
            }
        }

        return false;
    }
}

public enum AICLoopSearchParam
{
    OddLoopExclusion, EvenLoopExclusion, LoopFoundCancel, VisitedExclusion
}