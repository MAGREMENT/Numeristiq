using System.Collections.Generic;
using Model.Solver;
using Model.StrategiesUtil;
using Model.StrategiesUtil.LinkGraph;
using Model.StrategiesUtil.LoopFinder;

namespace Model.Strategies.AlternatingChains.ChainTypes;

public class NormalXCycles : IAlternatingChainType<CellPossibility>
{
    public string Name => "XCycles";
    public StrategyLevel Difficulty => StrategyLevel.Hard;
    public IStrategy? Strategy { get; set; }
    public IEnumerable<LinkGraph<CellPossibility>> GetGraphs(IStrategyManager view)
    {
        for (int n = 1; n <= 9; n++)
        {
            LinkGraph<CellPossibility> graph = new();
            int number = n;

            for (int row = 0; row < 9; row++)
            {
                var ppir = view.RowPositionsAt(row, n);
                var strength = ppir.Count == 2 ? LinkStrength.Strong : LinkStrength.Weak;
                var rowFinal = row;
                ppir.ForEachCombination((one, two) =>
                {
                    graph.AddLink(new CellPossibility(rowFinal, one, number),
                        new CellPossibility(rowFinal, two, number), strength);
                });
            }

            for (int col = 0; col < 9; col++)
            {
                var ppic = view.ColumnPositionsAt(col, n);
                var strength = ppic.Count == 2 ? LinkStrength.Strong : LinkStrength.Weak;
                var colFinal = col;
                ppic.ForEachCombination((one, two) =>
                {
                    graph.AddLink(new CellPossibility(one, colFinal, number),
                        new CellPossibility(two, colFinal, number), strength);
                });
            }

            for (int miniRow = 0; miniRow < 3; miniRow++)
            {
                for (int miniCol = 0; miniCol < 3; miniCol++)
                {
                    var ppimn = view.MiniGridPositionsAt(miniRow, miniCol, n);
                    var strength = ppimn.Count == 2 ? LinkStrength.Strong : LinkStrength.Weak;
                    ppimn.ForEachCombination((one, two) =>
                    {
                        graph.AddLink(new CellPossibility(one.Row, one.Col, number),
                            new CellPossibility(two.Row, two.Col, number), strength);
                    });
                }
            }
            
            yield return graph;
        }
    }

    public bool ProcessFullLoop(IStrategyManager view, Loop<CellPossibility> loop)
    {
        bool wasProgressMade = false;
        loop.ForEachLink((one, two)
            => ProcessWeakLink(view, one, two, out wasProgressMade), LinkStrength.Weak);

        return wasProgressMade;
    }

    private void ProcessWeakLink(IStrategyManager view, CellPossibility one, CellPossibility two, out bool wasProgressMade)
    {
        foreach (var coord in one.SharedSeenCells(two))
        {
            if (view.RemovePossibility(one.Possibility, coord.Row, coord.Col, Strategy!)) wasProgressMade = true;
        }

        wasProgressMade = false;
    }

    public bool ProcessWeakInference(IStrategyManager view, CellPossibility inference, Loop<CellPossibility> loop)
    {
        return view.RemovePossibility(inference.Possibility, inference.Row, inference.Col, Strategy!);
    }

    public bool ProcessStrongInference(IStrategyManager view, CellPossibility inference, Loop<CellPossibility> loop)
    {
        return view.AddSolution(inference.Possibility, inference.Row, inference.Col, Strategy!);
    }
}