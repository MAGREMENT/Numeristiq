using System;
using System.Collections.Generic;
using System.Linq;
using Model.LoopFinder;
using Model.Positions;
using Model.Strategies.AIC;
using Model.StrategiesUtil;
using Model.StrategiesUtil.LoopFinder;

namespace Model.Strategies.AlternatingChains.ChainTypes;

public class GroupedXCycles : IAlternatingChainType<IGroupedXCycleNode>
{
    public string Name => "XCycles";
    public StrategyLevel Difficulty => StrategyLevel.Hard;
    public IStrategy? Strategy { get; set; }
    public IEnumerable<Graph<IGroupedXCycleNode>> GetGraphs(ISolverView view)
    {
        for (int n = 1; n <= 9; n++)
        {
            Graph<IGroupedXCycleNode> graph = new();
            int number = n;

            for (int row = 0; row < 9; row++)
            {
                var ppir = view.PossibilityPositionsInRow(row, n);
                var strength = ppir.Count == 2 ? LinkStrength.Strong : LinkStrength.Weak;
                var rowFinal = row;
                ppir.ForEachCombination((one, two) =>
                {
                    graph.AddLink(new GroupedXCycleSingle(rowFinal, one, number),
                        new GroupedXCycleSingle(rowFinal, two, number), strength);
                });
            }

            for (int col = 0; col < 9; col++)
            {
                var ppic = view.PossibilityPositionsInColumn(col, n);
                var strength = ppic.Count == 2 ? LinkStrength.Strong : LinkStrength.Weak;
                var colFinal = col;
                ppic.ForEachCombination((one, two) =>
                {
                    graph.AddLink(new GroupedXCycleSingle(one, colFinal, number),
                        new GroupedXCycleSingle(two, colFinal, number), strength);
                });
            }

            for (int miniRow = 0; miniRow < 3; miniRow++)
            {
                for (int miniCol = 0; miniCol < 3; miniCol++)
                {
                    var ppimn = view.PossibilityPositionsInMiniGrid(miniRow, miniCol, n);
                    var strength = ppimn.Count == 2 ? LinkStrength.Strong : LinkStrength.Weak;
                    ppimn.ForEachCombination((one, two) =>
                    {
                        graph.AddLink(new GroupedXCycleSingle(one.Row, one.Col, number),
                            new GroupedXCycleSingle(two.Row, two.Col, number), strength);
                    });
                    SearchForPointingInMiniGrid(view, graph, ppimn, miniRow, miniCol, n);
                }
            }
            
            yield return graph;
        }
    }

    private void SearchForPointingInMiniGrid(ISolverView view, Graph<IGroupedXCycleNode> graph, MiniGridPositions ppimn, int miniRow,
        int miniCol, int numba)
    {
        for (int gridRow = 0; gridRow < 3; gridRow++)
        {
            var list = new List<int[]>(ppimn.OnGridRow(gridRow));
            if (list.Count > 1)
            {
                List<GroupedXCycleSingle> singles = new();
                List<GroupedXCyclePointingColumn> pcs = new();
                for (int gridCol = 0; gridCol < 3; gridCol++)
                {
                    int buffer = -1;
                    for (int a = 0; a < 3; a++)
                    {
                        if (a == gridRow) continue;
                        if (ppimn.PeekFromGridPositions(a, gridCol))
                        {
                            singles.Add(new GroupedXCycleSingle(miniRow * 3 + a, miniCol * 3 + gridCol, numba));
                            if (buffer == -1) buffer = a;
                            else pcs.Add(new GroupedXCyclePointingColumn(numba,
                                new Coordinate(miniRow * 3 + a, miniCol * 3 + gridCol),
                                new Coordinate(miniRow * 3 + buffer, miniCol * 3 + gridCol)));
                        }
                    }
                }

                var singleStrength = singles.Count == 1 ? LinkStrength.Strong : LinkStrength.Weak;
                var pcsStrength = pcs.Count == 1 ? LinkStrength.Strong : LinkStrength.Weak;
                var current = new GroupedXCyclePointingRow(numba, list);

                foreach (var single in singles)
                {
                    graph.AddLink(current, single, singleStrength);
                }

                foreach (var pc in pcs)
                {
                    graph.AddLink(current, pc, pcsStrength);
                }
                
                singles.Clear();
                var prs = new List<GroupedXCyclePointingRow>();

                for (int miniCol2 = 0; miniCol2 < 3; miniCol2++)
                {
                    if (miniCol == miniCol2) continue;

                    List<GroupedXCycleSingle> aligned = new();
                    for (int gridCol = 0; gridCol < 3; gridCol++)
                    {
                        int row = miniRow * 3 + gridRow;
                        int col = miniCol2 * 3 + gridCol;

                        if (view.Possibilities[row, col].Peek(numba)) aligned.Add(new GroupedXCycleSingle(row, col, numba));
                    }
                    
                    singles.AddRange(aligned);
                    if(aligned.Count > 1) prs.Add(new GroupedXCyclePointingRow(numba, aligned));
                }
                
                singleStrength = singles.Count == 1 ? LinkStrength.Strong : LinkStrength.Weak;
                var prsStrength = pcs.Count == 1 ? LinkStrength.Strong : LinkStrength.Weak;

                foreach (var single in singles)
                {
                    graph.AddLink(current, single, singleStrength);
                }

                foreach (var pr in prs)
                {
                    graph.AddLink(current, pr, prsStrength);
                }
            }
        }
        
        for (int gridCol = 0; gridCol < 3; gridCol++)
        {
            var list = new List<int[]>(ppimn.OnGridColumn(gridCol));
            if (list.Count > 1)
            {
                List<GroupedXCycleSingle> singles = new();
                List<GroupedXCyclePointingRow> prs = new();
                for (int gridRow = 0; gridRow < 3; gridRow++)
                {
                    int buffer = -1;
                    for (int a = 0; a < 3; a++)
                    {
                        if (a == gridCol) continue;
                        if (ppimn.PeekFromGridPositions(gridRow, a))
                        {
                            singles.Add(new GroupedXCycleSingle(miniRow * 3 + gridRow, miniCol * 3 + a, numba));
                            if (buffer == -1) buffer = a;
                            else prs.Add(new GroupedXCyclePointingRow(numba,
                                new Coordinate(miniRow * 3 + gridRow, miniCol * 3 + a),
                                new Coordinate(miniRow * 3 + gridRow, miniCol * 3 + buffer)));
                        }
                    }
                }

                var singleStrength = singles.Count == 1 ? LinkStrength.Strong : LinkStrength.Weak;
                var prsStrength = prs.Count == 1 ? LinkStrength.Strong : LinkStrength.Weak;
                var current = new GroupedXCyclePointingColumn(numba, list);

                foreach (var single in singles)
                {
                    graph.AddLink(current, single, singleStrength);
                }

                foreach (var pc in prs)
                {
                    graph.AddLink(current, pc, prsStrength);
                }
                
                singles.Clear();
                var pcs = new List<GroupedXCyclePointingColumn>();

                for (int miniRow2 = 0; miniRow2 < 3; miniRow2++)
                {
                    if (miniRow == miniRow2) continue;

                    List<GroupedXCycleSingle> aligned = new();
                    for (int gridRow = 0; gridRow < 3; gridRow++)
                    {
                        int row = miniRow2 * 3 + gridRow;
                        int col = miniCol * 3 + gridCol;

                        if (view.Possibilities[row, col].Peek(numba)) aligned.Add(new GroupedXCycleSingle(row, col, numba));
                    }
                    
                    singles.AddRange(aligned);
                    if(aligned.Count > 1) pcs.Add(new GroupedXCyclePointingColumn(numba, aligned));
                }
                
                singleStrength = singles.Count == 1 ? LinkStrength.Strong : LinkStrength.Weak;
                var pcsStrength = pcs.Count == 1 ? LinkStrength.Strong : LinkStrength.Weak;

                foreach (var single in singles)
                {
                    graph.AddLink(current, single, singleStrength);
                }

                foreach (var pc in pcs)
                {
                    graph.AddLink(current, pc, pcsStrength);
                }
            }
        }
    }

    public bool ProcessFullLoop(ISolverView view, Loop<IGroupedXCycleNode> loop)
    {
        bool wasProgressMade = false;
        loop.ForEachLink((one, two)
            => ProcessWeakLink(view, one, two, out wasProgressMade), LinkStrength.Weak);

        return wasProgressMade;
    }
    
    private void ProcessWeakLink(ISolverView view, IGroupedXCycleNode one, IGroupedXCycleNode two, out bool wasProgressMade)
    {
        if (one is GroupedXCyclePointingRow rOne)
        {
            if (two is GroupedXCyclePointingRow rTwo &&
                RemovePossibilityInAll(view, rOne.SharedSeenCells(rTwo))) wasProgressMade = true;
            else if(two is GroupedXCycleSingle sTwo &&
                    RemovePossibilityInAll(view, rOne.SharedSeenCells(sTwo))) wasProgressMade = true;
        }
        else if (one is GroupedXCycleSingle sOne)
        {
            if (two is GroupedXCyclePointingRow rTwo &&
                RemovePossibilityInAll(view, rTwo.SharedSeenCells(sOne))) wasProgressMade = true;
            else if(two is GroupedXCycleSingle sTwo &&
                    RemovePossibilityInAll(view, sOne.SharedSeenCells(sTwo))) wasProgressMade = true;
            else if (two is GroupedXCyclePointingColumn cTwo &&
                     RemovePossibilityInAll(view, cTwo.SharedSeenCells(sOne))) wasProgressMade = true;
        }
        else if (one is GroupedXCyclePointingColumn cOne)
        {
            if (two is GroupedXCyclePointingColumn cTwo &&
                RemovePossibilityInAll(view, cOne.SharedSeenCells(cTwo))) wasProgressMade = true;
            else if(two is GroupedXCycleSingle sTwo &&
                    RemovePossibilityInAll(view, cOne.SharedSeenCells(sTwo))) wasProgressMade = true;
        }

        wasProgressMade = false;
    }

    private bool RemovePossibilityInAll(ISolverView view, IEnumerable<PossibilityCoordinate> coords)
    {
        bool wasProgressMade = false;
        foreach (var coord in coords)
        {
            if (view.RemovePossibility(coord.Possibility, coord.Row, coord.Col, Strategy!)) wasProgressMade = true;
        }

        return wasProgressMade;
    }

    public bool ProcessWeakInference(ISolverView view, IGroupedXCycleNode inference)
    {
        if (inference is not GroupedXCycleSingle single) return false;
        return view.RemovePossibility(single.Possibility, single.Row, single.Col, Strategy!);
    }

    public bool ProcessStrongInference(ISolverView view, IGroupedXCycleNode inference)
    {
        if (inference is not GroupedXCycleSingle single) return false;
        return view.AddDefinitiveNumber(single.Possibility, single.Row, single.Col, Strategy!);
    }
}

public interface IGroupedXCycleNode
{
    
}

public class GroupedXCycleSingle : PossibilityCoordinate, IGroupedXCycleNode
{
    public GroupedXCycleSingle(int row, int col, int possibility) : base(row, col, possibility)
    {
    }

    public IEnumerable<PossibilityCoordinate> SharedSeenCells(GroupedXCycleSingle s)
    {
        foreach (var coord in base.SharedSeenCells(s))
        {
            yield return new PossibilityCoordinate(coord.Row, coord.Col, Possibility);
        }
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Possibility, Row, Col);
    }

    public override bool Equals(object? obj)
    {
        if (obj is not GroupedXCycleSingle pc) return false;
        return pc.Possibility == Possibility && pc.Row == Row && pc.Col == Col;
    }

    public override string ToString()
    {
        return $"[{Row + 1}, {Col + 1} => {Possibility}]";
    }
}

public class GroupedXCyclePointingRow : IGroupedXCycleNode
{
    public int Possibility { get; }
    public Coordinate[] Coordinates { get; } //TODO make array of int

    public GroupedXCyclePointingRow(int possibility, params Coordinate[] coords)
    {
        Possibility = possibility;
        Coordinates = coords;
    }

    public GroupedXCyclePointingRow(int possibility, IEnumerable<int[]> coords)
    {
        Possibility = possibility;
        var intsEnumerable = coords as int[][] ?? coords.ToArray();
        Coordinates = new Coordinate[intsEnumerable.Count()];
        int cursor = 0;
        foreach (var coord in intsEnumerable)
        {
            Coordinates[cursor] = new Coordinate(coord[0], coord[1]);
            cursor++;
        }
    }
    
    public GroupedXCyclePointingRow(int possibility, IEnumerable<Coordinate> coords)
    {
        Possibility = possibility;
        Coordinates = coords.ToArray();
    }

    public IEnumerable<PossibilityCoordinate> SharedSeenCells(GroupedXCycleSingle single)
    {
        for (int col = 0; col < 9; col++)
        {
            if (col == single.Col || Coordinates.Any(coord => coord.Col == col)) continue;
            yield return new PossibilityCoordinate(Coordinates[0].Row, col, Possibility);
        }
    }
    
    public IEnumerable<PossibilityCoordinate> SharedSeenCells(GroupedXCyclePointingRow row)
    {
        for (int col = 0; col < 9; col++)
        {
            if (row.Coordinates.Any(coord => coord.Col == col) ||
                Coordinates.Any(coord => coord.Col == col)) continue;
            yield return new PossibilityCoordinate(Coordinates[0].Row, col, Possibility);
        }
    }

    public override int GetHashCode()
    {
        int coordsHash = 0;
        foreach (var coord in Coordinates)
        {
            coordsHash ^= coord.GetHashCode();
        }

        return HashCode.Combine(Possibility, coordsHash);
    }

    public override bool Equals(object? obj)
    {
        if (obj is not GroupedXCyclePointingRow pr) return false;
        if (pr.Possibility != Possibility || Coordinates.Length != pr.Coordinates.Length) return false;
        foreach (var coord in Coordinates)
        {
            if (!pr.Coordinates.Contains(coord)) return false;
        }

        return true;
    }
}

public class GroupedXCyclePointingColumn : IGroupedXCycleNode
{
    public int Possibility { get; }
    public Coordinate[] Coordinates { get; }

    public GroupedXCyclePointingColumn(int possibility, params Coordinate[] coords)
    {
        Possibility = possibility;
        Coordinates = coords;
    }
    
    public GroupedXCyclePointingColumn(int possibility, IEnumerable<int[]> coords)
    {
        Possibility = possibility;
        var intsEnumerable = coords as int[][] ?? coords.ToArray();
        Coordinates = new Coordinate[intsEnumerable.Count()];
        int cursor = 0;
        foreach (var coord in intsEnumerable)
        {
            Coordinates[cursor] = new Coordinate(coord[0], coord[1]);
            cursor++;
        }
    }
    
    public GroupedXCyclePointingColumn(int possibility, IEnumerable<Coordinate> coords)
    {
        Possibility = possibility;
        Coordinates = coords.ToArray();
    }
    
    public IEnumerable<PossibilityCoordinate> SharedSeenCells(GroupedXCycleSingle single)
    {
        for (int row = 0; row < 9; row++)
        {
            if (row == single.Row || Coordinates.Any(coord => coord.Row == row)) continue;
            yield return new PossibilityCoordinate(row, Coordinates[0].Col, Possibility);
        }
    }
    
    public IEnumerable<PossibilityCoordinate> SharedSeenCells(GroupedXCyclePointingColumn col)
    {
        for (int row = 0; row < 9; row++)
        {
            if (col.Coordinates.Any(coord => coord.Row == row) ||
                Coordinates.Any(coord => coord.Row == row)) continue;
            yield return new PossibilityCoordinate(row, Coordinates[0].Col, Possibility);
        }
    }

    public override int GetHashCode()
    {
        int coordsHash = 0;
        foreach (var coord in Coordinates)
        {
            coordsHash ^= coord.GetHashCode();
        }

        return HashCode.Combine(Possibility, coordsHash);
    }

    public override bool Equals(object? obj)
    {
        if (obj is not GroupedXCyclePointingColumn pc) return false;
        if (pc.Possibility != Possibility || Coordinates.Length != pc.Coordinates.Length) return false;
        foreach (var coord in Coordinates)
        {
            if (!pc.Coordinates.Contains(coord)) return false;
        }

        return true;
    }
}