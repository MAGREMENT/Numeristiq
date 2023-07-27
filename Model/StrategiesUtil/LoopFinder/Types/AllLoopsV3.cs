using System.Collections.Generic;

namespace Model.StrategiesUtil.LoopFinder.Types;

public class AllLoopsV3<T> : ILoopType<T> where T : ILoopElement
{
    public void Apply(LoopFinder<T> manager)
    {
        Dictionary<T, LoopBuilder<T>> paths = new();
        foreach (var start in manager)
        {
            Search(manager, new LoopBuilder<T>(start), paths);
        }
    }

    private void Search(LoopFinder<T> manager, LoopBuilder<T> currentPath, Dictionary<T, LoopBuilder<T>> paths)
    {
        var last = currentPath.LastElement();
        if (!paths.TryAdd(last, currentPath)) return;

        var before = currentPath.ElementBefore();
        foreach (var friend in manager.GetLinks(last, LinkStrength.Strong))
        {
            if(!paths.ContainsKey(friend)) Search(manager, currentPath.Add(friend, LinkStrength.Strong), paths);
            else if (!(before is not null && before.Equals(friend)))
            {
                
            }
        }
        
        foreach (var friend in manager.GetLinks(last, LinkStrength.Weak))
        {
            if(!paths.ContainsKey(friend)) Search(manager, currentPath.Add(friend, LinkStrength.Weak), paths);
        }
    }
}