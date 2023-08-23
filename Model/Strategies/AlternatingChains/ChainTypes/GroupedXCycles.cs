using System.Collections.Generic;
using Model.Positions;
using Model.StrategiesUtil;
using Model.StrategiesUtil.LoopFinder;

namespace Model.Strategies.AlternatingChains.ChainTypes;

public class GroupedXCycles : IAlternatingChainType<ILinkGraphElement>
{
    public string Name => "XCycles";
    public StrategyLevel Difficulty => StrategyLevel.Hard;
    public IStrategy? Strategy { get; set; }
    
    public IEnumerable<LinkGraph<ILinkGraphElement>> GetGraphs(IStrategyManager view)
    {
        for (int n = 1; n <= 9; n++)
        {
            LinkGraph<ILinkGraphElement> graph = new();
            int number = n;

            for (int row = 0; row < 9; row++)
            {
                var ppir = view.PossibilityPositionsInRow(row, n);
                var strength = ppir.Count == 2 ? LinkStrength.Strong : LinkStrength.Weak;
                var rowFinal = row;
                ppir.ForEachCombination((one, two) =>
                {
                    graph.AddLink(new PossibilityCoordinate(rowFinal, one, number),
                        new PossibilityCoordinate(rowFinal, two, number), strength);
                });
            }

            for (int col = 0; col < 9; col++)
            {
                var ppic = view.PossibilityPositionsInColumn(col, n);
                var strength = ppic.Count == 2 ? LinkStrength.Strong : LinkStrength.Weak;
                var colFinal = col;
                ppic.ForEachCombination((one, two) =>
                {
                    graph.AddLink(new PossibilityCoordinate(one, colFinal, number),
                        new PossibilityCoordinate(two, colFinal, number), strength);
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
                        graph.AddLink(new PossibilityCoordinate(one.Row, one.Col, number),
                            new PossibilityCoordinate(two.Row, two.Col, number), strength);
                    });
                    SearchForPointingInMiniGrid(view, graph, ppimn, miniRow, miniCol, n);
                }
            }
            
            yield return graph;
        }
    }

    private void SearchForPointingInMiniGrid(IStrategyManager view, LinkGraph<ILinkGraphElement> graph, MiniGridPositions ppimn, int miniRow,
        int miniCol, int numba)
    {
        for (int gridRow = 0; gridRow < 3; gridRow++)
        {
            var colPos = ppimn.OnGridRow(gridRow);
            if (colPos.Count > 1)
            {
                List<PossibilityCoordinate> singles = new();
                List<PointingColumn> pcs = new();
                for (int gridCol = 0; gridCol < 3; gridCol++)
                {
                    int buffer = -1;
                    for (int a = 0; a < 3; a++)
                    {
                        if (a == gridRow) continue;
                        if (ppimn.PeekFromGridPositions(a, gridCol))
                        {
                            singles.Add(new PossibilityCoordinate(miniRow * 3 + a, miniCol * 3 + gridCol, numba));
                            if (buffer == -1) buffer = a;
                            else pcs.Add(new PointingColumn(numba, miniCol * 3 + gridCol,
                                miniRow * 3 + a, miniRow * 3 + buffer));
                        }
                    }
                }

                var singleStrength = singles.Count == 1 ? LinkStrength.Strong : LinkStrength.Weak;
                var pcsStrength = pcs.Count == 1 && singles.Count == pcs[0].Count ? LinkStrength.Strong : LinkStrength.Weak;
                var current = new PointingRow(numba, miniRow * 3 + gridRow, colPos);

                foreach (var single in singles)
                {
                    graph.AddLink(current, single, singleStrength);
                }

                foreach (var pc in pcs)
                {
                    graph.AddLink(current, pc, pcsStrength);
                }
                
                singles.Clear();
                var prs = new List<PointingRow>();

                for (int miniCol2 = 0; miniCol2 < 3; miniCol2++)
                {
                    if (miniCol == miniCol2) continue;

                    List<PossibilityCoordinate> aligned = new();
                    for (int gridCol = 0; gridCol < 3; gridCol++)
                    {
                        int row = miniRow * 3 + gridRow;
                        int col = miniCol2 * 3 + gridCol;

                        if (view.Possibilities[row, col].Peek(numba)) aligned.Add(new PossibilityCoordinate(row, col, numba));
                    }
                    
                    singles.AddRange(aligned);
                    if(aligned.Count > 1) prs.Add(new PointingRow(numba, aligned));
                }
                
                singleStrength = singles.Count == 1 ? LinkStrength.Strong : LinkStrength.Weak;
                var prsStrength = prs.Count == 1 && singles.Count == prs[0].Count ? LinkStrength.Strong : LinkStrength.Weak;

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
            var rowPos = ppimn.OnGridColumn(gridCol);
            if (rowPos.Count > 1)
            {
                List<PossibilityCoordinate> singles = new();
                List<PointingRow> prs = new();
                for (int gridRow = 0; gridRow < 3; gridRow++)
                {
                    int buffer = -1;
                    for (int a = 0; a < 3; a++)
                    {
                        if (a == gridCol) continue;
                        if (ppimn.PeekFromGridPositions(gridRow, a))
                        {
                            singles.Add(new PossibilityCoordinate(miniRow * 3 + gridRow, miniCol * 3 + a, numba));
                            if (buffer == -1) buffer = a;
                            else prs.Add(new PointingRow(numba, miniRow * 3 + gridRow,
                                miniCol * 3 + a, miniCol * 3 + buffer));
                        }
                    }
                }

                var singleStrength = singles.Count == 1 ? LinkStrength.Strong : LinkStrength.Weak;
                var prsStrength = prs.Count == 1 && singles.Count == prs[0].Count ? LinkStrength.Strong : LinkStrength.Weak;
                var current = new PointingColumn(numba, miniCol * 3 + gridCol, rowPos);

                foreach (var single in singles)
                {
                    graph.AddLink(current, single, singleStrength);
                }

                foreach (var pc in prs)
                {
                    graph.AddLink(current, pc, prsStrength);
                }
                
                singles.Clear();
                var pcs = new List<PointingColumn>();

                for (int miniRow2 = 0; miniRow2 < 3; miniRow2++)
                {
                    if (miniRow == miniRow2) continue;

                    List<PossibilityCoordinate> aligned = new();
                    for (int gridRow = 0; gridRow < 3; gridRow++)
                    {
                        int row = miniRow2 * 3 + gridRow;
                        int col = miniCol * 3 + gridCol;

                        if (view.Possibilities[row, col].Peek(numba)) aligned.Add(new PossibilityCoordinate(row, col, numba));
                    }
                    
                    singles.AddRange(aligned);
                    if(aligned.Count > 1) pcs.Add(new PointingColumn(numba, aligned));
                }
                
                singleStrength = singles.Count == 1 ? LinkStrength.Strong : LinkStrength.Weak;
                var pcsStrength = pcs.Count == 1 && singles.Count == pcs[0].Count ? LinkStrength.Strong : LinkStrength.Weak;

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

    public bool ProcessFullLoop(IStrategyManager view, Loop<ILinkGraphElement> loop)
    {
        loop.ForEachLink((one, two)
            => ProcessWeakLink(view, one, two), LinkStrength.Weak);

        return view.ChangeBuffer.Push(Strategy!, new AlternatingChainReportBuilder<ILinkGraphElement>(loop));
    }
    
    //TODO make this bullshit better for the eyes
    private void ProcessWeakLink(IStrategyManager view, ILinkGraphElement one, ILinkGraphElement two)
    {
        switch (one)
        {
            case PointingRow rOne when two is PointingRow rTwo :
                RemovePossibilityInAll(view, rOne.SharedSeenCells(rTwo));
                break;
            case PointingRow rOne when two is PossibilityCoordinate sTwo :
                RemovePossibilityInAll(view, rOne.SharedSeenCells(sTwo));
                break;
            case PossibilityCoordinate sOne when two is PointingRow rTwo :
                RemovePossibilityInAll(view, rTwo.SharedSeenCells(sOne));
                break;
            case PossibilityCoordinate sOne when two is PossibilityCoordinate sTwo :
                RemovePossibilityInAll(view, sOne.SharedSeenCells(sTwo), sOne.Possibility);
                break;
            case PossibilityCoordinate sOne when two is PointingColumn cTwo :
                RemovePossibilityInAll(view, cTwo.SharedSeenCells(sOne));
                break;
            case PointingColumn cOne when two is PointingColumn cTwo :
                RemovePossibilityInAll(view, cOne.SharedSeenCells(cTwo));
                break;
            case PointingColumn cOne when two is PossibilityCoordinate sTwo :
                RemovePossibilityInAll(view, cOne.SharedSeenCells(sTwo));
                break;
        }
    }

    private void RemovePossibilityInAll(IStrategyManager view, IEnumerable<PossibilityCoordinate> coords)
    {
        foreach (var coord in coords)
        {
            view.ChangeBuffer.AddPossibilityToRemove(coord.Possibility, coord.Row, coord.Col);
        }
    }
    
    private void RemovePossibilityInAll(IStrategyManager view, IEnumerable<Coordinate> coords, int possibility)
    {
        foreach (var coord in coords)
        {
            view.ChangeBuffer.AddPossibilityToRemove(possibility, coord.Row, coord.Col);
        }
    }

    public bool ProcessWeakInference(IStrategyManager view, ILinkGraphElement inference, Loop<ILinkGraphElement> loop)
    {
        if (inference is not PossibilityCoordinate single) return false;
        view.ChangeBuffer.AddPossibilityToRemove(single.Possibility, single.Row, single.Col);

        return view.ChangeBuffer.Push(Strategy!, new AlternatingChainReportBuilder<ILinkGraphElement>(loop));
    }

    public bool ProcessStrongInference(IStrategyManager view, ILinkGraphElement inference, Loop<ILinkGraphElement> loop)
    {
        if (inference is not PossibilityCoordinate single) return false;
        view.ChangeBuffer.AddDefinitiveToAdd(single.Possibility, single.Row, single.Col);
        
        return view.ChangeBuffer.Push(Strategy!, new AlternatingChainReportBuilder<ILinkGraphElement>(loop));
    }
}