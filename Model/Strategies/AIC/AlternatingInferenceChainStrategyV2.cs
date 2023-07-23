using System;
using LoopFinder;
using Model.LoopFinder;
using Model.LoopFinder.Types;
using Model.StrategiesUtil;

namespace Model.Strategies.AIC;

public class AlternatingInferenceChainStrategyV2 : IStrategy
{
    public string Name => "Alternating inference chain";
    public StrategyLevel Difficulty => StrategyLevel.Extreme;
    public int Score { get; set; }

    public void ApplyOnce(ISolverView solverView)
    {
        var graph = InitGraph(solverView);

        LoopFinder<PossibilityCoordinate> lf = 
            new(graph, new BruteAICLoops<PossibilityCoordinate>(AICLoopSearchParam.LoopFoundCancel),
                (loop) =>
                {
                    if (loop.Count % 2 == 0) return ProcessEvenLoop(solverView, graph, loop);
                    return ProcessOddLoop(solverView, loop);
                });
        lf.Run();
    }

    private bool ProcessEvenLoop(ISolverView view, Graph<PossibilityCoordinate> graph, Loop<PossibilityCoordinate> loop)
    {
        bool wasProgressMade = false;
        loop.ForEachLink((one, two) =>
        {
            if (!graph.IsOfStrength(one, two, LinkStrength.Weak)) return;
            if (one.Row == two.Row && one.Col == two.Col)
            {
                foreach (var possibility in view.Possibilities[one.Row, one.Col])
                {
                    if (possibility != one.Possibility && possibility != two.Possibility)
                    {
                        view.RemovePossibility(possibility, one.Row, one.Col, this);
                        wasProgressMade = true;
                    }
                }
            }
            else
            {
                foreach (var coord in one.SharedSeenCells(two))
                {
                    if (view.RemovePossibility(one.Possibility, coord.Row, coord.Col, this))
                        wasProgressMade = true;
                } 
            }
            
        }, LinkStrength.Weak);

        return wasProgressMade;
    }

    private bool ProcessOddLoop(ISolverView view, Loop<PossibilityCoordinate> loop)
    {
        switch (loop.FindDoubleLink(out var coord))
        {
            case LinkStrength.Strong :
                if (view.AddDefinitiveNumber(coord.Possibility, coord.Row, coord.Col, this)) return true;
                break;
            case LinkStrength.Weak :
                if(view.RemovePossibility(coord.Possibility, coord.Row, coord.Col, this)) return true;
                break;
        }

        return false;
    }

    private Graph<PossibilityCoordinate> InitGraph(ISolverView solverView)
    {
        var result = new Graph<PossibilityCoordinate>();
        for (int row = 0; row < 9; row++)
        {
            for (int col = 0; col < 9; col++)
            {
                foreach (var possibility in solverView.Possibilities[row, col])
                {
                    PossibilityCoordinate current = new PossibilityCoordinate(row, col, possibility);

                    //Row
                    var ppir = solverView.PossibilityPositionsInRow(row, possibility);
                    var strength = ppir.Count == 2 ? LinkStrength.Strong : LinkStrength.Weak;
                    foreach (var c in ppir)
                    {
                        if (c != col)
                        {
                            var coord = new PossibilityCoordinate(row, c, possibility);
                            result.AddLink(current, coord, strength);
                        }
                    }


                    //Col
                    var ppic = solverView.PossibilityPositionsInColumn(col, possibility);
                    strength = ppic.Count == 2 ? LinkStrength.Strong : LinkStrength.Weak;
                    foreach (var r in ppic)
                    {
                        if (r != row)
                        {
                            var coord = new PossibilityCoordinate(r, col, possibility);
                            result.AddLink(current, coord, strength);
                        }
                    }

                    //MiniGrids
                    var ppimn = solverView.PossibilityPositionsInMiniGrid(row / 3, col / 3, possibility);
                    strength = ppimn.Count == 2 ? LinkStrength.Strong : LinkStrength.Weak;
                    foreach (var pos in ppimn)
                    {
                        if (!(pos[0] == row && pos[1] == col))
                        {
                            var coord = new PossibilityCoordinate(pos[0], pos[1], possibility);
                            result.AddLink(current, coord, strength);
                        }
                    }

                    strength = solverView.Possibilities[row, col].Count == 2 ? LinkStrength.Strong : LinkStrength.Weak;
                    foreach (var pos in solverView.Possibilities[row, col])
                    {
                        if (pos != possibility)
                        {
                            var coord = new PossibilityCoordinate(row, col, pos);
                            result.AddLink(current, coord, strength);
                        }
                    }
                }
            }
        }

        return result;
    }
}