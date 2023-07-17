using System.Collections.Generic;
using System.Linq;

namespace Model.Strategies.XCycles;

public class XCyclesStrategyV2 : IStrategy
{
    public void ApplyOnce(ISolver solver)
    {
        for (int number = 0; number < 9; number++)
        {
            List<Link> strongLinks = new();
            //Rows
            for (int row = 0; row < 9; row++)
            {
                var poss = solver.PossibilityPositionsInRow(row, number);
                if (poss.Count == 2)
                {
                    var asArray = poss.ToArray();
                    strongLinks.Add(new Link(row, asArray[0], row, asArray[1]));
                }
            }
            
            //Cols //TODO
            for (int row = 0; row < 9; row++)
            {
                var poss = solver.PossibilityPositionsInRow(row, number);
                if (poss.Count == 2)
                {
                    var asArray = poss.ToArray();
                    strongLinks.Add(new Link(row, asArray[0], row, asArray[1]));
                }
            }
            
            //MiniGrids //TODO
            for (int row = 0; row < 9; row++)
            {
                var poss = solver.PossibilityPositionsInRow(row, number);
                if (poss.Count == 2)
                {
                    var asArray = poss.ToArray();
                    strongLinks.Add(new Link(row, asArray[0], row, asArray[1]));
                }
            }
            
            //TODO
        }
    }
}

public class Link
{
    private readonly int _row1;
    private readonly int _col1;
    private readonly int _row2;
    private readonly int _col2;

    public Link(int row1, int col1, int row2, int col2)
    {
        _row1 = row1;
        _col1 = col1;
        _row2 = row2;
        _col2 = col2;
    }
}