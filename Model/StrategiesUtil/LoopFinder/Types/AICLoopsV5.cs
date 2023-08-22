using System.Collections.Generic;

namespace Model.StrategiesUtil.LoopFinder.Types;

public class AICLoopsV5<T> : ILoopType<T> where T : ILinkGraphElement
{
    public void Apply(LoopFinder<T> manager)
    {
        Dictionary<T, HashSet<T>> searched = new();
        foreach (var start in manager)
        {
            Search(manager, new LoopBuilder<T>(start), searched, new Dictionary<T, HashSet<T>>());
        }
    }

    private void Search(LoopFinder<T> manager, LoopBuilder<T> builder, Dictionary<T, HashSet<T>> searched,
        Dictionary<T, HashSet<T>> localSearched)
    {
        var last = builder.LastElement();
        var before = builder.ElementBefore();

        HashSet<T>? alreadySearched = searched.TryGetValue(last, out var a) ? a : null;
        HashSet<T>? localAlreadySearched = localSearched.TryGetValue(last, out var b) ? b : null;

        foreach (var friend in manager.GetLinks(last, LinkStrength.Strong))
        {
            if (builder.Count == 1 && alreadySearched is not null && alreadySearched.Contains(friend)) continue;
            if(localAlreadySearched is not null && localAlreadySearched.Contains(friend)) continue;
            if (before is not null && friend.Equals(before)) continue;
            var index = builder.IndexOf(friend);

            var linkStrength = builder.Count % 2 == 1 ? LinkStrength.Strong : LinkStrength.Weak;
            if (index == -1)
            {
                AddToSearched(searched, last, friend);
                Search(manager, builder.Add(friend, linkStrength), searched, localSearched);
            }
            else
            {
                var cut = builder.Cut(index);
                if(cut.FirstLink() == LinkStrength.Strong && cut.Count >= 4) manager.AddLoop(cut.End(LinkStrength.Strong));
                AddToSearched(localSearched, cut.FirstElement(), last);
            }
        }

        if (builder.Count % 2 == 1 && builder.Count < 4) return;
        foreach (var friend in manager.GetLinks(last, LinkStrength.Weak))
        {
            if(localAlreadySearched is not null && localAlreadySearched.Contains(friend)) continue;
            if (before is not null && friend.Equals(before)) continue;
            var index = builder.IndexOf(friend);

            if (index == -1)
            {
                if(builder.Count % 2 == 0)Search(manager, builder.Add(friend, LinkStrength.Weak), searched, localSearched);
            }
            else
            {
                var cut = builder.Cut(index);
                if(cut.FirstLink() == LinkStrength.Strong && cut.Count >= 4) manager.AddLoop(cut.End(LinkStrength.Weak));
                AddToSearched(localSearched, cut.FirstElement(), last);
            }
        }
    }
    
    private void AddToSearched(Dictionary<T, HashSet<T>> searched, T from, T to)
    {
        if (searched.TryGetValue(from, out var hashSet)) hashSet.Add(to);
        else searched.Add(from, new HashSet<T>() {to});
    }
}