using System.Collections.Generic;
using Model.Changes;
using Model.Solver;
using Model.StrategiesUtil;
using Model.StrategiesUtil.LinkGraph;

namespace Model.Strategies;

public class SimpleColoringStrategy : IStrategy
{
    public string Name => "Simple coloring";
    public StrategyLevel Difficulty => StrategyLevel.Medium;
    public int Score { get; set; }
    public void ApplyOnce(IStrategyManager strategyManager)
    {
        var manager = new LinkGraphManager(strategyManager);
        manager.Construct(ConstructRule.UnitStrongLink);
        var graph = manager.LinkGraph;

        foreach (var coloredVertices in Color(graph))
        {
            if(!SearchForTwiceInTheSameUnit(strategyManager, coloredVertices))
                SearchForTwoColorsElsewhere(strategyManager, coloredVertices);

            if(strategyManager.ChangeBuffer.NotEmpty())
                strategyManager.ChangeBuffer.Push(this, new SimpleColoringReportBuilder(coloredVertices));
        }
    }

    private bool SearchForTwiceInTheSameUnit(IStrategyManager strategyManager,
        ColoredVertices<PossibilityCoordinate> cv)
    {
        for (int i = 0; i < cv.On.Count; i++)
        {
            for (int j = i + 1; j < cv.On.Count; j++)
            {
                if (cv.On[i].ShareAUnit(cv.On[j]))
                {
                    foreach (var coord in cv.Off)
                    {
                        strategyManager.ChangeBuffer.AddDefinitiveToAdd(coord);
                    }

                    return true;
                }
            }
        }
        
        for (int i = 0; i < cv.Off.Count; i++)
        {
            for (int j = i + 1; j < cv.Off.Count; j++)
            {
                if (cv.Off[i].ShareAUnit(cv.Off[j]))
                {
                    foreach (var coord in cv.On)
                    {
                        strategyManager.ChangeBuffer.AddDefinitiveToAdd(coord);
                    }

                    return true;
                }
            }
        }

        return false;
    }
    
    private void SearchForTwoColorsElsewhere(IStrategyManager strategyManager,
        ColoredVertices<PossibilityCoordinate> cv)
    {
        foreach (var on in cv.On)
        {
            foreach (var off in cv.Off)
            {
                if(on.Row == off.Row || on.Col == off.Col) continue;

                foreach (var coord in on.SharedSeenCells(off))
                {
                    strategyManager.ChangeBuffer.AddPossibilityToRemove(on.Possibility, coord.Row, coord.Col);
                }
            }
        }
    }

    private List<ColoredVertices<PossibilityCoordinate>> Color(LinkGraph<ILinkGraphElement> graph)
    {
        var result = new List<ColoredVertices<PossibilityCoordinate>>();
        HashSet<ILinkGraphElement> visited = new();

        foreach (var start in graph)
        {
            if (visited.Contains(start)) continue;

            ColoredVertices<PossibilityCoordinate> cv = new();
            cv.Add((PossibilityCoordinate)start, Coloring.On);
            visited.Add(start);

            Queue<ColoredElement> queue = new();
            queue.Enqueue(new ColoredElement(start, Coloring.On));

            while (queue.Count > 0)
            {
                var current = queue.Dequeue();
                var opposite = current.Coloring == Coloring.Off ? Coloring.On : Coloring.Off;

                foreach (var friend in graph.GetLinks(current.Element, LinkStrength.Strong))
                {
                    if (visited.Contains(friend)) continue;

                    cv.Add((PossibilityCoordinate)friend, opposite);
                    visited.Add(friend);
                    queue.Enqueue(new ColoredElement(friend, opposite));
                }
                
                foreach (var friend in graph.GetLinks(current.Element, LinkStrength.Weak))
                {
                    if (visited.Contains(friend)) continue;

                    cv.Add((PossibilityCoordinate)friend, opposite);
                    visited.Add(friend);
                    queue.Enqueue(new ColoredElement(friend, opposite));
                }
            }

            result.Add(cv);
        }

        return result;
    }
}

public class ColoredElement
{
    public ColoredElement(ILinkGraphElement element, Coloring coloring)
    {
        Element = element;
        Coloring = coloring;
    }

    public ILinkGraphElement Element { get; }
    public Coloring Coloring { get; }
}

public class SimpleColoringReportBuilder : IChangeReportBuilder
{
    private readonly ColoredVertices<PossibilityCoordinate> _vertices;

    public SimpleColoringReportBuilder(ColoredVertices<PossibilityCoordinate> vertices)
    {
        _vertices = vertices;
    }

    public ChangeReport Build(List<SolverChange> changes, IChangeManager manager)
    {
        List<Link<PossibilityCoordinate>> links = new();

        foreach (var on in _vertices.On)
        {
            foreach (var off in _vertices.Off)
            {
                if (on.ShareAUnit(off)) links.Add(new Link<PossibilityCoordinate>(on, off));
            }
        }
        
        return new ChangeReport(IChangeReportBuilder.ChangesToString(changes), "", lighter =>
        {
            foreach (var link in links)
            {
                lighter.CreateLink(link.From, link.To, LinkStrength.Strong);
            }

            foreach (var coord in _vertices.On)
            {
                lighter.HighlightPossibility(coord, ChangeColoration.CauseOnOne);
            }

            foreach (var coord in _vertices.Off)
            {
                lighter.HighlightPossibility(coord, ChangeColoration.CauseOffOne);
            }
            
            IChangeReportBuilder.HighlightChanges(lighter, changes);
        });
    }
}