using System.Linq;
using Model.Solver.StrategiesUtil;
using Model.Solver.StrategiesUtil.LinkGraph;

namespace Model.Solver.Strategies.AlternatingChains.ChainTypes;

public class NormalAIC : IAlternatingChainType<CellPossibility>
{
    public string Name => "Alternating inference chain";
    public StrategyDifficulty Difficulty => StrategyDifficulty.Extreme;
    public IStrategy? Strategy { get; set; }

    public LinkGraph<CellPossibility> GetGraph(IStrategyManager view)
    {
        LinkGraph<CellPossibility> graph = new();
        for (int row = 0; row < 9; row++)
        {
            for (int col = 0; col < 9; col++)
            {
                foreach (var possibility in view.PossibilitiesAt(row, col))
                {
                    CellPossibility current = new CellPossibility(row, col, possibility);
                    
                    //Row
                    var ppir = view.RowPositionsAt(row, possibility);
                    var strength = ppir.Count == 2 ? LinkStrength.Strong : LinkStrength.Weak;
                    foreach (var c in ppir)
                    {
                        if (c != col)
                        {
                            graph.AddLink(current, new CellPossibility(row, c, possibility), strength);
                        }
                    }


                    //Col
                    var ppic = view.ColumnPositionsAt(col, possibility);
                    strength = ppic.Count == 2 ? LinkStrength.Strong : LinkStrength.Weak;
                    foreach (var r in ppic)
                    {
                        if (r != row)
                        {
                            graph.AddLink(current, new CellPossibility(r, col, possibility), strength);
                        }
                    }


                    //MiniGrids
                    var ppimn = view.MiniGridPositionsAt(row / 3, col / 3, possibility);
                    strength = ppimn.Count == 2 ? LinkStrength.Strong : LinkStrength.Weak;
                    foreach (var pos in ppimn)
                    {
                        if (!(pos.Row == row && pos.Col == col))
                        {
                            graph.AddLink(current, new CellPossibility(pos.Row, pos.Col, possibility), strength);
                        }
                    }

                    strength = view.PossibilitiesAt(row, col).Count == 2 ? LinkStrength.Strong : LinkStrength.Weak;
                    foreach (var pos in view.PossibilitiesAt(row, col))
                    {
                        if (pos != possibility)
                        {
                            graph.AddLink(current, new CellPossibility(row, col, pos), strength);
                        }
                    }
                }
            }
        }

        return graph;
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
        foreach (var possibility in strategyManager.PossibilitiesAt(row, col))
        {
            if (!except.Contains(possibility))
            {
                if (strategyManager.RemovePossibility(possibility, row, col, Strategy!)) wasProgressMade = true;
            }
        }

        return wasProgressMade;
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