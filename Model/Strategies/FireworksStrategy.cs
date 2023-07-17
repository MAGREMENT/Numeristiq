using System.Collections.Generic;
using System.Linq;
using Model.Strategies.StrategiesUtil;

namespace Model.Strategies;

public class FireworksStrategy : IStrategy
{
    public void ApplyOnce(ISolver solver)
    {

        List<Firework> fireworks = new();
        for (int row = 0; row < 9; row++)
        {
            for (int col = 0; col < 9; col++)
            {
                List<Firework> cellFireworks = new();
                foreach (var possibility in solver.Possibilities[row, col])
                {
                    AddIfFirework(solver, cellFireworks, row, col, possibility);
                }

                if (cellFireworks.Count == 3)
                {
                    var sharedWings = cellFireworks[0].MashWings(cellFireworks[1], cellFireworks[2]);
                    if (sharedWings.Count == 2)
                        ProcessTripleFirework(solver, row, col, sharedWings,
                            cellFireworks[0].Possibility, cellFireworks[1].Possibility,
                            cellFireworks[2].Possibility);
                }
                
                fireworks.AddRange(cellFireworks);
            }
        }
        
        //TODO other elimintations
    }

    private void ProcessTripleFirework(ISolver solver, int crossRow, int crossCol, IEnumerable<Coordinate> wings,
        params int[] possibilities)
    {
        foreach (var possibility in solver.Possibilities[crossRow, crossCol])
        {
            if (!possibilities.Contains(possibility)) solver.RemovePossibility(possibility, crossRow, crossCol);
        }

        foreach (var wing in wings)
        {
            foreach (var possibility in solver.Possibilities[wing.Row, wing.Col])
            {
                if (!possibilities.Contains(possibility)) solver.RemovePossibility(possibility, wing.Row, wing.Col);
            }
        }
    }

    private void AddIfFirework(ISolver solver, List<Firework> fireworks, int row, int col, int possibility)
    {
        int miniRow = row / 3;
        int miniCol = col / 3;

        int rowCol = -1;
        int colRow = -1;
        
        //Check row
        for (int c = 0; c < 9; c++)
        {
            if (c / 3 != miniCol && solver.Possibilities[row, c].Peek(possibility))
            {
                if (rowCol == -1) rowCol = c;
                else return;
            }
        }
        
        //Check col
        for (int r = 0; r < 9; r++)
        {
            if (r / 3 != miniRow && solver.Possibilities[r, col].Peek(possibility))
            {
                if (colRow == -1) colRow = col;
                else return;
            }
        }

        if (rowCol != -1)
        {
            if (colRow != -1) fireworks.Add(new Firework(possibility, row, col,
                    row, rowCol, col, colRow));
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

    private readonly int _crossRow;
    private readonly int _crossCol;

    private readonly HashSet<Coordinate> _wings = new();

    public Firework(int possibility, int crossRow, int crossCol, int wingRow, int wingCol)
    {
        Possibility = possibility;
        _crossRow = crossRow;
        _crossCol = crossCol;
        _wings.Add(new Coordinate(wingRow, wingCol));
    }
    
    public Firework(int possibility, int crossRow, int crossCol, int wingRow1, int wingCol1, int wingRow2, int wingCol2)
    {
        Possibility = possibility;
        _crossRow = crossRow;
        _crossCol = crossCol;
        _wings.Add(new Coordinate(wingRow1, wingCol1));
        _wings.Add(new Coordinate(wingRow2, wingCol2));
    }

    public HashSet<Coordinate> MashWings(IEnumerable<Firework> fireworks)
    {
        HashSet<Coordinate> result = new(_wings);
        foreach (var firework in fireworks)
        {
            foreach (var wing in firework._wings)
            {
                result.Add(wing);
            }
        }

        return result;
    }
    
    public HashSet<Coordinate> MashWings(params Firework[] fireworks)
    {
        HashSet<Coordinate> result = new(_wings);
        foreach (var firework in fireworks)
        {
            foreach (var wing in firework._wings)
            {
                result.Add(wing);
            }
        }

        return result;
    }

}