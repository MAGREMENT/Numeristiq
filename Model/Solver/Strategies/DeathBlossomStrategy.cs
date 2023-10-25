using System.Collections.Generic;
using Model.Solver.Helpers.Changes;
using Model.Solver.Helpers.Highlighting;
using Model.Solver.Possibilities;
using Model.Solver.StrategiesUtil;

namespace Model.Solver.Strategies;

public class DeathBlossomStrategy : AbstractStrategy
{
    public const string OfficialName = "Death Blossom";
    
    public DeathBlossomStrategy() : base(OfficialName, StrategyDifficulty.Extreme)
    {
    }

    public override void ApplyOnce(IStrategyManager strategyManager)
    {
        var als = strategyManager.PreComputer.AlmostLockedSets();

        for (int i = 0; i < als.Count; i++)
        {
            for (int j = 0; j < als.Count; j++)
            {
                var one = als[i];
                var two = als[j];
                
                if (one.HasAtLeastOneCoordinateInCommon(two)) continue;

                foreach (var possibility in one.Possibilities)
                {
                    if (two.Possibilities.Peek(possibility))
                    {
                        if (Try(strategyManager, one, two, possibility)) return;
                    }
                }
            }
        }
    }

    private bool Try(IStrategyManager strategyManager, AlmostLockedSet one, AlmostLockedSet two, int possibility)
    {
        var stems = new List<Cell>();
        Dictionary<Cell, IPossibilities> eliminations = new();
        List<Cell> buffer = new();

        foreach (var cell in one.Cells)
        {
            if (strategyManager.PossibilitiesAt(cell).Peek(possibility)) buffer.Add(cell);
        }

        foreach (var cell in two.Cells)
        {
            if (strategyManager.PossibilitiesAt(cell).Peek(possibility)) buffer.Add(cell);
        }

        foreach (var cell in Cells.SharedSeenCells(buffer))
        {
            if (strategyManager.PossibilitiesAt(cell).Peek(possibility)) stems.Add(cell);
        }

        if (stems.Count == 0) return false;
        buffer.Clear();

        /*Debug
        var debugPossOne = IPossibilities.NewEmpty();
        debugPossOne.Add(1);
        debugPossOne.Add(3);
        debugPossOne.Add(5);
        debugPossOne.Add(7);
        var debugOne = new AlmostLockedSet(new[]
        {
            new Cell(0, 2),
            new Cell(4, 2), new Cell(5, 2)
        }, debugPossOne);

        var debugPossTwo = IPossibilities.NewEmpty();
        debugPossTwo.Add(1);
        debugPossTwo.Add(6);
        debugPossTwo.Add(7);
        debugPossTwo.Add(8);
        var debugTwo = new AlmostLockedSet(new[]
        {
            new Cell(2, 4), new Cell(2, 5), new Cell(2, 7)
        }, debugPossTwo);

        if (one.Equals(debugOne) && two.Equals(debugTwo) && possibility == 7)
        {
            int a = 0;
        }*/

        for (int i = 0; i < 2; i++)
        {
            var current = i == 0 ? one : two;
            foreach (var poss in current.Possibilities)
            {
                if (poss == possibility) continue;
                
                foreach (var cell in current.Cells)
                {
                    if (strategyManager.PossibilitiesAt(cell).Peek(poss)) buffer.Add(cell);
                }

                foreach (var cell in Cells.SharedSeenCells(buffer))
                {
                    if (!eliminations.TryGetValue(cell, out var value))
                    {
                        value = IPossibilities.NewEmpty();
                        eliminations[cell] = value;
                    }

                    value.Add(poss);
                }
            
                buffer.Clear();
            }
        }

        foreach (var entry in eliminations)
        {
            var actualPoss = strategyManager.PossibilitiesAt(entry.Key);
            if(actualPoss.Count == 0) continue;
            
            if (entry.Value.PeekAll(actualPoss) && !one.Contains(entry.Key) && !two.Contains(entry.Key)
                && !stems.Contains(entry.Key))
            {
                foreach (var stem in stems)
                {
                    strategyManager.ChangeBuffer.AddPossibilityToRemove(possibility, stem.Row, stem.Col);
                }
                
                strategyManager.ChangeBuffer.Push(this, new DeathBlossomReportBuilder(stems, one, two, entry.Key));
                return true;
            }
        }

        return false;
    }
}

public class DeathBlossomReportBuilder : IChangeReportBuilder
{
    private readonly List<Cell> _stems;
    private readonly AlmostLockedSet _one;
    private readonly AlmostLockedSet _two;
    private readonly Cell _target;

    public DeathBlossomReportBuilder(List<Cell> stems, AlmostLockedSet one, AlmostLockedSet two, Cell target)
    {
        _stems = stems;
        _one = one;
        _two = two;
        _target = target;
    }

    public ChangeReport Build(List<SolverChange> changes, IPossibilitiesHolder snapshot)
    {
        return new ChangeReport(IChangeReportBuilder.ChangesToString(changes), "", lighter =>
        {
            foreach (var stem in _stems)
            {
                lighter.HighlightCell(stem, ChangeColoration.Neutral); 
            }

            foreach (var cell in _one.Cells)
            {
                lighter.HighlightCell(cell, ChangeColoration.CauseOffOne);
            }

            foreach (var cell in _two.Cells)
            {
                lighter.HighlightCell(cell, ChangeColoration.CauseOffTwo);
            }
            
            lighter.HighlightCell(_target, ChangeColoration.CauseOnOne);

            IChangeReportBuilder.HighlightChanges(lighter, changes);
        });
    }
}