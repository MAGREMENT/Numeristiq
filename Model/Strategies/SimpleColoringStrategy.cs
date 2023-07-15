using System.Collections.Generic;
using Model.Strategies.ChainingStrategiesUtil;

namespace Model.Strategies;

public class SimpleColoringStrategy : IStrategy
{
    public void ApplyOnce(ISolver solver)
    {
        for (int number = 1; number <= 9; number++)
        {
            List<ColorableWeb<ColoringCoordinate>> chains = new();
            for (int row = 0; row < 9; row++)
            {
                for (int col = 0; col < 9; col++)
                {
                    if (solver.Possibilities[row, col].Peek(number))
                    {
                        ColoringCoordinate current = new(row, col);
                        if (DoesAnyChainContains(chains, current)) continue;
                        
                        ColorableWeb<ColoringCoordinate> web = new();
                        InitChain(solver, web, current, number);
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
                SearchForTwiceInTheSameUnit(solver, number, chain);
                SearchForTwoColorsElsewhere(solver, number, chain);
            }
        }
    }

    private void SearchForTwiceInTheSameUnit(ISolver solver, int number, ColorableWeb<ColoringCoordinate> web)
    {
        web.ForEachCombinationOfTwo((one, two) =>
        {
            if (one.ShareAUnit(two) && one.Coloring == two.Coloring)
            {
                foreach (var coord in web)
                {
                    if (coord.Coloring == one.Coloring)
                        solver.RemovePossibility(number, coord.Row, coord.Col,
                            new SimpleColoringLog(coord.Row, coord.Col, number, false, true));
                    else
                        solver.AddDefinitiveNumber(number, coord.Row, coord.Col,
                            new SimpleColoringLog(coord.Row, coord.Col, number, true, true));
                }
            }

            return false;
        });
    }

    private void SearchForTwoColorsElsewhere(ISolver solver, int number, ColorableWeb<ColoringCoordinate> web)
    {
        for (int row = 0; row < 9; row++)
        {
            for (int col = 0; col < 9; col++)
            {
                if (solver.Possibilities[row, col].Peek(number))
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
                                solver.RemovePossibility(number, row, col,
                                    new SimpleColoringLog(row, col, number, false, false));
                                break;
                            }
                        }
                    }
                }
            }
        }
    }

    private void InitChain(ISolver solver, ColorableWeb<ColoringCoordinate> web, ColoringCoordinate current, int number)
    {
        var ppir = solver.PossibilityPositionsInRow(current.Row, number);
        if (ppir.Count == 2)
        {
            foreach (var col in ppir)
            {
                if (col != current.Col)
                {
                    ColoringCoordinate next = new ColoringCoordinate(current.Row, col);
                    if(web.AddLink(current, next)) InitChain(solver, web, next, number);
                    break;
                }
            }
        }
        
        var ppic = solver.PossibilityPositionsInColumn(current.Col, number);
        if (ppic.Count == 2)
        {
            foreach (var row in ppic)
            {
                if (row != current.Row)
                {
                    ColoringCoordinate next = new ColoringCoordinate(row, current.Col);
                    if(web.AddLink(current, next)) InitChain(solver, web, next, number);
                    break;
                }
            }
        }
        
        var ppimn = solver.PossibilityPositionsInMiniGrid(current.Row / 3, current.Col / 3, number);
        if (ppimn.Count == 2)
        {
            foreach (var pos in ppimn)
            {
                if (pos[0] != current.Row && pos[1] != current.Col)
                {
                    ColoringCoordinate next = new ColoringCoordinate(pos[0], pos[1]);
                    if(web.AddLink(current, next)) InitChain(solver, web, next, number);
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

public class SimpleColoringLog : ISolverLog
{
    public string AsString { get; }
    public StrategyLevel Level { get; } = StrategyLevel.Hard;

    public SimpleColoringLog(int row, int col, int number, bool added, bool twiceInUnitRule)
    {
        string rule = twiceInUnitRule ? "Twice in unit rule" : "Two elsewhere rule";
        string action = added ? "added as definitive number" : "removed from possibilities";
        AsString = $"[{row + 1}, {col + 1}] {number} {action} because of simple coloring " +
                   $": {rule}";
    }
}