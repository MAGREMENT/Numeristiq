using System.Collections.Generic;
using Model.Changes;
using Model.Solver;
using Model.StrategiesUtil;
using Model.StrategiesUtil.LinkGraph;

namespace Model.Strategies;

public class OldSimpleColoringStrategy : IStrategy
{
    public string Name => "Simple coloring";
    
    public StrategyLevel Difficulty => StrategyLevel.Medium;
    public int Score { get; set; }

    public void ApplyOnce(IStrategyManager strategyManager)
    {
        for (int number = 1; number <= 9; number++)
        {
            List<ColorableWeb<CoordinateColoring>> chains = new();
            for (int row = 0; row < 9; row++)
            {
                for (int col = 0; col < 9; col++)
                {
                    if (strategyManager.Possibilities[row, col].Peek(number))
                    {
                        CoordinateColoring current = new(row, col);
                        if (DoesAnyChainContains(chains, current)) continue;
                        
                        ColorableWeb<CoordinateColoring> web = new();
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

    private void SearchForTwiceInTheSameUnit(IStrategyManager strategyManager, int number, ColorableWeb<CoordinateColoring> web)
    {
        web.ForEachCombinationOfTwo((one, two) =>
        {
            if (one.Coordinate.ShareAUnit(two.Coordinate) && one.Coloring == two.Coloring)
            {
                foreach (var coord in web)
                {
                    if (coord.Coloring == one.Coloring) strategyManager.ChangeBuffer.AddPossibilityToRemove(number, coord.Coordinate.Row, coord.Coordinate.Col);
                    else strategyManager.ChangeBuffer.AddDefinitiveToAdd(number, coord.Coordinate.Row, coord.Coordinate.Col);
                }
            }

            return false;
        });
    }

    private void SearchForTwoColorsElsewhere(IStrategyManager strategyManager,
        int number, ColorableWeb<CoordinateColoring> web)
    {
        for (int row = 0; row < 9; row++)
        {
            for (int col = 0; col < 9; col++)
            {
                if (strategyManager.Possibilities[row, col].Peek(number))
                {
                    CoordinateColoring current = new(row, col);
                    if (web.Contains(current)) continue;

                    bool[] onAndOff = new bool[2];
                    foreach (var coord in web)
                    {
                        if (coord.Coordinate.ShareAUnit(current.Coordinate))
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

    private void InitChain(IStrategyManager strategyManager, ColorableWeb<CoordinateColoring> web, CoordinateColoring current, int number)
    {
        var ppir = strategyManager.RowPositions(current.Coordinate.Row, number);
        if (ppir.Count == 2)
        {
            foreach (var col in ppir)
            {
                if (col != current.Coordinate.Col)
                {
                    CoordinateColoring next = new CoordinateColoring(current.Coordinate.Row, col);
                    if(web.AddLink(current, next)) InitChain(strategyManager, web, next, number);
                    break;
                }
            }
        }
        
        var ppic = strategyManager.ColumnPositions(current.Coordinate.Col, number);
        if (ppic.Count == 2)
        {
            foreach (var row in ppic)
            {
                if (row != current.Coordinate.Row)
                {
                    CoordinateColoring next = new CoordinateColoring(row, current.Coordinate.Col);
                    if(web.AddLink(current, next)) InitChain(strategyManager, web, next, number);
                    break;
                }
            }
        }
        
        var ppimn = strategyManager.MiniGridPositions(current.Coordinate.Row / 3, current.Coordinate.Col / 3, number);
        if (ppimn.Count == 2)
        {
            foreach (var pos in ppimn)
            {
                if (pos[0] != current.Coordinate.Row && pos[1] != current.Coordinate.Col)
                {
                    CoordinateColoring next = new CoordinateColoring(pos[0], pos[1]);
                    if(web.AddLink(current, next)) InitChain(strategyManager, web, next, number);
                    break;
                }
            }
        }
    }

    private static bool DoesAnyChainContains(IEnumerable<ColorableWeb<CoordinateColoring>> chains, CoordinateColoring coord)
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
    private readonly ColorableWeb<CoordinateColoring> _web;

    public OldSimpleColoringReportBuilder(int number, ColorableWeb<CoordinateColoring> web)
    {
        _number = number;
        _web = web;
    }

    public ChangeReport Build(List<SolverChange> changes, IChangeManager manager)
    {
        return new ChangeReport(IChangeReportBuilder.ChangesToString(changes), "", lighter =>
        {
            foreach (var coord in _web)
            {
                lighter.HighlightPossibility(_number, coord.Coordinate.Row, coord.Coordinate.Col, coord.Coloring == Coloring.On ?
                    ChangeColoration.CauseOnOne : ChangeColoration.CauseOffTwo);

                foreach (var friend in _web.GetLinkedVertices(coord))
                {
                    if (friend.Coloring == Coloring.Off) continue;
                    lighter.CreateLink(new PossibilityCoordinate(friend.Coordinate.Row, friend.Coordinate.Col, _number),
                        new PossibilityCoordinate(coord.Coordinate.Row, coord.Coordinate.Col, _number), LinkStrength.Strong);
                }
            }

            IChangeReportBuilder.HighlightChanges(lighter, changes);
        });
    }
}