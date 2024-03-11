using System.Collections.Generic;
using Model.Helpers;
using Model.Helpers.Changes;
using Model.Helpers.Highlighting;
using Model.Sudoku.Solver.StrategiesUtility;
using Model.Sudoku.Solver.StrategiesUtility.CellColoring;
using Model.Sudoku.Solver.StrategiesUtility.CellColoring.ColoringResults;
using Model.Sudoku.Solver.StrategiesUtility.Graphs;

namespace Model.Sudoku.Solver.Strategies;

public class SimpleColoringStrategy : SudokuStrategy
{
    public const string OfficialName = "Simple Coloring";
    private const OnCommitBehavior DefaultBehavior = OnCommitBehavior.Return;
    
    public override OnCommitBehavior DefaultOnCommitBehavior => DefaultBehavior;
    
    public SimpleColoringStrategy() : base(OfficialName, StrategyDifficulty.Medium, DefaultBehavior){}

    public override void Apply(IStrategyUser strategyUser)
    {
        strategyUser.PreComputer.Graphs.ConstructSimple(ConstructRule.UnitStrongLink);
        var graph = strategyUser.PreComputer.Graphs.SimpleLinkGraph;

        foreach (var coloredVertices in ColorHelper.ColorAll<CellPossibility,
                     ColoringListCollection<CellPossibility>>(ColorHelper.Algorithm.ColorWithoutRules, graph,
                     Coloring.On, strategyUser.LogsManaged))
        {
            if(coloredVertices.Count <= 1) continue;

            if (SearchForTwiceInTheSameUnit(strategyUser, coloredVertices))
            {
                if (strategyUser.ChangeBuffer.NotEmpty() && strategyUser.ChangeBuffer.Commit(
                        new SimpleColoringReportBuilder(coloredVertices, true)) &&
                            OnCommitBehavior == OnCommitBehavior.Return) return;
                
                continue;
            }
            
            SearchForTwoColorsElsewhere(strategyUser, coloredVertices);
            
            if (strategyUser.ChangeBuffer.NotEmpty() && strategyUser.ChangeBuffer.Commit(
                    new SimpleColoringReportBuilder(coloredVertices)) && OnCommitBehavior == OnCommitBehavior.Return) return;
        }
    }

    private bool SearchForTwiceInTheSameUnit(IStrategyUser strategyUser,
        ColoringList<CellPossibility> cv)
    {
        return SearchColorForTwiceInTheSameUnit(strategyUser, cv.On, cv.Off) ||
               SearchColorForTwiceInTheSameUnit(strategyUser, cv.Off, cv.On);
    }

    private bool SearchColorForTwiceInTheSameUnit(IStrategyUser strategyUser,
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
                        strategyUser.ChangeBuffer.ProposeSolutionAddition(coord);
                    }

                    return true;
                }
            }
        }

        return false;
    }

    private void SearchForTwoColorsElsewhere(IStrategyUser strategyUser,
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
                    
                    strategyUser.ChangeBuffer.ProposePossibilityRemoval(on.Possibility, coord.Row, coord.Column);
                }
            }
        }
    }
}

public class SimpleColoringReportBuilder : IChangeReportBuilder<IUpdatableSudokuSolvingState>
{
    private readonly ColoringList<CellPossibility> _vertices;
    private readonly bool _isInvalidColoring;

    public SimpleColoringReportBuilder(ColoringList<CellPossibility> vertices, bool isInvalidColoring = false)
    {
        _vertices = vertices;
        _isInvalidColoring = isInvalidColoring;
    }

    public ChangeReport Build(IReadOnlyList<SolverProgress> changes, IUpdatableSudokuSolvingState snapshot)
    {
        Highlight<ISudokuHighlighter>[] highlights = new Highlight<ISudokuHighlighter>[_isInvalidColoring ? 2 : 1];
        if (_isInvalidColoring)
        {
            highlights[0] = lighter => ChangeReportHelper.HighlightChanges(lighter, changes);
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
            
                ChangeReportHelper.HighlightChanges(lighter, changes);
            };
        }

        return new ChangeReport( "", highlights);
    }
}