using System.Collections.Generic;
using Model.Helpers;
using Model.Helpers.Changes;
using Model.Helpers.Highlighting;
using Model.Utility;
using Model.Utility.BitSets;

namespace Model.Sudoku.Solver.Strategies.UniquenessClueCover.PatternCollections.Bands;

public class BandCollection : IPatternCollection
{
    private readonly BandPattern[] _collection;
    private readonly List<BandPatternCandidate> _candidates = new();
    
    private readonly HashSet<Cell> _cluesBuffer = new();
    private readonly List<Cell> _usedBuffer = new();

    public SudokuStrategy? Strategy { get; set; }

    public BandCollection(params BandPattern[] collection)
    {
        _collection = collection;
    }

    public static BandCollection FullCollection()
    {
        return new BandCollection(new TwoClueBandPattern(), new TripleCrossBandPattern(),
            new DiagonalTripleClueBandPattern(), new LTripleClueBandPattern(),
            new AlmostFlatTTripleClueBandPattern(), new ExtendedAlmostFlatTripleClueBandPattern(),
            new BrokenLTripleClueBandPattern());
    }

    public bool Filter(IStrategyUser strategyUser)
    {
        _candidates.Clear();
        for (int mini = 0; mini < 3; mini++)
        {
            if(CheckForCandidates(strategyUser, mini, Unit.Row)) return true;
            if(CheckForCandidates(strategyUser, mini, Unit.Column)) return true;
        }

        return false;
    }

    public bool Apply(IStrategyUser strategyUser)
    {
        foreach (var c in _candidates)
        {
            if (Try(strategyUser, c)) return true;
        }

        return false;
    }

    private bool CheckForCandidates(IStrategyUser strategyUser, int mini, Unit unit)
    {
        foreach (var pattern in _collection)
        {
            if (!GetClues(strategyUser, mini, unit, pattern.ClueCount, pattern.DifferentClueCount)) continue;

            foreach (var boxKey in OrderKeyGenerator.GenerateAll())
            {
                foreach (var widthKey in OrderKeyGenerator.GenerateAll())
                {
                    foreach (var lengthKey in OrderKeyGenerator.GenerateAll())
                    {
                        if (TryAndAddToCandidates(strategyUser, pattern, boxKey, widthKey, lengthKey, mini, unit)) return true;
                    }
                }
            }
        }

        return false;
    }

    private bool TryAndAddToCandidates(IStrategyUser strategyUser, BandPattern pattern, int[] boxKey, int[] widthKey,
        int[] lengthKey, int mini, Unit unit)
    {
        _usedBuffer.Clear();
        
        var boxes = pattern.PlacementsWithKey(boxKey);
        int[] numberEquivalence = new int[pattern.DifferentClueCount];
        bool ok = true;

        for (int i = 0; i < 3; i++)
        {
            foreach (var entry in boxes[i])
            {
                var cell = entry.Key.Transform(widthKey, lengthKey).ToCell(mini, i, unit);
                if (_cluesBuffer.Contains(cell)) _usedBuffer.Add(cell);

                var solved = strategyUser.Sudoku[cell.Row, cell.Column];
                if (solved == 0) ok = false;

                if (numberEquivalence[entry.Value] == 0) numberEquivalence[entry.Value] = solved;
                else if (numberEquivalence[entry.Value] != solved) return false;
            }
        }

        if (_usedBuffer.Count != _cluesBuffer.Count) return false;

        var candidate = new BandPatternCandidate(pattern, boxKey, widthKey, lengthKey, mini, unit);
        _candidates.Add(candidate);

        return ok && Process(strategyUser, candidate, numberEquivalence);
    }
    
    private bool Try(IStrategyUser strategyUser, BandPatternCandidate candidate)
    {
        var boxes = candidate.Pattern.PlacementsWithKey(candidate.BoxKey);
        int[] numberEquivalence = new int[candidate.Pattern.DifferentClueCount];

        for (int i = 0; i < 3; i++)
        {
            foreach (var entry in boxes[i])
            {
                var cell = entry.Key.Transform(candidate.WidthKey, candidate.LengthKey).ToCell(candidate.Mini,
                    i, candidate.Unit);

                var solved = strategyUser.Sudoku[cell.Row, cell.Column];
                if (solved == 0) return false;

                if (numberEquivalence[entry.Value] == 0) numberEquivalence[entry.Value] = solved;
                else if (numberEquivalence[entry.Value] != solved) return false;
            }
        }
        
        return Process(strategyUser, candidate, numberEquivalence);
    }

    private bool Process(IStrategyUser strategyUser, BandPatternCandidate candidate, int[] numberEquivalence)
    {
        var eliminations = candidate.Pattern.EliminationsWithKey(candidate.BoxKey);

        for (int i = 0; i < 3; i++)
        {
            foreach (var entry in eliminations[i])
            {
                var cell = entry.Key.Transform(candidate.WidthKey, candidate.LengthKey).ToCell(candidate.Mini,
                    i, candidate.Unit);

                foreach (var p in entry.Value.EveryElimination(numberEquivalence))
                {
                    strategyUser.ChangeBuffer.ProposePossibilityRemoval(p, cell);
                }
            }
        }
        
        return strategyUser.ChangeBuffer.NotEmpty() && strategyUser.ChangeBuffer.Commit(
            new BandUniquenessClueCoverReportBuilder(candidate)) && Strategy!.OnCommitBehavior == OnCommitBehavior.Return;
    }
    
    private bool GetClues(IStrategyUser strategyUser, int mini, Unit unit, int maxClueCount, int maxDifferentClueCount)
    {
        _cluesBuffer.Clear();

        int clueCount = 0;
        var differentClues = new ReadOnlyBitSet16();
        for (int w = 0; w < 3; w++)
        {
            for (int l = 0; l < 9; l++)
            {
                var cell = unit == Unit.Row ? new Cell(mini * 3 + w, l) : new Cell(l, mini * 3 + w);
                var clue = strategyUser.StartState[cell.Row, cell.Column];
                if (clue == 0) continue;

                clueCount++;
                differentClues += clue;
                if (clueCount > maxClueCount || differentClues.Count > maxDifferentClueCount) return false;

                _cluesBuffer.Add(cell);
            }
        }

        return true;
    }
}

public record BandPatternCandidate(BandPattern Pattern, int[] BoxKey, int[] WidthKey, int[] LengthKey, int Mini,
    Unit Unit);

public class BandUniquenessClueCoverReportBuilder : IChangeReportBuilder<IUpdatableSudokuSolvingState, ISudokuHighlighter>
{
    private readonly BandPatternCandidate _candidate;

    public BandUniquenessClueCoverReportBuilder(BandPatternCandidate candidate)
    {
        _candidate = candidate;
    }

    public ChangeReport<ISudokuHighlighter> Build(IReadOnlyList<SolverProgress> changes, IUpdatableSudokuSolvingState snapshot)
    {
        return new ChangeReport<ISudokuHighlighter>( "", lighter =>
        {
            var boxes = _candidate.Pattern.PlacementsWithKey(_candidate.BoxKey);

            for (int i = 0; i < 3; i++)
            {
                foreach (var key in boxes[i].Keys)
                {
                    var cell = key.Transform(_candidate.WidthKey, _candidate.LengthKey).ToCell(_candidate.Mini,
                        i, _candidate.Unit);
                    
                    lighter.HighlightCell(cell, ChangeColoration.CauseOffTwo);
                }
            }
            
            ChangeReportHelper.HighlightChanges(lighter, changes);
        });
    }
}