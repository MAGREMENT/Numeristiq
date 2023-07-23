using System.Collections.Generic;
using System.Linq;
using LoopFinder.Strategies;

namespace Model.LoopFinder.Types;

public class AllLoopsV2<T> : ILoopType<T> where T : notnull
{
    public void Apply(LoopFinder<T> manager)
    {
        Dictionary<T, LoopBuilder<T>> paths = new();

        var start = manager.First();
        paths[start] = new LoopBuilder<T>(start);
    }

    private void Search(LoopFinder<T> manager, T current,  Dictionary<T, LoopBuilder<T>> paths)
    {
        foreach (var next in manager.GetLinks(current, LinkStrength.Strong))
        {
            if (!paths.TryGetValue(next, out var loop))
            {
                paths[next] = paths[current].Add(next, LinkStrength.Strong);
            }
            else
            {
                
            }
        }
    }
}