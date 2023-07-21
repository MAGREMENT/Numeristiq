using System.Collections.Generic;
using Model.StrategiesUtil;

namespace Model.Strategies;

public class SimpleColoringStrategy : IStrategy
{
    public string Name { get; } = "Simple coloring";
    
    public StrategyLevel Difficulty { get; } = StrategyLevel.Hard;
    public int Score { get; set; }

    public void ApplyOnce(ISolverView solverView)
    {
        for (int number = 1; number <= 9; number++)
        {
            List<ColorableWeb<ColoringCoordinate>> chains = new();
            for (int row = 0; row < 9; row++)
            {
                for (int col = 0; col < 9; col++)
                {
                    if (solverView.Possibilities[row, col].Peek(number))
                    {
                        ColoringCoordinate current = new(row, col);
                        if (DoesAnyChainContains(chains, current)) continue;
                        
                        ColorableWeb<ColoringCoordinate> web = new();
                        InitChain(solverView, web, current, number);
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
                SearchForTwiceInTheSameUnit(solverView, number, chain);
                SearchForTwoColorsElsewhere(solverView, number, chain);
            }
        }
    }

    private void SearchForTwiceInTheSameUnit(ISolverView solverView, int number, ColorableWeb<ColoringCoordinate> web)
    {
        web.ForEachCombinationOfTwo((one, two) =>
        {
            if (one.ShareAUnit(two) && one.Coloring == two.Coloring)
            {
                foreach (var coord in web)
                {
                    if (coord.Coloring == one.Coloring)
                        solverView.RemovePossibility(number, coord.Row, coord.Col, this);
                    else
                        solverView.AddDefinitiveNumber(number, coord.Row, coord.Col, this);
                }
            }

            return false;
        });
    }

    private void SearchForTwoColorsElsewhere(ISolverView solverView, int number, ColorableWeb<ColoringCoordinate> web)
    {
        for (int row = 0; row < 9; row++)
        {
            for (int col = 0; col < 9; col++)
            {
                if (solverView.Possibilities[row, col].Peek(number))
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
                                solverView.RemovePossibility(number, row, col, this);
                                break;
                            }
                        }
                    }
                }
            }
        }
    }

    private void InitChain(ISolverView solverView, ColorableWeb<ColoringCoordinate> web, ColoringCoordinate current, int number)
    {
        var ppir = solverView.PossibilityPositionsInRow(current.Row, number);
        if (ppir.Count == 2)
        {
            foreach (var col in ppir)
            {
                if (col != current.Col)
                {
                    ColoringCoordinate next = new ColoringCoordinate(current.Row, col);
                    if(web.AddLink(current, next)) InitChain(solverView, web, next, number);
                    break;
                }
            }
        }
        
        var ppic = solverView.PossibilityPositionsInColumn(current.Col, number);
        if (ppic.Count == 2)
        {
            foreach (var row in ppic)
            {
                if (row != current.Row)
                {
                    ColoringCoordinate next = new ColoringCoordinate(row, current.Col);
                    if(web.AddLink(current, next)) InitChain(solverView, web, next, number);
                    break;
                }
            }
        }
        
        var ppimn = solverView.PossibilityPositionsInMiniGrid(current.Row / 3, current.Col / 3, number);
        if (ppimn.Count == 2)
        {
            foreach (var pos in ppimn)
            {
                if (pos[0] != current.Row && pos[1] != current.Col)
                {
                    ColoringCoordinate next = new ColoringCoordinate(pos[0], pos[1]);
                    if(web.AddLink(current, next)) InitChain(solverView, web, next, number);
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