using System.Collections.Generic;
using Model.LoopFinder;
using Model.Strategies.AIC;
using Model.StrategiesUtil;
using Model.StrategiesUtil.LoopFinder;

namespace Model.Strategies.AlternatingChains.ChainTypes;

public class NormalXCycles : IAlternatingChainType<PossibilityCoordinate>
{
    public string Name => "XCycles";
    public StrategyLevel Difficulty => StrategyLevel.Hard;
    public IStrategy? Strategy { get; set; }
    public IEnumerable<Graph<PossibilityCoordinate>> GetGraphs(ISolverView view)
    {
        for (int n = 1; n <= 9; n++)
        {
            Graph<PossibilityCoordinate> graph = new();
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
                }
            }
            
            yield return graph;
        }
    }

    public bool ProcessFullLoop(ISolverView view, Loop<PossibilityCoordinate> loop)
    {
        bool wasProgressMade = false;
        loop.ForEachLink((one, two)
            => ProcessWeakLink(view, one, two, out wasProgressMade), LinkStrength.Weak);

        return wasProgressMade;
    }

    private void ProcessWeakLink(ISolverView view, PossibilityCoordinate one, PossibilityCoordinate two, out bool wasProgressMade)
    {
        foreach (var coord in one.SharedSeenCells(two))
        {
            if (view.RemovePossibility(one.Possibility, coord.Row, coord.Col, Strategy!)) wasProgressMade = true;
        }

        wasProgressMade = false;
    }

    public bool ProcessWeakInference(ISolverView view, PossibilityCoordinate inference)
    {
        return view.RemovePossibility(inference.Possibility, inference.Row, inference.Col, Strategy!);
    }

    public bool ProcessStrongInference(ISolverView view, PossibilityCoordinate inference)
    {
        return view.AddDefinitiveNumber(inference.Possibility, inference.Row, inference.Col, Strategy!);
    }
}