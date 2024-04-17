using System.Collections.Generic;
using Model.Helpers;
using Model.Sudokus.Solver.StrategiesUtility.Graphs;

namespace Model.Sudokus.Solver.StrategiesUtility.Oddagons;

public class AlmostOddagon
{
    public LinkGraphLoop<CellPossibility> Loop { get; }
    public CellPossibility[] Guardians { get; }
    
    public AlmostOddagon(LinkGraphLoop<CellPossibility> loop, CellPossibility[] guardians)
    {
        Loop = loop;
        Guardians = guardians;
    }

    public static AlmostOddagon FromBoard(ISudokuSolvingState holder, LinkGraphLoop<CellPossibility> loop)
    {
        List<CellPossibility> guardians = new();
        loop.ForEachLink((one, two) => guardians.AddRange(
                OddagonSearcher.FindGuardians(holder, one, two)), LinkStrength.Weak);

        return new AlmostOddagon(loop, guardians.ToArray());
    }
}