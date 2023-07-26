using System.Collections.Generic;

namespace Model.StrategiesUtil.LoopFinder.Types;

public class AllLoopsV2<T> : ILoopType<T> where T : ILoopElement
{
    public void Apply(LoopFinder<T> manager)
    {
        Dictionary<T, int> explored = new();
        foreach (var start in manager)
        {
            if (explored.TryGetValue(start, out var i)) continue;
            Search(new LoopBuilder<T>(start), manager, explored);
            return;
        }
    }

    private void Search(LoopBuilder<T> path, LoopFinder<T> manager, Dictionary<T, int> explored)
    {
        var last = path.LastElement();

        var nextLink = (path.LastLink() == LinkStrength.None || path.LastLink() == LinkStrength.Weak)
            ? LinkStrength.Strong
            : LinkStrength.Weak;
        foreach (var friend in manager.GetLinks(last, LinkStrength.Strong))
        {
            int index = path.IndexOf(friend);
            if(index == -1) Search(path.Add(friend, nextLink), manager, explored);
            else if (path.Count - index > 2)
            { 
                manager.AddLoop(path.Cut(index).End(nextLink));
            }
        }
        
        foreach (var friend in manager.GetLinks(last, LinkStrength.Weak))
        {
            int index = path.IndexOf(friend);
            if (index == -1)
            {
                Search(path.Add(friend, LinkStrength.Weak), manager, explored);
            }
            else if (path.Count - index > 4)
            { 
                manager.AddLoop(path.Cut(index).End(LinkStrength.Weak));
            }
        }

        explored[last] = 1;
    }
}