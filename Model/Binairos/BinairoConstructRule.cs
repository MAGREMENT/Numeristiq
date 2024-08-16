using Model.Core.Graphs;
using Model.Utility;

namespace Model.Binairos;

public class BinairoConstructRule : IConstructRule<IBinairoSolverData, CellPossibility>
{
    public static BinairoConstructRule Instance { get; } = new();
    
    private BinairoConstructRule() {}
    
    public int ID { get; } = UniqueConstructRuleID.Next();
    public void Apply(IGraph<CellPossibility, LinkStrength> linkGraph, IBinairoSolverData data)
    {
        int n;
        for (int row = 0; row < data.Binairo.RowCount; row++)
        {
            for (int col = 0; col < data.Binairo.ColumnCount; col++)
            {
                if(data.Binairo[row, col] != 0) continue;

                if (col > 0)
                {
                    //Double
                    n = data.Binairo[row, col - 1];
                    if (n != 0)
                    {
                        if (col - 1 > 0 && data.Binairo[row, col - 2] == 0) linkGraph.Add(new CellPossibility(row, col, n),
                            new CellPossibility(row, col - 2, BinairoUtility.Opposite(n)),
                            LinkStrength.Strong, LinkType.MonoDirectional);
                        if(col < data.ColumnCount - 1 && data.Binairo[row, col + 1] == 0) linkGraph.Add(new CellPossibility(row, col, n),
                            new CellPossibility(row, col + 1, BinairoUtility.Opposite(n)),
                            LinkStrength.Strong, LinkType.MonoDirectional);
                    }
                    
                    //Triple
                    if (col - 1 > 0)
                    {
                        n = data.Binairo[row, col - 2];
                        if(n != 0 && data.Binairo[row, col - 1] == 0) linkGraph.Add(new CellPossibility(row, col, n),
                            new CellPossibility(row, col - 1, BinairoUtility.Opposite(n)),
                            LinkStrength.Strong, LinkType.MonoDirectional);
                    }
                }
                
                
                if (row > 0)
                {
                    //Double
                    n = data.Binairo[row - 1, col];
                    if (n != 0)
                    {
                        if (row - 1 > 0 && data.Binairo[row - 2, col] == 0) linkGraph.Add(new CellPossibility(row, col, n),
                            new CellPossibility(row - 2, col, BinairoUtility.Opposite(n)),
                            LinkStrength.Strong, LinkType.MonoDirectional);
                        if(row < data.RowCount - 1 && data.Binairo[row + 1, col] == 0) linkGraph.Add(new CellPossibility(row, col, n),
                            new CellPossibility(row + 1, col, BinairoUtility.Opposite(n)),
                            LinkStrength.Strong, LinkType.MonoDirectional);
                    }
                    
                    //Triple
                    if (row - 1 > 0)
                    {
                        n = data.Binairo[row - 2, col];
                        if(n != 0 && data.Binairo[row - 1, col] == 0) linkGraph.Add(new CellPossibility(row, col, n),
                            new CellPossibility(row - 1, col, BinairoUtility.Opposite(n)),
                            LinkStrength.Strong, LinkType.MonoDirectional);
                    }
                }

                if (col < data.Binairo.ColumnCount - 1)
                {
                    //Double
                    n = data.Binairo[row, col + 1];
                    if (n != 0)
                    {
                        if (col > 0 && data.Binairo[row, col - 1] == 0) linkGraph.Add(new CellPossibility(row, col, n),
                            new CellPossibility(row, col - 1, BinairoUtility.Opposite(n)),
                            LinkStrength.Strong, LinkType.MonoDirectional);
                        if(col < data.ColumnCount - 2 && data.Binairo[row, col + 2] == 0) linkGraph.Add(new CellPossibility(row, col, n),
                            new CellPossibility(row, col + 2, BinairoUtility.Opposite(n)),
                            LinkStrength.Strong, LinkType.MonoDirectional);
                    }
                    
                    //Triple
                    if (col < data.Binairo.ColumnCount - 2)
                    {
                        n = data.Binairo[row, col + 2];
                        if(n != 0 && data.Binairo[row, col + 1] == 0) linkGraph.Add(new CellPossibility(row, col, n),
                            new CellPossibility(row, col + 1, BinairoUtility.Opposite(n)),
                            LinkStrength.Strong, LinkType.MonoDirectional);
                    }
                }
                
                if (row < data.Binairo.RowCount - 1)
                {
                    //Double
                    n = data.Binairo[row + 1, col];
                    if (n != 0)
                    {
                        if (row > 0 && data.Binairo[row - 1, col] == 0) linkGraph.Add(new CellPossibility(row, col, n),
                            new CellPossibility(row - 1, col, BinairoUtility.Opposite(n)),
                            LinkStrength.Strong, LinkType.MonoDirectional);
                        if(row < data.RowCount - 2 && data.Binairo[row + 2, col] == 0) linkGraph.Add(new CellPossibility(row, col, n),
                            new CellPossibility(row + 2, col, BinairoUtility.Opposite(n)),
                            LinkStrength.Strong, LinkType.MonoDirectional);
                    }
                    
                    //Triple
                    if (row < data.Binairo.RowCount - 2)
                    {
                        n = data.Binairo[row + 2, col];
                        if(n != 0 && data.Binairo[row + 1, col] == 0) linkGraph.Add(new CellPossibility(row, col, n),
                            new CellPossibility(row + 1, col, BinairoUtility.Opposite(n)),
                            LinkStrength.Strong, LinkType.MonoDirectional);
                    }
                }
            }
        }

        var half = data.ColumnCount / 2;
        for (int row = 0; row < data.RowCount; row++)
        {
            var set = data.Binairo.RowSetAt(row);
            if (set.OnesCount != half - 1 || set.TwosCount != half - 1) continue;

            var empty = set.EmptyToList(data.ColumnCount);
            linkGraph.Add(new CellPossibility(row, empty[0], 1),
                new CellPossibility(row, empty[1], 2), LinkStrength.Strong);
            linkGraph.Add(new CellPossibility(row, empty[0], 2),
                new CellPossibility(row, empty[1], 1), LinkStrength.Strong);
        }

        half = data.RowCount / 2;
        for (int col = 0; col < data.ColumnCount; col++)
        {
            var set = data.Binairo.ColumnSetAt(col);
            if (set.OnesCount != half - 1 || set.TwosCount != half - 1) continue;

            var empty = set.EmptyToList(data.ColumnCount);
            linkGraph.Add(new CellPossibility(empty[0], col, 1),
                new CellPossibility(empty[1], col, 2), LinkStrength.Strong);
            linkGraph.Add(new CellPossibility(empty[0], col, 2),
                new CellPossibility(empty[1], col, 1), LinkStrength.Strong);
        }
    }
}