using System.Collections.Generic;
using System.Linq;
using Model.Changes;
using Model.Solver;
using Model.StrategiesUtil;

namespace Model.Strategies;

public class FireworksStrategy : IStrategy //TODO fixme
{
    public string Name => "Fireworks";
    
    public StrategyLevel Difficulty => StrategyLevel.Hard;
    public StatisticsTracker Tracker { get; } = new();

    public void ApplyOnce(IStrategyManager strategyManager)
    {

        List<Firework[]> looseDoubles = new();
        List<Firework[]> strictDoubles = new();
        for (int row = 0; row < 9; row++)
        {
            for (int col = 0; col < 9; col++)
            {
                List<Firework> cellFireworks = new();
                foreach (var possibility in strategyManager.Possibilities[row, col])
                {
                    AddIfFirework(strategyManager, cellFireworks, row, col, possibility);
                }

                if (cellFireworks.Count < 2) continue;
                
                for (int i = 0; i < cellFireworks.Count; i++)
                {
                    for (int j = i + 1; j < cellFireworks.Count; j++)
                    {
                        var sharedWings = cellFireworks[i].MashWings(cellFireworks[j]);

                        if (sharedWings.Count <= 2)
                        {
                            var dual = new[] { cellFireworks[i], cellFireworks[j] };
                            
                            looseDoubles.Add(dual);
                            if(cellFireworks[i].Wings.Length == 2 && cellFireworks[j].Wings.Length == 2)
                                strictDoubles.Add(dual);
                        }
                        else continue;
                            
                        //Triple
                        for (int k = j + 1; k < cellFireworks.Count; k++)
                        {
                            cellFireworks[k].MashWings(ref sharedWings);
                            if (sharedWings.Count == 2)
                                ProcessTripleFirework(strategyManager, sharedWings,
                                    cellFireworks[i], cellFireworks[j],
                                    cellFireworks[k]);
                        }
                    }
                }
            }
        }

        //Quad
        for (int i = 0; i < looseDoubles.Count; i++)
        {
            for (int j = i + 1; j < looseDoubles.Count; j++)
            {
                var one = looseDoubles[i];
                var two = looseDoubles[j];

                if (one[0].Cross.Row == two[0].Cross.Row || one[0].Cross.Col == two[0].Cross.Col ||
                    one[0].Possibility == two[0].Possibility || one[0].Possibility == two[1].Possibility ||
                    one[1].Possibility == two[0].Possibility || one[1].Possibility == two[1].Possibility) continue;

                var sharedWings = one[0].MashWings(one[1], two[0], two[1]);
                if (sharedWings.Count == 2)
                {
                    RemoveAllExcept(strategyManager, one[0].Cross, one[0].Possibility, one[1].Possibility);
                    RemoveAllExcept(strategyManager, two[0].Cross, two[0].Possibility, two[1].Possibility);

                    foreach (var coord in sharedWings)
                    {
                        RemoveAllExcept(strategyManager, coord, one[0].Possibility, one[1].Possibility,
                            two[0].Possibility, two[1].Possibility);
                    }
                    
                    strategyManager.ChangeBuffer.Push(this, new FireworksReportBuilder(one[0], one[1], two[0], two[1]));
                }
            }
        }
        
        foreach (var dual in strictDoubles)
        {
            int oppositeRow = dual[0].Wings[0].Row == dual[0].Cross.Row ? dual[0].Wings[1].Row : dual[0].Wings[0].Row;
            int oppositeCol = dual[0].Wings[0].Col == dual[0].Cross.Col ? dual[0].Wings[1].Col : dual[0].Wings[0].Col;
            if(!strategyManager.Possibilities[oppositeRow, oppositeCol].Peek(dual[0].Possibility) &&
               !strategyManager.Possibilities[oppositeRow, oppositeCol].Peek(dual[1].Possibility)) continue;
            
            //W-wing
            List<AlmostLockedSet>[] als = new List<AlmostLockedSet>[2];
            bool nah = false;
            for (int i = 0; i < 2; i++)
            {
                var coord = dual[0].Wings[i];

                als[i] = AlmostLockedSet.SearchForAls(strategyManager, CoordinatesToSearch(dual[0].Cross,
                    coord, new Coordinate(oppositeRow, oppositeCol)), 4);

                als[i].RemoveAll(singleAls => !singleAls.Possibilities.Peek(dual[0].Possibility) ||
                                              !singleAls.Possibilities.Peek(dual[1].Possibility));

                if (als[i].Count == 0)
                {
                    nah = true;
                    break;
                }
            }
            
            if(nah) continue;

            strategyManager.ChangeBuffer.AddPossibilityToRemove(dual[0].Possibility, oppositeRow, oppositeCol);
            strategyManager.ChangeBuffer.AddPossibilityToRemove(dual[1].Possibility, oppositeRow, oppositeCol);
            strategyManager.ChangeBuffer.Push(this, new FireworksWithAlsReportBuilder(dual,
                als[0][0], als[1][0]));
        }

        
        foreach (var dual in strictDoubles)
        {
            int oppositeRow = dual[0].Wings[0].Row == dual[0].Cross.Row ? dual[0].Wings[1].Row : dual[0].Wings[0].Row;
            int oppositeCol = dual[0].Wings[0].Col == dual[0].Cross.Col ? dual[0].Wings[1].Col : dual[0].Wings[0].Col;

            foreach (var possibility in strategyManager.Possibilities[oppositeRow, oppositeCol])
            {
                if(possibility == dual[0].Possibility || possibility == dual[1].Possibility) continue;

                if (strategyManager.Possibilities[oppositeRow, dual[0].Cross.Col].Peek(possibility) &&
                    strategyManager.RowPositions(oppositeRow, possibility).Count == 2)
                {
                    strategyManager.ChangeBuffer.AddPossibilityToRemove(possibility, dual[0].Cross.Row, oppositeCol);
                    if (strategyManager.ChangeBuffer.NotEmpty())
                        strategyManager.ChangeBuffer.Push(this, new FireworksWithStrongLinkReportBuilder(dual, possibility,
                                new Coordinate(oppositeRow, oppositeCol),
                                new Coordinate(oppositeRow, dual[0].Cross.Col)));
                }

                if (strategyManager.Possibilities[dual[0].Cross.Row, oppositeCol].Peek(possibility) &&
                    strategyManager.ColumnPositions(oppositeCol, possibility).Count == 2)
                {
                    strategyManager.ChangeBuffer.AddPossibilityToRemove(possibility, oppositeRow, dual[0].Cross.Col);
                    if (strategyManager.ChangeBuffer.NotEmpty())
                        strategyManager.ChangeBuffer.Push(this, new FireworksWithStrongLinkReportBuilder(dual, possibility,
                            new Coordinate(oppositeRow, oppositeCol),
                            new Coordinate(dual[0].Cross.Row, oppositeCol)));
                }
                    
            }
        }

        //TODO other elimintations
    }

    private List<Coordinate> CoordinatesToSearch(Coordinate cross, Coordinate wing, Coordinate opposite)
    {
        List<Coordinate> result = new();
        bool sameRow = cross.Row == wing.Row;

        for (int i = 0; i < 9; i++)
        {
            result.Add(sameRow ? new Coordinate(i, wing.Col) : new Coordinate(wing.Row, i));
        }

        result.Remove(wing);
        result.Remove(opposite);

        return result;
    }

    private void RemoveAllExcept(IStrategyManager strategyManager, Coordinate coord, params int[] except)
    {
        foreach (var possibility in strategyManager.Possibilities[coord.Row, coord.Col])
        {
            if(except.Contains(possibility)) continue;
            strategyManager.ChangeBuffer.AddPossibilityToRemove(possibility, coord.Row, coord.Col);
        }
    }

    private void ProcessTripleFirework(IStrategyManager strategyManager, HashSet<Coordinate> wings,
        Firework one, Firework two, Firework three)
    {
        foreach (var possibility in strategyManager.Possibilities[one.Cross.Row, one.Cross.Col])
        {
            if (possibility != one.Possibility && possibility != two.Possibility && possibility != three.Possibility)
                strategyManager.ChangeBuffer.AddPossibilityToRemove(possibility, one.Cross.Row, one.Cross.Col);
        }

        foreach (var wing in wings)
        {
            foreach (var possibility in strategyManager.Possibilities[wing.Row, wing.Col])
            {
                if (possibility != one.Possibility && possibility != two.Possibility && possibility != three.Possibility) 
                    strategyManager.ChangeBuffer.AddPossibilityToRemove(possibility, wing.Row, wing.Col);
            }
        }

        strategyManager.ChangeBuffer.Push(this, new FireworksReportBuilder(one, two, three));
    }

    private void AddIfFirework(IStrategyManager strategyManager, List<Firework> fireworks, int row, int col, int possibility)
    {
        int miniRow = row / 3;
        int miniCol = col / 3;

        int rowCol = -1;
        int colRow = -1;
        
        //Check row
        for (int c = 0; c < 9; c++)
        {
            if (c / 3 != miniCol && strategyManager.Possibilities[row, c].Peek(possibility))
            {
                if (rowCol == -1) rowCol = c;
                else return;
            }
        }
        
        //Check col
        for (int r = 0; r < 9; r++)
        {
            if (r / 3 != miniRow && strategyManager.Possibilities[r, col].Peek(possibility))
            {
                if (colRow == -1) colRow = r;
                else return;
            }
        }

        if (rowCol != -1)
        {
            if (colRow != -1) fireworks.Add(new Firework(possibility, row, col,
                    row, rowCol, colRow, col));
            else fireworks.Add(new Firework(possibility, row, col,
                row, rowCol));
        }
        else if (colRow != -1) fireworks.Add(new Firework(possibility, row, col,
            colRow, col));
    }
}

public class Firework
{
    public int Possibility { get; }
    public Coordinate Cross { get; }
    public Coordinate[] Wings { get; }

    public Firework(int possibility, int crossRow, int crossCol, int wingRow, int wingCol)
    {
        Possibility = possibility;
        Cross = new Coordinate(crossRow, crossCol);
        Wings = new Coordinate[] { new(wingRow, wingCol) };
    }
    
    public Firework(int possibility, int crossRow, int crossCol, int wingRow1, int wingCol1, int wingRow2, int wingCol2)
    {
        Possibility = possibility;
        Cross = new Coordinate(crossRow, crossCol);
        Wings = new Coordinate[] { new(wingRow1, wingCol1), new(wingRow2, wingCol2) };
    }

    public HashSet<Coordinate> MashWings(params Firework[] fireworks)
    {
        HashSet<Coordinate> result = new(Wings);
        foreach (var firework in fireworks)
        {
            foreach (var wing in firework.Wings)
            {
                result.Add(wing);
            }
        }

        return result;
    }

    public void MashWings(ref HashSet<Coordinate> wings)
    {
        foreach (var wing in Wings)
        {
            wings.Add(wing);
        }
    }
}

public static class FireworksReportBuilderHelper
{
    public static int HighlightFirework(IHighlightable lighter, Firework[] fireworks)
    {
        int color = (int) ChangeColoration.CauseOffOne;
        foreach (var firework in fireworks)
        {
            lighter.HighlightPossibility(firework.Possibility, firework.Cross.Row,
                firework.Cross.Col, (ChangeColoration) color);
            foreach (var coord in firework.Wings)
            {
                lighter.HighlightPossibility(firework.Possibility, coord.Row, coord.Col, (ChangeColoration) color);
            }

            color++;
        }

        return color;
    }
}

public class FireworksReportBuilder : IChangeReportBuilder
{
    private readonly Firework[] _fireworks;

    public FireworksReportBuilder(params Firework[] fireworks)
    {
        _fireworks = fireworks;
    }

    public ChangeReport Build(List<SolverChange> changes, IChangeManager manager)
    {
        return new ChangeReport(IChangeReportBuilder.ChangesToString(changes), "", lighter =>
        {
            FireworksReportBuilderHelper.HighlightFirework(lighter, _fireworks);

            IChangeReportBuilder.HighlightChanges(lighter, changes);
        });
    }
}

public class FireworksWithAlsReportBuilder : IChangeReportBuilder
{
    private readonly Firework[] _fireworks;
    private readonly AlmostLockedSet[] _als;

    public FireworksWithAlsReportBuilder(Firework[] fireworks, params AlmostLockedSet[] als)
    {
        _fireworks = fireworks;
        _als = als;
    }

    public ChangeReport Build(List<SolverChange> changes, IChangeManager manager)
    {
        return new ChangeReport(IChangeReportBuilder.ChangesToString(changes), "", lighter =>
        {
            int color = FireworksReportBuilderHelper.HighlightFirework(lighter, _fireworks) + 1;

            foreach (var als in _als)
            {
                var coloration = (ChangeColoration)color;

                foreach (var coord in als.Coordinates)
                {
                    lighter.HighlightCell(coord, coloration);
                }

                color++;
            }

            IChangeReportBuilder.HighlightChanges(lighter, changes);
        });
    }
}

public class FireworksWithStrongLinkReportBuilder : IChangeReportBuilder
{
    private readonly Firework[] _fireworks;
    private readonly int _possibility;
    private readonly Coordinate _opposite;
    private readonly Coordinate _wing;

    public FireworksWithStrongLinkReportBuilder(Firework[] fireworks, int possibility, Coordinate opposite, Coordinate wing)
    {
        _fireworks = fireworks;
        _possibility = possibility;
        _opposite = opposite;
        _wing = wing;
    }

    public ChangeReport Build(List<SolverChange> changes, IChangeManager manager)
    {
        return new ChangeReport(IChangeReportBuilder.ChangesToString(changes), "", lighter =>
        {
            FireworksReportBuilderHelper.HighlightFirework(lighter, _fireworks);
            
            lighter.HighlightPossibility(_possibility, _opposite.Row, _opposite.Col, ChangeColoration.CauseOnOne);
            lighter.HighlightPossibility(_possibility, _wing.Row, _wing.Col, ChangeColoration.CauseOnOne);

            IChangeReportBuilder.HighlightChanges(lighter, changes);
        });
    }
}