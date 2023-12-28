using System.Collections.Generic;
using Global;
using Model.Solver.Helpers.Changes;
using Model.Solver.Possibility;

namespace Model.Solver.Strategies.UniquenessClueCover.PatternCollections.Bands;

public class BandCollection : IPatternCollection
{
    private readonly BandPattern[] _collection;

    public IStrategy? Strategy { get; set; }

    public BandCollection(params BandPattern[] collection)
    {
        _collection = collection;
    }

    public static BandCollection FullCollection()
    {
        return new BandCollection(new TwoClueBandPattern());
    }

    public bool Apply(IStrategyManager strategyManager)
    {
        for (int mini = 0; mini < 3; mini++)
        {
            if(Check(strategyManager, mini, Unit.Row)) return true;
            if(Check(strategyManager, mini, Unit.Column)) return true;
        }

        return false;
    }

    private readonly HashSet<Cell> _clues = new();
    private readonly List<Cell> _used = new();

    private bool Check(IStrategyManager strategyManager, int mini, Unit unit)
    {
        foreach (var pattern in _collection)
        {
            if (!GetClues(strategyManager, mini, unit, _clues, pattern.ClueCount,
                    pattern.DifferentClueCount)) continue;

            foreach (var boxKey in OrderKeyGenerator.GenerateAll())
            {
                foreach (var widthKey in OrderKeyGenerator.GenerateAll())
                {
                    foreach (var lengthKey in OrderKeyGenerator.GenerateAll())
                    {
                        if (Try(strategyManager, pattern, boxKey, widthKey, lengthKey, mini, unit)) return true;
                    }
                }
            }
        }

        return false;
    }

    private bool Try(IStrategyManager strategyManager, BandPattern pattern, int[] boxKey, int[] widthKey,
        int[] lengthKey, int mini, Unit unit)
    {
        _used.Clear();
        
        var boxes = pattern.PlacementsWithKey(boxKey);
        int[] numberEquivalence = new int[pattern.DifferentClueCount];

        for (int i = 0; i < 3; i++)
        {
            foreach (var entry in boxes[i])
            {
                var cell = entry.Key.Transform(widthKey, lengthKey).ToCell(mini, i, unit);
                if (_clues.Contains(cell)) _used.Add(cell);

                var solved = strategyManager.Sudoku[cell.Row, cell.Column];
                if (solved == 0) return false;

                if (numberEquivalence[entry.Value] == 0) numberEquivalence[entry.Value] = solved;
                else if (numberEquivalence[entry.Value] != solved) return false;
            }
        }

        if (_used.Count != _clues.Count) return false;

        var eliminations = pattern.EliminationsWithKey(boxKey);

        for (int i = 0; i < 3; i++)
        {
            foreach (var entry in eliminations[i])
            {
                var cell = entry.Key.Transform(widthKey, lengthKey).ToCell(mini, i, unit);

                foreach (var p in entry.Value.EveryElimination(numberEquivalence))
                {
                    strategyManager.ChangeBuffer.ProposePossibilityRemoval(p, cell);
                }
            }
        }
        
        return strategyManager.ChangeBuffer.NotEmpty() && strategyManager.ChangeBuffer.Commit(Strategy!,
            new BandUniquenessClueCoverReportBuilder()) && Strategy!.OnCommitBehavior == OnCommitBehavior.Return;
    }
    
    private bool GetClues(IStrategyManager strategyManager, int mini, Unit unit, HashSet<Cell> clues,
        int maxClueCount, int maxDifferentClueCount)
    {
        _clues.Clear();

        int clueCount = 0;
        Possibilities differentClues = Possibilities.NewEmpty();
        for (int w = 0; w < 3; w++)
        {
            for (int l = 0; l < 9; l++)
            {
                var cell = unit == Unit.Row ? new Cell(mini * 3 + w, l) : new Cell(l, mini * 3 + w);
                var clue = strategyManager.StartState[cell.Row, cell.Column];
                if (clue == 0) continue;

                clueCount++;
                differentClues.Add(clue);
                if (clueCount > maxClueCount || differentClues.Count > maxDifferentClueCount) return false;

                clues.Add(cell);
            }
        }

        return true;
    }
}

public class BandUniquenessClueCoverReportBuilder : IChangeReportBuilder
{
    public ChangeReport Build(List<SolverChange> changes, IPossibilitiesHolder snapshot)
    {
        return ChangeReport.Default(changes);
    }
}