using System.Collections.Generic;
using Model.StrategiesUtil;

namespace Model.Strategies;

public class SimpleColoringStrategy : IStrategy
{
    public string Name => "Simple coloring";
    
    public StrategyLevel Difficulty => StrategyLevel.Medium;
    public int Score { get; set; }

    public void ApplyOnce(IStrategyManager strategyManager)
    {
        for (int number = 1; number <= 9; number++)
        {
            List<ColorableWeb<ColoringCoordinate>> chains = new();
            for (int row = 0; row < 9; row++)
            {
                for (int col = 0; col < 9; col++)
                {
                    if (strategyManager.Possibilities[row, col].Peek(number))
                    {
                        ColoringCoordinate current = new(row, col);
                        if (DoesAnyChainContains(chains, current)) continue;
                        
                        ColorableWeb<ColoringCoordinate> web = new();
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
                var changeBuffer = strategyManager.CreateChangeBuffer(this,
                    new SimpleColoringReportWaiter(number, chain));
                
                SearchForTwiceInTheSameUnit(changeBuffer, number, chain);
                SearchForTwoColorsElsewhere(strategyManager, changeBuffer, number, chain);
                
                changeBuffer.Push();
            }
        }
    }

    private void SearchForTwiceInTheSameUnit(ChangeBuffer changeBuffer, int number, ColorableWeb<ColoringCoordinate> web)
    {
        web.ForEachCombinationOfTwo((one, two) =>
        {
            if (one.ShareAUnit(two) && one.Coloring == two.Coloring)
            {
                foreach (var coord in web)
                {
                    if (coord.Coloring == one.Coloring) changeBuffer.AddPossibilityToRemove(number, coord.Row, coord.Col);
                    else changeBuffer.AddDefinitiveToAdd(number, coord.Row, coord.Col);
                }
            }

            return false;
        });
    }

    private void SearchForTwoColorsElsewhere(IStrategyManager strategyManager, ChangeBuffer changeBuffer,
        int number, ColorableWeb<ColoringCoordinate> web)
    {
        for (int row = 0; row < 9; row++)
        {
            for (int col = 0; col < 9; col++)
            {
                if (strategyManager.Possibilities[row, col].Peek(number))
                {
                    ColoringCoordinate current = new(row, col);
                    if (web.Contains(current)) continue;

                    bool[] onAndOff = new bool[2];
                    foreach (var coord in web)
                    {
                        if (coord.ShareAUnit(current))
                        {
                            onAndOff[(int)(coord.Coloring - 1)] = true;
                            if (onAndOff[0] && onAndOff[1])
                            {
                                changeBuffer.AddPossibilityToRemove(number, row, col);
                                break;
                            }
                        }
                    }
                }
            }
        }
    }

    private void InitChain(IStrategyManager strategyManager, ColorableWeb<ColoringCoordinate> web, ColoringCoordinate current, int number)
    {
        var ppir = strategyManager.PossibilityPositionsInRow(current.Row, number);
        if (ppir.Count == 2)
        {
            foreach (var col in ppir)
            {
                if (col != current.Col)
                {
                    ColoringCoordinate next = new ColoringCoordinate(current.Row, col);
                    if(web.AddLink(current, next)) InitChain(strategyManager, web, next, number);
                    break;
                }
            }
        }
        
        var ppic = strategyManager.PossibilityPositionsInColumn(current.Col, number);
        if (ppic.Count == 2)
        {
            foreach (var row in ppic)
            {
                if (row != current.Row)
                {
                    ColoringCoordinate next = new ColoringCoordinate(row, current.Col);
                    if(web.AddLink(current, next)) InitChain(strategyManager, web, next, number);
                    break;
                }
            }
        }
        
        var ppimn = strategyManager.PossibilityPositionsInMiniGrid(current.Row / 3, current.Col / 3, number);
        if (ppimn.Count == 2)
        {
            foreach (var pos in ppimn)
            {
                if (pos[0] != current.Row && pos[1] != current.Col)
                {
                    ColoringCoordinate next = new ColoringCoordinate(pos[0], pos[1]);
                    if(web.AddLink(current, next)) InitChain(strategyManager, web, next, number);
                    break;
                }
            }
        }
    }

    private static bool DoesAnyChainContains(IEnumerable<ColorableWeb<ColoringCoordinate>> chains, ColoringCoordinate coord)
    {
        foreach (var chain in chains)
        {
            if (chain.Contains(coord)) return true;
        }

        return false;
    }
}

public class SimpleColoringReportWaiter : IChangeReportWaiter
{
    private readonly int _number;
    private readonly ColorableWeb<ColoringCoordinate> _web;

    public SimpleColoringReportWaiter(int number, ColorableWeb<ColoringCoordinate> web)
    {
        _number = number;
        _web = web;
    }

    public ChangeReport Process(List<SolverChange> changes, IChangeManager manager)
    {
        return new ChangeReport(IChangeReportWaiter.ChangesToString(changes), lighter =>
        {
            foreach (var coord in _web)
            {
                lighter.HighlightPossibility(_number, coord.Row, coord.Col, coord.Coloring == Coloring.On ?
                    ChangeColoration.CauseOnOne : ChangeColoration.CauseOffTwo);
            }

            IChangeReportWaiter.HighlightChanges(lighter, changes);
        }, "");
    }
}