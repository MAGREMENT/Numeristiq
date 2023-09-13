using System;
using System.Collections.Generic;
using System.Linq;
using Model.Solver.Possibilities;
using Model.Solver.StrategiesUtil;
using Model.Solver.StrategiesUtil.LinkGraph;
using Model.StrategiesUtil;
using Model.StrategiesUtil.LinkGraph;

namespace Model.Solver.Strategies.AlternatingChains.ChainTypes;

public class FullAIC : IAlternatingChainType<ILinkGraphElement>
{
    public string Name => "Alternating inference chain";
    public StrategyLevel Difficulty => StrategyLevel.Extreme;
    public IStrategy? Strategy { get; set; }
    public IEnumerable<LinkGraph<ILinkGraphElement>> GetGraphs(IStrategyManager view)
    {
        yield return view.LinkGraph();
    }

    public bool ProcessFullLoop(IStrategyManager view, Loop<ILinkGraphElement> loop)
    {
        loop.ForEachLink((one, two) => ProcessWeakLink(view, one, two), LinkStrength.Weak);

        return view.ChangeBuffer.Push(Strategy!, new AlternatingChainReportBuilder<ILinkGraphElement>(loop));
    }

    private void ProcessWeakLink(IStrategyManager view, ILinkGraphElement one, ILinkGraphElement two)
    {
        List<CellPossibility> onePoss = new();
        List<CellPossibility> twoPoss = new();

        foreach (var c1 in one.EachElement())
        {
            onePoss.AddRange(c1.ToPossibilityCoordinates());
        }

        foreach (var c2 in two.EachElement())
        {
            twoPoss.AddRange(c2.ToPossibilityCoordinates());
        }

        if (onePoss.Count == 0 || twoPoss.Count == 0) throw new ArgumentException("Wtf");

        if (onePoss.Count == 1 && twoPoss.Count == 1 && onePoss[0].Row == twoPoss[0].Row &&
            onePoss[0].Col == twoPoss[0].Col)
        {
            RemoveAllExcept(view, onePoss[0].Row, onePoss[0].Col, onePoss[0].Possibility, twoPoss[0].Possibility);
        }
        else
        {
            foreach (var commons in AllCommons(onePoss, twoPoss))
            {
                bool sameRow = true;
                int sharedRow = commons[0].Row;
                bool sameCol = true;
                int sharedCol = commons[0].Col;
                bool sameMini = true;
                int sharedMiniRow =  commons[0].Row / 3;
                int sharedMiniCol =  commons[0].Col / 3;

                for (int i = 1; i < commons.Count; i++)
                {
                    if (commons[i].Row != sharedRow) sameRow = false;
                    if (commons[i].Col != sharedCol) sameCol = false;
                    if (commons[i].Row / 3 != sharedMiniRow || commons[i].Col / 3 != sharedMiniCol) sameMini = false;
                }

                int possibility = commons[0].Possibility;
                if (sameRow)
                {
                    for (int col = 0; col < 9; col++)
                    {
                        var current = new CellPossibility(sharedRow, col, possibility);
                        if (commons.Contains(current)) continue;

                        view.ChangeBuffer.AddPossibilityToRemove(possibility, sharedRow, col);
                    }
                }

                if (sameCol)
                {
                    for (int row = 0; row < 9; row++)
                    {
                        var current = new CellPossibility(row, sharedCol, possibility);
                        if (commons.Contains(current)) continue;

                        view.ChangeBuffer.AddPossibilityToRemove(possibility, row, sharedCol);
                    }
                }

                if (sameMini)
                {
                    for (int gridRow = 0; gridRow < 3; gridRow++)
                    {
                        for (int gridCol = 0; gridCol < 3; gridCol++)
                        {
                            var row = sharedMiniRow * 3 + gridRow;
                            var col = sharedMiniCol * 3 + gridCol;
                            
                            var current = new CellPossibility(row, col, possibility);
                            if (commons.Contains(current)) continue;

                            view.ChangeBuffer.AddPossibilityToRemove(possibility, row, col);
                        }
                    }
                }
            }
        }
    }

    private List<List<CellPossibility>> AllCommons(List<CellPossibility> onePoss, List<CellPossibility> twoPoss)
    {
        var result = new List<List<CellPossibility>>();
        IPossibilities done = IPossibilities.NewEmpty();
        for (int i = 0; i < onePoss.Count; i++)
        {
            if (done.Peek(onePoss[i].Possibility)) continue;
            
            List<CellPossibility> current = new() { onePoss[i] };
            for (int j = i + 1; j < onePoss.Count; j++)
            {
                if (onePoss[j].Possibility == onePoss[i].Possibility) current.Add(onePoss[j]);
            }

            bool yes = false;
            for (int j = 0; j < twoPoss.Count; j++)
            {
                if (twoPoss[j].Possibility == onePoss[i].Possibility)
                {
                    current.Add(twoPoss[j]);
                    yes = true;
                }
            }

            if (yes) result.Add(current);
            done.Add(onePoss[i].Possibility);
        }

        return result;
    }
    
    private void RemoveAllExcept(IStrategyManager strategyManager, int row, int col, params int[] except)
    {
        foreach (var possibility in strategyManager.PossibilitiesAt(row, col))
        {
            if (!except.Contains(possibility))
            {
                strategyManager.RemovePossibility(possibility, row, col, Strategy!);
            }
        }
    }

    public bool ProcessWeakInference(IStrategyManager view, ILinkGraphElement inference, Loop<ILinkGraphElement> loop)
    {
        if (inference is not CellPossibility pos) return false;
        view.ChangeBuffer.AddPossibilityToRemove(pos.Possibility, pos.Row, pos.Col);

        return view.ChangeBuffer.Push(Strategy!, new AlternatingChainReportBuilder<ILinkGraphElement>(loop));
    }

    public bool ProcessStrongInference(IStrategyManager view, ILinkGraphElement inference, Loop<ILinkGraphElement> loop)
    {
        if (inference is not CellPossibility pos) return false;
        view.ChangeBuffer.AddDefinitiveToAdd(pos.Possibility, pos.Row, pos.Col);

        return view.ChangeBuffer.Push(Strategy!, new AlternatingChainReportBuilder<ILinkGraphElement>(loop));
    }
}