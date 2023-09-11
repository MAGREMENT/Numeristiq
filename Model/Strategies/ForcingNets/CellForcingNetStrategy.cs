using System.Collections.Generic;
using System.Linq;
using Model.Changes;
using Model.Possibilities;
using Model.Solver;
using Model.StrategiesUtil;
using Model.StrategiesUtil.LinkGraph;

namespace Model.Strategies.ForcingNets;

public class CellForcingNetStrategy : IStrategy
{
    public string Name => "Cell forcing net";
    public StrategyLevel Difficulty => StrategyLevel.Extreme;
    public StatisticsTracker Tracker { get; } = new();

    private readonly int _max;

    public CellForcingNetStrategy(int maxPossibilities)
    {
        _max = maxPossibilities;
    }

    public void ApplyOnce(IStrategyManager strategyManager)
    {
        for (int row = 0; row < 9; row++)
        {
            for (int col = 0; col < 9; col++)
            {
                if (strategyManager.Possibilities[row, col].Count < 2 ||
                    strategyManager.Possibilities[row, col].Count > _max) continue;
                var possAsArray = strategyManager.Possibilities[row, col].ToArray();

                Dictionary<ILinkGraphElement, Coloring>[] colorings =
                    new Dictionary<ILinkGraphElement, Coloring>[possAsArray.Length];

                for (int i = 0; i < possAsArray.Length; i++)
                {
                    colorings[i] = strategyManager.OnColoring(row, col, possAsArray[i]);
                }

                Process(strategyManager, colorings);

                if (strategyManager.ChangeBuffer.NotEmpty()) strategyManager.ChangeBuffer.Push(this,
                        new CellForcingNetReportBuilder(colorings, row, col));
            }
        }
    }

    private void Process(IStrategyManager view, Dictionary<ILinkGraphElement, Coloring>[] colorings)
    {
        PossibilityStacker?[,] cellCheck = new PossibilityStacker[9, 9];

        foreach (var element in colorings[0])
        {
            if (element.Key is not CellPossibility current) continue;
            var currentColoring = element.Value;
            bool isSameInAll = true;

            for (int i = 1; i < colorings.Length && isSameInAll; i++)
            {
                if (!colorings[i].TryGetValue(current, out var c) || c != currentColoring) isSameInAll = false;
            }

            if (isSameInAll)
            {
                if (currentColoring == Coloring.On)
                    view.ChangeBuffer.AddDefinitiveToAdd(current.Possibility, current.Row, current.Col);
                else view.ChangeBuffer.AddPossibilityToRemove(current.Possibility, current.Row, current.Col);
            }

            if(currentColoring == Coloring.On) InitStackerArray(cellCheck, current);
        }

        for (int i = 1; i < colorings.Length; i++)
        {
            foreach (var element in colorings[i])
            {
                if (element.Value != Coloring.On) continue;
                if (element.Key is not CellPossibility current) continue;

                AddToStackerArray(view, cellCheck, current, colorings.Length, i);
            }
        }
        
        //TODO do rule 4 and look into rule 3 (doesnt change anything currently)
    }

    private void InitStackerArray(PossibilityStacker?[,] array, CellPossibility coord)
    {
        array[coord.Row, coord.Col] ??= new PossibilityStacker();
        array[coord.Row, coord.Col]!.Possibilities.Add(coord.Possibility);
        array[coord.Row, coord.Col]!.ColoringCount = 1;
    }
    
    private void AddToStackerArray(IStrategyManager view, PossibilityStacker?[,] array, CellPossibility coord,
        int total, int currentColoring)
    {
        var current = array[coord.Row, coord.Col];
        if (current is null) return;

        current.Possibilities.Add(coord.Possibility);
        current.ColoringCount |= 1 << currentColoring;

        if (currentColoring == total - 1 && System.Numerics.BitOperations.PopCount((uint) current.ColoringCount) == total)
            RemoveAll(view, coord.Row, coord.Col, current.Possibilities);
    }

    private void RemoveAll(IStrategyManager view, int row, int col, IPossibilities except)
    {
        foreach (var possibility in view.Possibilities[row, col])
        {
            if(except.Peek(possibility)) continue;
            view.ChangeBuffer.AddPossibilityToRemove(possibility, row, col);
        }
    }
}

public class CellForcingNetReportBuilder : IChangeReportBuilder
{
    private readonly Dictionary<ILinkGraphElement, Coloring>[] _colorings;
    private readonly int _row;
    private readonly int _col;

    public CellForcingNetReportBuilder(Dictionary<ILinkGraphElement, Coloring>[] colorings, int row, int col)
    {
        _colorings = colorings;
        _row = row;
        _col = col;
    }

    public ChangeReport Build(List<SolverChange> changes, ISolver snapshot)
    {
        HighlightSolver[] highlights = new HighlightSolver[_colorings.Length + 1];
        highlights[0] = lighter =>
        {
            IChangeReportBuilder.HighlightChanges(lighter, changes);
            lighter.CircleCell(_row, _col);
        };

        for (int i = 0; i < _colorings.Length; i++)
        {
            var filtered = ForcingNetsUtil.FilterPossibilityCoordinates(_colorings[i]);
            highlights[i + 1] = lighter =>
            {
                ForcingNetsUtil.HighlightColoring(lighter, filtered);
                lighter.CircleCell(_row, _col);
            };
        }
        
        return new ChangeReport(IChangeReportBuilder.ChangesToString(changes), "", highlights);
    }
}

public class PossibilityStacker
{
    public int ColoringCount { get; set; }
    public IPossibilities Possibilities { get; } = IPossibilities.NewEmpty();
}