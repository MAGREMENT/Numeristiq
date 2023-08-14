using System.Collections.Generic;
using System.Linq;

namespace Model.StrategiesUtil.LoopFinder.Types;

public class AllLoopsV4<T> : ILoopType<T> where T : ILinkGraphElement
{
    public void Apply(LoopFinder<T> manager)
    {
        HashSet<T> searched = new();
        Search(manager, new LoopBuilder<T>(manager.First()), searched);
    }

    private void Search(LoopFinder<T> manager, LoopBuilder<T> builder, HashSet<T> searched)
    {
        var last = builder.LastElement();
        var before = builder.ElementBefore();

        foreach (var friend in manager.GetLinks(last, LinkStrength.Strong))
        {
            if (searched.Contains(friend)) return;
            if (before is not null && friend.Equals(before)) continue;
            var index = builder.IndexOf(friend);

            if (index == -1) Search(manager, builder.Add(friend, LinkStrength.Strong), searched);
            else manager.AddLoop(builder.Cut(index).End(LinkStrength.Strong));
        }
        
        foreach (var friend in manager.GetLinks(last, LinkStrength.Weak))
        {
            if (searched.Contains(friend)) return;
            if (before is not null && friend.Equals(before)) continue;
            var index = builder.IndexOf(friend);

            if (index == -1) Search(manager, builder.Add(friend, LinkStrength.Weak), searched);
            else manager.AddLoop(builder.Cut(index).End(LinkStrength.Weak));
        }

        searched.Add(last);
    }
}