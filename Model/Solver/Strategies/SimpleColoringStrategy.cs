using System.Collections.Generic;
using Global.Enums;
using Model.Solver.Helpers.Changes;
using Model.Solver.Helpers.Highlighting;
using Model.Solver.StrategiesUtility;
using Model.Solver.StrategiesUtility.CellColoring;
using Model.Solver.StrategiesUtility.CellColoring.ColoringResults;
using Model.Solver.StrategiesUtility.Graphs;

namespace Model.Solver.Strategies;

public class SimpleColoringStrategy : AbstractStrategy
{
    public const string OfficialName = "Simple Coloring";
    private const OnCommitBehavior DefaultBehavior = OnCommitBehavior.Return;
    
    public override OnCommitBehavior DefaultOnCommitBehavior => DefaultBehavior;
    
    public SimpleColoringStrategy() : base(OfficialName, StrategyDifficulty.Medium, DefaultBehavior){}

    public override void Apply(IStrategyManager strategyManager)
    {
        strategyManager.GraphManager.ConstructSimple(ConstructRule.UnitStrongLink);
        var graph = strategyManager.GraphManager.SimpleLinkGraph;

        foreach (var coloredVertices in ColorHelper.ColorAll<CellPossibility,
                     ColoringListCollection<CellPossibility>>(ColorHelper.Algorithm.ColorWithoutRules, graph,
                     Coloring.On, strategyManager.LogsManaged))
        {
            if(coloredVertices.Count <= 1) continue;

            if (SearchForTwiceInTheSameUnit(strategyManager, coloredVertices))
            {
                if (strategyManager.ChangeBuffer.NotEmpty() && strategyManager.ChangeBuffer.Commit(this,
                        new SimpleColoringReportBuilder(coloredVertices, true)) &&
                            OnCommitBehavior == OnCommitBehavior.Return) return;
                
                continue;
            }
            
            SearchForTwoColorsElsewhere(strategyManager, coloredVertices);
            
            if (strategyManager.ChangeBuffer.NotEmpty() && strategyManager.ChangeBuffer.Commit(this,
                    new SimpleColoringReportBuilder(coloredVertices)) && OnCommitBehavior == OnCommitBehavior.Return) return;
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
                        strategyManager.ChangeBuffer.ProposeSolutionAddition(coord);
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
                    
                    strategyManager.ChangeBuffer.ProposePossibilityRemoval(on.Possibility, coord.Row, coord.Column);
                }
            }
        }
    }
}

public class SimpleColoringReportBuilder : IChangeReportBuilder
{
    private readonly ColoringList<CellPossibility> _vertices;
    private readonly bool _isInvalidColoring;

    public SimpleColoringReportBuilder(ColoringList<CellPossibility> vertices, bool isInvalidColoring = false)
    {
        _vertices = vertices;
        _isInvalidColoring = isInvalidColoring;
    }

    public ChangeReport Build(IReadOnlyList<SolverChange> changes, IPossibilitiesHolder snapshot)
    {
        Highlight[] highlights = new Highlight[_isInvalidColoring ? 2 : 1];
        if (_isInvalidColoring)
        {
            highlights[0] = lighter => IChangeReportBuilder.HighlightChanges(lighter, changes);
            highlights[1] = lighter =>
            {
                _vertices.History!.ForeachLink((one, two)
                    => lighter.CreateLink(one, two, LinkStrength.Strong));

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
                _vertices.History!.ForeachLink((one, two)
                    => lighter.CreateLink(one, two, LinkStrength.Strong));

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
}