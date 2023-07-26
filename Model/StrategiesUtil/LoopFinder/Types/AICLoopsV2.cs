namespace Model.StrategiesUtil.LoopFinder.Types;

public class AICLoopsV2<T> : ILoopType<T> where T : ILoopElement
{
    public void Apply(LoopFinder<T> manager)
    {
        foreach (var start in manager)
        {
            if (manager.GetLinks(start, LinkStrength.Strong).Count == 0) continue;
            LoopBuilder<T> path = new LoopBuilder<T>(start);
            Search(path, manager);
        }
    }

    private void Search(LoopBuilder<T> path, LoopFinder<T> manager)
    {
        //if (manager.Loops.Contains(path.End(LinkStrength.None))) return;
        var last = path.LastElement();

        if (path.Count % 2 == 1)
        {
            foreach (var friend in manager.GetLinks(last, LinkStrength.Strong))
            {
                switch (path.Contains(friend))
                {
                    case ContainedStatus.First :
                        if (path.Count >= 4) manager.AddLoop(path.End(LinkStrength.Strong));
                        break;
                    case ContainedStatus.NotContained :
                        Search(path.Add(friend, LinkStrength.Strong), manager);
                        break;
                } 
            }
        }
        else
        {
            if (path.Count >= 4)
            {
                var weakFromFirst = manager.GetLinks(path.FirstElement(), LinkStrength.Weak);
                foreach (var weakFromLast in manager.GetLinks(last, LinkStrength.Weak))
                {
                    if (weakFromFirst.Contains(weakFromLast))
                    {
                        if(path.Contains(weakFromLast) == ContainedStatus.Contained) continue;
                        manager.AddLoop(path.Add(weakFromLast, LinkStrength.Weak).End(LinkStrength.Weak));
                    }
                }
            }
            
            foreach (var friend in manager.GetLinks(last, LinkStrength.Weak))
            {
                switch (path.Contains(friend))
                {
                    case ContainedStatus.First :
                        if (path.Count >= 4) manager.AddLoop(path.End(LinkStrength.Weak));
                        break;
                    case ContainedStatus.NotContained :
                        Search(path.Add(friend, LinkStrength.Weak), manager);
                        break;
                } 
            }
            
            foreach (var friend in manager.GetLinks(last, LinkStrength.Strong))
            {
                switch (path.Contains(friend))
                {
                    case ContainedStatus.First :
                        if (path.Count >= 4) manager.AddLoop(path.End(LinkStrength.Weak));
                        break;
                    case ContainedStatus.NotContained :
                        Search(path.Add(friend, LinkStrength.Weak), manager);
                        break;
                } 
            }
        }
    }
}