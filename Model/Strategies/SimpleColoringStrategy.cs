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
    public StatisticsTracker Tracker { get; } = new();

    public void ApplyOnce(IStrategyManager strategyManager)
    {
        var manager = new LinkGraphManager(strategyManager);
        manager.Construct(ConstructRule.UnitStrongLink);
        var graph = manager.LinkGraph;

        foreach (var coloredVertices in ColorHelper.Color<PossibilityCoordinate>(graph))
        {
            if(coloredVertices.Count <= 1) continue;

            if (SearchForTwiceInTheSameUnit(strategyManager, coloredVertices))
            {
                strategyManager.ChangeBuffer.Push(this, new SimpleColoringReportBuilder(coloredVertices, graph, true));
                continue;
            }
            
            SearchForTwoColorsElsewhere(strategyManager, coloredVertices);
            if (strategyManager.ChangeBuffer.NotEmpty())
                strategyManager.ChangeBuffer.Push(this, new SimpleColoringReportBuilder(coloredVertices, graph));
        }
    }

    private bool SearchForTwiceInTheSameUnit(IStrategyManager strategyManager,
        ColoredVertices<PossibilityCoordinate> cv)
    {
        return SearchColorForTwiceInTheSameUnit(strategyManager, cv.On, cv.Off) ||
               SearchColorForTwiceInTheSameUnit(strategyManager, cv.Off, cv.On);
    }

    private bool SearchColorForTwiceInTheSameUnit(IStrategyManager strategyManager,
        List<PossibilityCoordinate> toSearch, List<PossibilityCoordinate> other)
    {
        for (int i = 0; i < toSearch.Count; i++)
        {
            for (int j = i + 1; j < toSearch.Count; j++)
            {
                if (toSearch[i].ShareAUnit(toSearch[j]))
                {
                    foreach (var coord in other)
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
                if (on.Row == off.Row || on.Col == off.Col) continue;

                foreach (var coord in on.SharedSeenCells(off))
                {
                    strategyManager.ChangeBuffer.AddPossibilityToRemove(on.Possibility, coord.Row, coord.Col);
                }
            }
        }
    }
}

public class SimpleColoringReportBuilder : IChangeReportBuilder
{
    private readonly ColoredVertices<PossibilityCoordinate> _vertices;
    private readonly LinkGraph<ILinkGraphElement> _graph;
    private readonly bool _isInvalidColoring;

    public SimpleColoringReportBuilder(ColoredVertices<PossibilityCoordinate> vertices,
        LinkGraph<ILinkGraphElement> graph, bool isInvalidColoring = false)
    {
        _vertices = vertices;
        _graph = graph;
        _isInvalidColoring = isInvalidColoring;
    }

    public ChangeReport Build(List<SolverChange> changes, IChangeManager manager)
    {
        List<Link<PossibilityCoordinate>> links = FindPath();

        HighlightSolver[] highlights = new HighlightSolver[_isInvalidColoring ? 2 : 1];
        if (_isInvalidColoring)
        {
            highlights[0] = lighter => IChangeReportBuilder.HighlightChanges(lighter, changes);
            highlights[1] = lighter =>
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
            };
        }
        else
        {
            highlights[0] = lighter =>
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
            };
        }

        return new ChangeReport(IChangeReportBuilder.ChangesToString(changes), "", highlights);
    }

    private List<Link<PossibilityCoordinate>> FindPath()
    {
        List<Link<PossibilityCoordinate>> links = new();

        Queue<PossibilityCoordinate> queue = new();
        HashSet<PossibilityCoordinate> inGraph = new HashSet<PossibilityCoordinate>(_vertices.On);
        inGraph.UnionWith(_vertices.Off);
        
        queue.Enqueue(_vertices.On[0]);

        while (queue.Count > 0 && inGraph.Count > 0)
        {
            var current = queue.Dequeue();

            foreach (var friend in _graph.GetLinks(current, LinkStrength.Strong))
            {
                if(friend is not PossibilityCoordinate pc || !inGraph.Contains(pc)) continue;

                links.Add(new Link<PossibilityCoordinate>(current, pc));
                inGraph.Remove(pc);
                queue.Enqueue(pc);
            }
        }

        return links;
    }
}