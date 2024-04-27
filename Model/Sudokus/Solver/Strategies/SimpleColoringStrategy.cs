using System.Collections.Generic;
using Model.Helpers;
using Model.Helpers.Changes;
using Model.Helpers.Graphs;
using Model.Helpers.Highlighting;
using Model.Sudokus.Solver.Utility;
using Model.Sudokus.Solver.Utility.CellColoring;
using Model.Sudokus.Solver.Utility.CellColoring.ColoringResults;
using Model.Sudokus.Solver.Utility.Graphs;
using Model.Utility;

namespace Model.Sudokus.Solver.Strategies;

public class SimpleColoringStrategy : SudokuStrategy
{
    public const string OfficialName = "Simple Coloring";
    private const InstanceHandling DefaultInstanceHandling = InstanceHandling.FirstOnly;

    public SimpleColoringStrategy() : base(OfficialName, StrategyDifficulty.Medium, DefaultInstanceHandling){}

    public override void Apply(ISudokuStrategyUser strategyUser)
    {
        strategyUser.PreComputer.Graphs.ConstructSimple(SudokuConstructRuleBank.UnitStrongLink);
        var graph = strategyUser.PreComputer.Graphs.SimpleLinkGraph;

        foreach (var coloredVertices in ColorHelper.ColorAll<CellPossibility,
                     ColoringListCollection<CellPossibility>>(ColorHelper.Algorithm.ColorWithoutRules, graph,
                     Coloring.On, strategyUser.StepsManaged))
        {
            if(coloredVertices.Count <= 1) continue;

            if (SearchForTwiceInTheSameUnit(strategyUser, coloredVertices))
            {
                if (strategyUser.ChangeBuffer.NotEmpty() && strategyUser.ChangeBuffer.Commit(
                        new SimpleColoringReportBuilder(coloredVertices, true)) &&
                            StopOnFirstPush) return;
                
                continue;
            }
            
            SearchForTwoColorsElsewhere(strategyUser, coloredVertices);
            
            if (strategyUser.ChangeBuffer.NotEmpty() && strategyUser.ChangeBuffer.Commit(
                    new SimpleColoringReportBuilder(coloredVertices)) && StopOnFirstPush) return;
        }
    }

    private bool SearchForTwiceInTheSameUnit(ISudokuStrategyUser strategyUser,
        ColoringList<CellPossibility> cv)
    {
        return SearchColorForTwiceInTheSameUnit(strategyUser, cv.On, cv.Off) ||
               SearchColorForTwiceInTheSameUnit(strategyUser, cv.Off, cv.On);
    }

    private bool SearchColorForTwiceInTheSameUnit(ISudokuStrategyUser strategyUser,
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

    private void SearchForTwoColorsElsewhere(ISudokuStrategyUser strategyUser,
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

public class SimpleColoringReportBuilder : IChangeReportBuilder<IUpdatableSudokuSolvingState, ISudokuHighlighter>
{
    private readonly ColoringList<CellPossibility> _vertices;
    private readonly bool _isInvalidColoring;

    public SimpleColoringReportBuilder(ColoringList<CellPossibility> vertices, bool isInvalidColoring = false)
    {
        _vertices = vertices;
        _isInvalidColoring = isInvalidColoring;
    }

    public ChangeReport<ISudokuHighlighter> BuildReport(IReadOnlyList<SolverProgress> changes, IUpdatableSudokuSolvingState snapshot)
    {
        var highlights = new Highlight<ISudokuHighlighter>[_isInvalidColoring ? 2 : 1];
        if (_isInvalidColoring)
        {
            highlights[0] = lighter => ChangeReportHelper.HighlightChanges(lighter, changes);
            highlights[1] = lighter =>
            {
                _vertices.History?.ForeachLink((one, two)
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
                _vertices.History?.ForeachLink((one, two)
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

        return new ChangeReport<ISudokuHighlighter>( "", highlights);
    }
    
    public Clue<ISudokuHighlighter> BuildClue(IReadOnlyList<SolverProgress> changes, IUpdatableSudokuSolvingState snapshot)
    {
        return Clue<ISudokuHighlighter>.Default();
    }
}