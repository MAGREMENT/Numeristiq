using System.Collections.Generic;
using LoopFinder;
using LoopFinder.Strategies;

namespace Model.LoopFinder.Types;

public class AlternatingLoops<T> : ILoopType<T> where T : notnull
{
    public void Apply(LoopFinder<T> manager)
    {
        HashSet<T> visited = new();

        foreach (var start in manager)
        {
            Search(manager, visited, start, default(T), new LoopBuilder<T>(start));
            visited.Add(start);
        }
    }

    private void Search(LoopFinder<T> manager, HashSet<T> visited, T current, T? from, LoopBuilder<T> path)
    {
        LinkStrength buffer = path.LastLink() == LinkStrength.None
            ? LinkStrength.None
            : (path.LastLink() == LinkStrength.Strong ? LinkStrength.Weak : LinkStrength.Strong);
        LinkStrength nextLink = buffer == LinkStrength.None ? LinkStrength.Strong : buffer;
        foreach (var next in manager.GetLinks(current, LinkStrength.Strong))
        {
            if (visited.Contains(next) || (from is not null && next.Equals(from))) continue;
            switch (path.Contains(next))
            {
                case ContainedStatus.First : 
                    if(path.Count % 2 == 0) manager.AddLoop(path.End(nextLink));
                    break;
                case ContainedStatus.NotContained :
                    Search(manager, visited, next, current, path.Add(next, nextLink));
                    break;
            }
        }

        if (path.LastLink() == LinkStrength.Weak) return;
        foreach (var next in manager.GetLinks(current, LinkStrength.Weak))
        {
            if (visited.Contains(next) || (from is not null && next.Equals(from))) continue;
            switch (path.Contains(next))
            {
                case ContainedStatus.First : 
                    if(path.Count % 2 == 0 &&
                       path.FirstLink() != LinkStrength.Weak) manager.AddLoop(path.End(LinkStrength.Weak));
                    break;
                case ContainedStatus.NotContained :
                    Search(manager, visited, next, current, path.Add(next, LinkStrength.Weak));
                    break;
            }
        }
    }
}