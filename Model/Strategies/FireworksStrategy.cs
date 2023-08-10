using System.Collections.Generic;
using Model.StrategiesUtil;

namespace Model.Strategies;

public class FireworksStrategy : IStrategy
{
    public string Name => "Fireworks";
    
    public StrategyLevel Difficulty => StrategyLevel.Hard;
    public int Score { get; set; }

    public void ApplyOnce(IStrategyManager strategyManager)
    {

        //List<Firework> fireworks = new();
        for (int row = 0; row < 9; row++)
        {
            for (int col = 0; col < 9; col++)
            {
                List<Firework> cellFireworks = new();
                foreach (var possibility in strategyManager.Possibilities[row, col])
                {
                    AddIfFirework(strategyManager, cellFireworks, row, col, possibility);
                }

                if (cellFireworks.Count >= 3)
                {
                    for (int i = 0; i < cellFireworks.Count; i++)
                    {
                        for (int j = i + 1; j < cellFireworks.Count; j++)
                        {
                            for (int k = j + 1; k < cellFireworks.Count; k++)
                            {
                                var sharedWings = cellFireworks[i].MashWings(cellFireworks[j], cellFireworks[k]);
                                if (sharedWings.Count == 2)
                                    ProcessTripleFirework(strategyManager, sharedWings,
                                        cellFireworks[i], cellFireworks[j],
                                        cellFireworks[k]);
                            }
                        }
                    }
                }
                
                //fireworks.AddRange(cellFireworks);
            }
        }
        
        //TODO other elimintations
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
        return new ChangeReport(IChangeReportBuilder.ChangesToString(changes), lighter =>
        {
            int color = (int) ChangeColoration.CauseOffOne;
            foreach (var firework in _fireworks)
            {
                lighter.HighlightPossibility(firework.Possibility, firework.Cross.Row,
                    firework.Cross.Col, (ChangeColoration) color);
                foreach (var coord in firework.Wings)
                {
                    lighter.HighlightPossibility(firework.Possibility, coord.Row, coord.Col, (ChangeColoration) color);
                }

                color++;
            }

            IChangeReportBuilder.HighlightChanges(lighter, changes);
        }, "");
    }
}