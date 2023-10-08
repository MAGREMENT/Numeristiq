using System.Collections.Generic;
using Model.Solver.Helpers.Changes;
using Model.Solver.Helpers.Highlighting;
using Model.Solver.StrategiesUtil;
using Model.Solver.StrategiesUtil.CellColoring;
using Model.Solver.StrategiesUtil.CellColoring.ColoringResults;
using Model.Solver.StrategiesUtil.LinkGraph;

namespace Model.Solver.Strategies;

public class SimpleColoringStrategy : AbstractStrategy
{
    public const string OfficialName = "Simple Coloring";
    
    public SimpleColoringStrategy() : base(OfficialName, StrategyDifficulty.Medium){}

    public override void ApplyOnce(IStrategyManager strategyManager)
    {
        strategyManager.GraphManager.ConstructSimple(ConstructRule.UnitStrongLink);
        var graph = strategyManager.GraphManager.SimpleLinkGraph;

        foreach (var coloredVertices in ColorHelper.ColorAll<CellPossibility,
                     ColoringListCollection<CellPossibility>>(ColorHelper.Algorithm.ColoringWithoutRules, graph))
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
        ColoringList<CellPossibility> cv)
    {
        return SearchColorForTwiceInTheSameUnit(strategyManager, cv.On, cv.Off) ||
               SearchColorForTwiceInTheSameUnit(strategyManager, cv.Off, cv.On);
    }

    private bool SearchColorForTwiceInTheSameUnit(IStrategyManager strategyManager,
        IReadOnlyList<CellPossibility> toSearch, IReadOnlyList<CellPossibility> other)
    {
        for (int i = 0; i < toSearch.Count; i++)
        {
            for (int j = i + 1; j < toSearch.Count; j++)
            {
                if (toSearch[i].ShareAUnit(toSearch[j]))
                {
                    foreach (var coord in other)
                    {
                        strategyManager.ChangeBuffer.AddSolutionToAdd(coord);
                    }

                    return true;
                }
            }
        }

        return false;
    }

    private void SearchForTwoColorsElsewhere(IStrategyManager strategyManager,
        ColoringList<CellPossibility> cv)
    {
        HashSet<CellPossibility> inGraph = new(cv.On);
        inGraph.UnionWith(cv.Off);
        
        foreach (var on in cv.On)
        {
            foreach (var off in cv.Off)
            {
                foreach (var coord in on.SharedSeenCells(off))
                {
                    var current = new CellPossibility(coord, on.Possibility);
                    if (inGraph.Contains(current)) continue;
                    
                    strategyManager.ChangeBuffer.AddPossibilityToRemove(on.Possibility, coord.Row, coord.Col);
                }
            }
        }
    }
}

public class SimpleColoringReportBuilder : IChangeReportBuilder
{
    private readonly ColoringList<CellPossibility> _vertices;
    private readonly LinkGraph<CellPossibility> _graph;
    private readonly bool _isInvalidColoring;

    public SimpleColoringReportBuilder(ColoringList<CellPossibility> vertices,
        LinkGraph<CellPossibility> graph, bool isInvalidColoring = false)
    {
        _vertices = vertices;
        _graph = graph;
        _isInvalidColoring = isInvalidColoring;
    }

    public ChangeReport Build(List<SolverChange> changes, IPossibilitiesHolder snapshot)
    {
        List<Link<CellPossibility>> links = FindPath();

        Highlight[] highlights = new Highlight[_isInvalidColoring ? 2 : 1];
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

    private List<Link<CellPossibility>> FindPath()
    {
        List<Link<CellPossibility>> links = new();

        Queue<CellPossibility> queue = new();
        HashSet<CellPossibility> inGraph = new HashSet<CellPossibility>(_vertices.On);
        inGraph.UnionWith(_vertices.Off);
        
        queue.Enqueue(_vertices.On[0]);

        while (queue.Count > 0 && inGraph.Count > 0)
        {
            var current = queue.Dequeue();

            foreach (var friend in _graph.GetLinks(current, LinkStrength.Strong))
            {
                if(!inGraph.Contains(friend)) continue;

                links.Add(new Link<CellPossibility>(current, friend));
                inGraph.Remove(friend);
                queue.Enqueue(friend);
            }
        }

        return links;
    }
}