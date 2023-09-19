using System.Collections.Generic;
using Model.Solver.Helpers;
using Model.Solver.Helpers.Changes;
using Model.Solver.StrategiesUtil;
using Model.Solver.StrategiesUtil.LinkGraph;

namespace Model.Solver.Strategies.ForcingNets;

public class DigitForcingNetStrategy : IStrategy
{
    public string Name => "Digit forcing net";
    public StrategyLevel Difficulty => StrategyLevel.Extreme;
    public StatisticsTracker Tracker { get; } = new();
    public void ApplyOnce(IStrategyManager strategyManager)
    {
        for (int row = 0; row < 9; row++)
        {
            for (int col = 0; col < 9; col++)
            {
                foreach (var possibility in strategyManager.PossibilitiesAt(row, col))
                {
                    Dictionary<ILinkGraphElement, Coloring> onColoring =
                        strategyManager.PreComputer.OnColoring(row, col, possibility);
                    Dictionary<ILinkGraphElement, Coloring> offColoring =
                        strategyManager.PreComputer.OffColoring(row, col, possibility);

                    if(onColoring.Count == 1 || offColoring.Count == 1) continue;
                    Process(strategyManager, onColoring, offColoring);

                    if (strategyManager.ChangeBuffer.NotEmpty()) strategyManager.ChangeBuffer.Push(this,
                            new DigitForcingNetReportBuilder(onColoring, offColoring, row, col, possibility));
                }
            }
        }
    }

    private void Process(IStrategyManager view, Dictionary<ILinkGraphElement, Coloring> onColoring,
        Dictionary<ILinkGraphElement, Coloring> offColoring)
    {
        foreach (var on in onColoring)
        {
            if (on.Key is not CellPossibility possOn) continue;
            if (offColoring.TryGetValue(possOn, out var other))
            {
                switch (other)
                {
                    case Coloring.Off when on.Value == Coloring.Off :
                        view.ChangeBuffer.AddPossibilityToRemove(possOn.Possibility, possOn.Row, possOn.Col);
                        break;
                    case Coloring.On when on.Value == Coloring.On :
                        view.ChangeBuffer.AddDefinitiveToAdd(possOn.Possibility, possOn.Row, possOn.Col);
                        break;
                }
            }

            if (on.Value != Coloring.On) continue;
            foreach (var off in offColoring)
            {
                if (off.Value != Coloring.On || off.Key is not CellPossibility possOff) continue;
                if (possOff.Row == possOn.Row && possOn.Col == possOff.Col)
                    RemoveAll(view, possOn.Row, possOn.Col, possOn.Possibility, possOff.Possibility);

                else if (possOff.Possibility == possOn.Possibility && possOn.ShareAUnit(possOff))
                {
                    foreach (var coord in possOn.SharedSeenCells(possOff))
                    {
                        view.ChangeBuffer.AddPossibilityToRemove(possOn.Possibility, coord.Row, coord.Col);
                    }
                }
            }
        }
    }

    private void RemoveAll(IStrategyManager view, int row, int col, int except1, int except2)
    {
        foreach (var possibility in view.PossibilitiesAt(row, col))
        {
            if (possibility == except1 || possibility == except2) continue;
            view.ChangeBuffer.AddPossibilityToRemove(possibility, row, col);
        }
    }
}

public class DigitForcingNetReportBuilder : IChangeReportBuilder
{
    private readonly Dictionary<ILinkGraphElement, Coloring> _onColoring;
    private readonly Dictionary<ILinkGraphElement, Coloring> _offColoring;
    private readonly int _row;
    private readonly int _col;
    private readonly int _possibility;

    public DigitForcingNetReportBuilder(Dictionary<ILinkGraphElement, Coloring> onColoring,
        Dictionary<ILinkGraphElement, Coloring> offColoring, int row, int col, int possibility)
    {
        _onColoring = onColoring;
        _offColoring = offColoring;
        _row = row;
        _col = col;
        _possibility = possibility;
    }
    
    public ChangeReport Build(List<SolverChange> changes, IPossibilitiesHolder snapshot)
    {
        var on = ForcingNetsUtil.FilterPossibilityCoordinates(_onColoring);
        var off = ForcingNetsUtil.FilterPossibilityCoordinates(_offColoring);

        return new ChangeReport(IChangeReportBuilder.ChangesToString(changes), "",
            lighter =>
            {
                IChangeReportBuilder.HighlightChanges(lighter, changes);
                lighter.CirclePossibility(_possibility, _row, _col);
            },
            lighter =>
            {
                ForcingNetsUtil.HighlightColoring(lighter, on);
                lighter.CirclePossibility(_possibility, _row, _col);
            },
            lighter =>
            {
                ForcingNetsUtil.HighlightColoring(lighter, off);
                lighter.CirclePossibility(_possibility, _row, _col);
            });
    }
}