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
            if (!SearchForTwiceInTheSameUnit(strategyManager, coloredVertices))
                SearchForTwoColorsElsewhere(strategyManager, coloredVertices);

            if (strategyManager.ChangeBuffer.NotEmpty())
                strategyManager.ChangeBuffer.Push(this, new SimpleColoringReportBuilder(coloredVertices));
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

    public SimpleColoringReportBuilder(ColoredVertices<PossibilityCoordinate> vertices)
    {
        _vertices = vertices;
    }

    public ChangeReport Build(List<SolverChange> changes, IChangeManager manager) //TODO improve
    {
        List<Link<PossibilityCoordinate>> links = new();

        HashSet<PossibilityCoordinate> once = new();
        HashSet<PossibilityCoordinate> twice = new();

        foreach (var on in _vertices.On)
        {
            if(twice.Contains(on)) continue;
            
            foreach (var off in _vertices.Off)
            {
                if(twice.Contains(off) || twice.Contains(on)) continue;
                
                if (on.ShareAUnit(off))
                {
                    links.Add(new Link<PossibilityCoordinate>(on, off));

                    if (once.Contains(on)) twice.Add(on);
                    else once.Add(on);

                    if (once.Contains(off)) twice.Add(off);
                    else once.Add(off);
                }
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