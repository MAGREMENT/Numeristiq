using System.Linq;

namespace Model.StrategiesUtil.LoopFinder.Types;

public class AllLoopsV4<T> : ILoopType<T> where T : ILinkGraphElement
{
    public void Apply(LoopFinder<T> manager)
    {
        Search(manager, new LoopBuilder<T>(manager.First()));
    }

    private void Search(LoopFinder<T> manager, LoopBuilder<T> builder)
    {
        var last = builder.LastElement();
        var before = builder.ElementBefore();

        foreach (var friend in manager.GetLinks(last, LinkStrength.Strong))
        {
            if (before is not null && friend.Equals(before)) continue;
            var index = builder.IndexOf(friend);

            if (index == -1) Search(manager, builder.Add(friend, LinkStrength.Strong));
            else manager.AddLoop(builder.Cut(index).End(LinkStrength.Strong));
        }
        
        foreach (var friend in manager.GetLinks(last, LinkStrength.Weak))
        {
            if (before is not null && friend.Equals(before)) continue;
            var index = builder.IndexOf(friend);

            if (index == -1) Search(manager, builder.Add(friend, LinkStrength.Weak));
            else manager.AddLoop(builder.Cut(index).End(LinkStrength.Weak));
        }
    }
}