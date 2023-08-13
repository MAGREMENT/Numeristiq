using System.Collections.Generic;
using System.Linq;
using Model.StrategiesUtil;
using Model.StrategiesUtil.LoopFinder;

namespace Model.Strategies.AlternatingChains.ChainTypes;

public class NormalAIC : IAlternatingChainType<PossibilityCoordinate>
{
    public string Name => "Alternating inference chain";
    public StrategyLevel Difficulty => StrategyLevel.Extreme;
    public IStrategy? Strategy { get; set; }

    public IEnumerable<LinkGraph<PossibilityCoordinate>> GetGraphs(IStrategyManager view)
    {
        LinkGraph<PossibilityCoordinate> graph = new();
        for (int row = 0; row < 9; row++)
        {
            for (int col = 0; col < 9; col++)
            {
                foreach (var possibility in view.Possibilities[row, col])
                {
                    PossibilityCoordinate current = new PossibilityCoordinate(row, col, possibility);
                    
                    //Row
                    var ppir = view.PossibilityPositionsInRow(row, possibility);
                    var strength = ppir.Count == 2 ? LinkStrength.Strong : LinkStrength.Weak;
                    foreach (var c in ppir)
                    {
                        if (c != col)
                        {
                            graph.AddLink(current, new PossibilityCoordinate(row, c, possibility), strength);
                        }
                    }


                    //Col
                    var ppic = view.PossibilityPositionsInColumn(col, possibility);
                    strength = ppic.Count == 2 ? LinkStrength.Strong : LinkStrength.Weak;
                    foreach (var r in ppic)
                    {
                        if (r != row)
                        {
                            graph.AddLink(current, new PossibilityCoordinate(r, col, possibility), strength);
                        }
                    }


                    //MiniGrids
                    var ppimn = view.PossibilityPositionsInMiniGrid(row / 3, col / 3, possibility);
                    strength = ppimn.Count == 2 ? LinkStrength.Strong : LinkStrength.Weak;
                    foreach (var pos in ppimn)
                    {
                        if (!(pos[0] == row && pos[1] == col))
                        {
                            graph.AddLink(current, new PossibilityCoordinate(pos[0], pos[1], possibility), strength);
                        }
                    }

                    strength = view.Possibilities[row, col].Count == 2 ? LinkStrength.Strong : LinkStrength.Weak;
                    foreach (var pos in view.Possibilities[row, col])
                    {
                        if (pos != possibility)
                        {
                            graph.AddLink(current, new PossibilityCoordinate(row, col, pos), strength);
                        }
                    }
                }
            }
        }

        yield return graph;
    }

    public bool ProcessFullLoop(IStrategyManager view, Loop<PossibilityCoordinate> loop)
    {
        bool wasProgressMade = false;
        
        loop.ForEachLink((one, two)
            => ProcessWeakLink(view, one, two, out wasProgressMade), LinkStrength.Weak);
        return wasProgressMade;
    }

    private void ProcessWeakLink(IStrategyManager view, PossibilityCoordinate one, PossibilityCoordinate two, out bool wasProgressMade)
    {
        if (one.Row == two.Row && one.Col == two.Col)
        {
            if (RemoveAllExcept(view, one.Row, one.Col, one.Possibility, two.Possibility)) wasProgressMade = true;
        }
        else
        {
            foreach (var coord in one.SharedSeenCells(two))
            {
                if (view.RemovePossibility(one.Possibility, coord.Row, coord.Col, Strategy!)) wasProgressMade = true;
            }   
        }

        wasProgressMade = false;
    }
    
    private bool RemoveAllExcept(IStrategyManager strategyManager, int row, int col, params int[] except)
    {
        bool wasProgressMade = false;
        foreach (var possibility in strategyManager.Possibilities[row, col])
        {
            if (!except.Contains(possibility))
            {
                if (strategyManager.RemovePossibility(possibility, row, col, Strategy!)) wasProgressMade = true;
            }
        }

        return wasProgressMade;
    }

    public bool ProcessWeakInference(IStrategyManager view, PossibilityCoordinate inference, Loop<PossibilityCoordinate> loop)
    {
        return view.RemovePossibility(inference.Possibility, inference.Row, inference.Col, Strategy!);
    }

    public bool ProcessStrongInference(IStrategyManager view, PossibilityCoordinate inference, Loop<PossibilityCoordinate> loop)
    {
        return view.AddDefinitiveNumber(inference.Possibility, inference.Row, inference.Col, Strategy!);
    }
}