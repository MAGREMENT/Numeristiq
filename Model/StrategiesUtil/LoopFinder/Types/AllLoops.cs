using System.Collections.Generic;
using LoopFinder;
using LoopFinder.Strategies;
using Model.StrategiesUtil.LoopFinder;

namespace Model.LoopFinder.Types;

public class AllLoops<T> : ILoopType<T> where T : notnull
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
        foreach (var next in manager.GetLinks(current, LinkStrength.Strong))
        {
            if (visited.Contains(next) || (from is not null && next.Equals(from))) continue;
            switch (path.Contains(next))
            {
                case ContainedStatus.First : 
                    manager.AddLoop(path.End(LinkStrength.Strong));
                    break;
                case ContainedStatus.NotContained :
                    Search(manager, visited, next, current, path.Add(next, LinkStrength.Strong));
                    break;
            }
        }
        
        foreach (var next in manager.GetLinks(current, LinkStrength.Weak))
        {
            if (visited.Contains(next) || (from is not null && next.Equals(from))) continue;
            switch (path.Contains(next))
            {
                case ContainedStatus.First : 
                    manager.AddLoop(path.End(LinkStrength.Weak));
                    break;
                case ContainedStatus.NotContained :
                    Search(manager, visited, next, current, path.Add(next, LinkStrength.Weak));
                    break;
            }
        }
    }
}