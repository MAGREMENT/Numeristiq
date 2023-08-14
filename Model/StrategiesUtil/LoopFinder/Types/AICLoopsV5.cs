using System.Collections.Generic;
using System.Linq;

namespace Model.StrategiesUtil.LoopFinder.Types;

public class AICLoopsV5<T> : ILoopType<T> where T : ILinkGraphElement //TODO look into
{
    public void Apply(LoopFinder<T> manager)
    {
        HashSet<T> searched = new();
        foreach (var start in manager)
        {
            if (searched.Contains(start)) return;
            Search(manager, new LoopBuilder<T>(start), searched);
        }
    }

    private void Search(LoopFinder<T> manager, LoopBuilder<T> builder, HashSet<T> searched)
    {
        var last = builder.LastElement();
        var before = builder.ElementBefore();

        foreach (var friend in manager.GetLinks(last, LinkStrength.Strong))
        {
            if (before is not null && friend.Equals(before)) continue;
            var index = builder.IndexOf(friend);

            var linkStrength = builder.Count % 2 == 1 ? LinkStrength.Strong : LinkStrength.Weak;
            if (index == -1)
            {
                Search(manager, builder.Add(friend, linkStrength), searched);
            }
            else
            {
                var cut = builder.Cut(index);
                if(cut.FirstLink() == LinkStrength.Strong && cut.Count >= 4) manager.AddLoop(cut.End(LinkStrength.Strong));
            }
        }
        
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

        if (builder.Count % 2 == 1) searched.Add(last);
    }
}