using Model.Solver;
using Model.Solver.StrategiesUtil;
using Model.Solver.StrategiesUtil.LinkGraph;

namespace Model.StrategiesUtil.LinkGraph.ConstructRules;

public class AlmostNakedPossibilitiesConstructRule : IConstructRule
{
    public void Apply(LinkGraph<ILinkGraphElement> linkGraph, IStrategyManager strategyManager)
    {
        foreach (var als in strategyManager.AlmostLockedSets())
        {
            if (als.Coordinates.Length is < 2 or > 4) continue;

            CellPossibility buffer = default;
            bool found = false;
            foreach (var possibility in als.Possibilities)
            {
                found = false;
                foreach (var coord in als.Coordinates)
                {
                    if (!strategyManager.PossibilitiesAt(coord.Row, coord.Col).Peek(possibility)) continue;

                    if (!found)
                    {
                        buffer = new CellPossibility(coord.Row, coord.Col, possibility); 
                        found = true;
                    }
                    else
                    {
                        found = false;
                        break;
                    }
                }

                if (found)
                {
                    break;
                }
            }

            if (!found) continue;

            //Almost naked possibility found
            CellPossibilities[] buildUp = new CellPossibilities[als.Coordinates.Length];
            for (int i = 0; i < als.Coordinates.Length; i++)
            {
                buildUp[i] = new CellPossibilities(als.Coordinates[i],
                    strategyManager.PossibilitiesAt(als.Coordinates[i].Row, als.Coordinates[i].Col));
            }

            AlmostNakedPossibilities anp = new AlmostNakedPossibilities(buildUp, buffer);
            linkGraph.AddLink(buffer, anp, LinkStrength.Strong, LinkType.MonoDirectional);

            bool sameRow = true;
            int sharedRow = anp.CoordinatePossibilities[0].Cell.Row;
            bool sameCol = true;
            int sharedCol = anp.CoordinatePossibilities[0].Cell.Col;
            bool sameMini = true;
            int sharedMiniRow = anp.CoordinatePossibilities[0].Cell.Row / 3;
            int sharedMiniCol = anp.CoordinatePossibilities[0].Cell.Col / 3;

            for (int i = 1; i < anp.CoordinatePossibilities.Length; i++)
            {
                if (anp.CoordinatePossibilities[i].Cell.Row != sharedRow) sameRow = false;
                if (anp.CoordinatePossibilities[i].Cell.Col != sharedCol) sameCol = false;
                if (anp.CoordinatePossibilities[i].Cell.Row / 3 != sharedMiniRow ||
                    anp.CoordinatePossibilities[i].Cell.Col / 3 != sharedMiniCol) sameMini = false;
            }

            foreach (var possibility in als.Possibilities)
            {
                if (possibility == buffer.Possibility) continue;
                if (sameRow)
                {
                    for (int col = 0; col < 9; col++)
                    {
                        if (!strategyManager.PossibilitiesAt(sharedRow, col).Peek(possibility)) continue;

                        Cell current = new Cell(sharedRow, col);
                        if (als.Contains(current)) continue;
                        
                        linkGraph.AddLink(anp, new CellPossibility(current.Row, current.Col, possibility),
                            LinkStrength.Weak, LinkType.MonoDirectional);
                    }
                }

                if (sameCol)
                {
                    for (int row = 0; row < 9; row++)
                    {
                        if (!strategyManager.PossibilitiesAt(row, sharedCol).Peek(possibility)) continue;
                        
                        Cell current = new Cell(row, sharedCol);
                        if (als.Contains(current)) continue;
                        
                        linkGraph.AddLink(anp, new CellPossibility(current.Row, current.Col, possibility),
                            LinkStrength.Weak, LinkType.MonoDirectional);
                    }
                }

                if (sameMini)
                {
                    for (int gridRow = 0; gridRow < 3; gridRow++)
                    {
                        for(int gridCol = 0; gridCol < 3; gridCol++)
                        {
                            int row = sharedMiniRow * 3 + gridRow;
                            int col = sharedMiniCol * 3 + gridCol;
                            
                            if (!strategyManager.PossibilitiesAt(row, col).Peek(possibility)) continue;
                        
                            Cell current = new Cell(row, col);
                            if (als.Contains(current)) continue;
                        
                            linkGraph.AddLink(anp, new CellPossibility(current.Row, current.Col, possibility),
                                LinkStrength.Weak, LinkType.MonoDirectional);
                        }
                    }
                }
            }
        }
    }
}