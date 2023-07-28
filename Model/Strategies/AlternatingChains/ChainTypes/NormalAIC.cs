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

    public IEnumerable<Graph<PossibilityCoordinate>> GetGraphs(ISolverView view)
    {
        yield return view.LinkGraph();
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
    
    private bool RemoveAllExcept(ISolverView solverView, int row, int col, params int[] except)
    {
        bool wasProgressMade = false;
        foreach (var possibility in solverView.Possibilities[row, col])
        {
            if (!except.Contains(possibility))
            {
                if (solverView.RemovePossibility(possibility, row, col, Strategy!)) wasProgressMade = true;
            }
        }

        return wasProgressMade;
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