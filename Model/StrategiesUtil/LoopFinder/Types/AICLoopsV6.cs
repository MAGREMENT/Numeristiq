using System.Collections.Generic;
using System.Runtime.Serialization.Formatters;

namespace Model.StrategiesUtil.LoopFinder.Types;

public class AICLoopsV6<T> : ILoopType<T> where T : ILinkGraphElement
{
    public void Apply(LoopFinder<T> manager)
    {
        Dictionary<T, HashSet<T>> searched = new();
        foreach (var start in manager)
        {
            Search(manager, new LoopBuilder<T>(start), false, searched);
        }
    }

    private void Search(LoopFinder<T> manager, LoopBuilder<T> path, bool hasInference, Dictionary<T, HashSet<T>> searched)
    {
        var last = path.LastElement();
        var before = path.ElementBefore();
        
        HashSet<T>? alreadySearched = searched.TryGetValue(last, out var a) ? a : null;

        foreach (var friend in manager.GetLinks(last, LinkStrength.Strong))
        {
            if (path.Count == 1 && alreadySearched is not null && alreadySearched.Contains(friend)) continue;
            if (before is not null && friend.Equals(before)) continue;

            int index = path.IndexOf(friend);

            LinkStrength opposite = path.Count == 1 ? LinkStrength.Strong :
                path.LastLink() == LinkStrength.Strong ? LinkStrength.Weak : LinkStrength.Strong;
            if (index == -1)
            {
                Search(manager, path.Add(friend, opposite), false, searched);
                AddToSearched(searched, last, friend);
            }
            else
            {
                var cut = path.Cut(index);
                if (cut.Count >= 4)
                {
                    manager.AddLoop(cut.End(opposite));
                }
            }
        }

        if (hasInference && path.LastLink() == LinkStrength.Weak) return;
        foreach (var friend in manager.GetLinks(last, LinkStrength.Weak))
        {
            if (path.Count == 1 && alreadySearched is not null && alreadySearched.Contains(friend)) continue;
            if (before is not null && friend.Equals(before)) continue;

            int index = path.IndexOf(friend);

            if (index == -1)
            {
                Search(manager, path.Add(friend, LinkStrength.Weak), path.LastLink() == LinkStrength.Weak, searched);
                AddToSearched(searched, last, friend);
            }
            else
            {
                var cut = path.Cut(index);
                hasInference = hasInference || cut.LastLink() == LinkStrength.Weak || cut.FirstLink() == LinkStrength.Weak;
                if (cut.Count >= 4 && (!hasInference || cut.FirstLink() == LinkStrength.Strong))
                {
                    manager.AddLoop(cut.End(LinkStrength.Weak));
                }
            }
        }
    }

    private void AddToSearched(Dictionary<T, HashSet<T>> searched, T from, T to)
    {
        if (searched.TryGetValue(from, out var hashSet)) hashSet.Add(to);
        else searched.Add(from, new HashSet<T>() {to});
    }
}