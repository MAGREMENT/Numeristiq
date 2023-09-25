using System.Collections.Generic;
using Model.Solver;
using Model.Solver.Helpers;
using Model.Solver.Helpers.Changes;
using Model.Solver.StrategiesUtil;
using Model.Solver.StrategiesUtil.LinkGraph;
using Model.StrategiesUtil;

namespace Model.DeprecatedStrategies;

public class OldSimpleColoringStrategy : IStrategy
{
    public string Name => "Simple coloring";
    
    public StrategyDifficulty Difficulty => StrategyDifficulty.Medium;
    public StatisticsTracker Tracker { get; } = new();

    public void ApplyOnce(IStrategyManager strategyManager)
    {
        for (int number = 1; number <= 9; number++)
        {
            List<ColorableWeb<CellColoring>> chains = new();
            for (int row = 0; row < 9; row++)
            {
                for (int col = 0; col < 9; col++)
                {
                    if (strategyManager.PossibilitiesAt(row, col).Peek(number))
                    {
                        CellColoring current = new(row, col);
                        if (DoesAnyChainContains(chains, current)) continue;
                        
                        ColorableWeb<CellColoring> web = new();
                        InitChain(strategyManager, web, current, number);
                        if (web.Count >= 2)
                        {
                            web.StartColoring();
                            chains.Add(web);
                        }
                    }
                }
            }

            foreach (var chain in chains)
            {
                SearchForTwiceInTheSameUnit(strategyManager, number, chain);
                SearchForTwoColorsElsewhere(strategyManager, number, chain);
                
                strategyManager.ChangeBuffer.Push(this,
                    new OldSimpleColoringReportBuilder(number, chain));
            }
        }
    }

    private void SearchForTwiceInTheSameUnit(IStrategyManager strategyManager, int number, ColorableWeb<CellColoring> web)
    {
        web.ForEachCombinationOfTwo((one, two) =>
        {
            if (one.Cell.ShareAUnit(two.Cell) && one.Coloring == two.Coloring)
            {
                foreach (var coord in web)
                {
                    if (coord.Coloring == one.Coloring) strategyManager.ChangeBuffer.AddPossibilityToRemove(number, coord.Cell.Row, coord.Cell.Col);
                    else strategyManager.ChangeBuffer.AddSolutionToAdd(number, coord.Cell.Row, coord.Cell.Col);
                }
            }

            return false;
        });
    }

    private void SearchForTwoColorsElsewhere(IStrategyManager strategyManager,
        int number, ColorableWeb<CellColoring> web)
    {
        for (int row = 0; row < 9; row++)
        {
            for (int col = 0; col < 9; col++)
            {
                if (strategyManager.PossibilitiesAt(row, col).Peek(number))
                {
                    CellColoring current = new(row, col);
                    if (web.Contains(current)) continue;

                    bool[] onAndOff = new bool[2];
                    foreach (var coord in web)
                    {
                        if (coord.Cell.ShareAUnit(current.Cell))
                        {
                            onAndOff[(int)(coord.Coloring - 1)] = true;
                            if (onAndOff[0] && onAndOff[1])
                            {
                                strategyManager.ChangeBuffer.AddPossibilityToRemove(number, row, col);
                                break;
                            }
                        }
                    }
                }
            }
        }
    }

    private void InitChain(IStrategyManager strategyManager, ColorableWeb<CellColoring> web, CellColoring current, int number)
    {
        var ppir = strategyManager.RowPositionsAt(current.Cell.Row, number);
        if (ppir.Count == 2)
        {
            foreach (var col in ppir)
            {
                if (col != current.Cell.Col)
                {
                    CellColoring next = new CellColoring(current.Cell.Row, col);
                    if(web.AddLink(current, next)) InitChain(strategyManager, web, next, number);
                    break;
                }
            }
        }
        
        var ppic = strategyManager.ColumnPositionsAt(current.Cell.Col, number);
        if (ppic.Count == 2)
        {
            foreach (var row in ppic)
            {
                if (row != current.Cell.Row)
                {
                    CellColoring next = new CellColoring(row, current.Cell.Col);
                    if(web.AddLink(current, next)) InitChain(strategyManager, web, next, number);
                    break;
                }
            }
        }
        
        var ppimn = strategyManager.MiniGridPositionsAt(current.Cell.Row / 3, current.Cell.Col / 3, number);
        if (ppimn.Count == 2)
        {
            foreach (var pos in ppimn)
            {
                if (pos.Row != current.Cell.Row && pos.Col != current.Cell.Col)
                {
                    CellColoring next = new CellColoring(pos.Row, pos.Col);
                    if(web.AddLink(current, next)) InitChain(strategyManager, web, next, number);
                    break;
                }
            }
        }
    }

    private static bool DoesAnyChainContains(IEnumerable<ColorableWeb<CellColoring>> chains, CellColoring coord)
    {
        foreach (var chain in chains)
        {
            if (chain.Contains(coord)) return true;
        }

        return false;
    }
}

public class OldSimpleColoringReportBuilder : IChangeReportBuilder
{
    private readonly int _number;
    private readonly ColorableWeb<CellColoring> _web;

    public OldSimpleColoringReportBuilder(int number, ColorableWeb<CellColoring> web)
    {
        _number = number;
        _web = web;
    }

    public ChangeReport Build(List<SolverChange> changes, IPossibilitiesHolder snapshot)
    {
        return new ChangeReport(IChangeReportBuilder.ChangesToString(changes), "", lighter =>
        {
            foreach (var coord in _web)
            {
                lighter.HighlightPossibility(_number, coord.Cell.Row, coord.Cell.Col, coord.Coloring == Coloring.On ?
                    ChangeColoration.CauseOnOne : ChangeColoration.CauseOffTwo);

                foreach (var friend in _web.GetLinkedVertices(coord))
                {
                    if (friend.Coloring == Coloring.Off) continue;
                    lighter.CreateLink(new CellPossibility(friend.Cell.Row, friend.Cell.Col, _number),
                        new CellPossibility(coord.Cell.Row, coord.Cell.Col, _number), LinkStrength.Strong);
                }
            }

            IChangeReportBuilder.HighlightChanges(lighter, changes);
        });
    }
}