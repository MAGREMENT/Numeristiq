using System.Collections.Generic;

namespace Model.StrategiesUtil.LoopFinder.Types;

public class AICLoopsV5<T> : ILoopType<T> where T : ILinkGraphElement
{
    public void Apply(LoopFinder<T> manager)
    {
        Dictionary<T, HashSet<T>> searched = new();
        foreach (var start in manager)
        {
            Search(manager, new LoopBuilder<T>(start), searched);
        }
    }

    private void Search(LoopFinder<T> manager, LoopBuilder<T> builder, Dictionary<T, HashSet<T>> searched)
    {
        var last = builder.LastElement();
        var before = builder.ElementBefore();

        HashSet<T>? alreadySearched = searched.TryGetValue(last, out var a) ? a : null;

        foreach (var friend in manager.GetLinks(last, LinkStrength.Strong))
        {
            if (builder.Count == 1 && alreadySearched is not null && alreadySearched.Contains(friend)) continue;
            if (before is not null && friend.Equals(before)) continue;
            var index = builder.IndexOf(friend);

            var linkStrength = builder.Count % 2 == 1 ? LinkStrength.Strong : LinkStrength.Weak;
            if (index == -1)
            {
                Search(manager, builder.Add(friend, linkStrength), searched);

                if (searched.TryGetValue(last, out var to)) to.Add(friend);
                else searched.Add(last, new HashSet<T>() {friend});
            }
            else
            {
                var cut = builder.Cut(index);
                if(cut.FirstLink() == LinkStrength.Strong && cut.Count >= 4) manager.AddLoop(cut.End(LinkStrength.Strong));
            }
        }

        if (builder.Count % 2 == 1 && builder.Count < 4) return;
        foreach (var friend in manager.GetLinks(last, LinkStrength.Weak))
        {
            if (before is not null && friend.Equals(before)) continue;
            var index = builder.IndexOf(friend);

            if (index == -1)
            {
                if(builder.Count % 2 == 0)Search(manager, builder.Add(friend, LinkStrength.Weak), searched);
            }
            else
            {
                var cut = builder.Cut(index);
                if(cut.FirstLink() == LinkStrength.Strong && cut.Count >= 4) manager.AddLoop(cut.End(LinkStrength.Weak));
            }
        }
    }
}