﻿using System.Collections.Generic;
using Model.Core;
using Model.Core.Changes;
using Model.Core.Explanations;
using Model.Core.Highlighting;
using Model.Utility;
using Model.Utility.BitSets;
using Model.Utility.Collections;

namespace Model.Sudokus.Solver.Strategies.UniquenessClueCover.PatternCollections.Bands;

public class BandCollection : IPatternCollection
{
    private readonly BandPattern[] _collection;
    private readonly List<BandPatternCandidate> _candidates = new();

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

    public bool Filter(ISudokuSolverData solverData)
    {
        _candidates.Clear();
        for (int mini = 0; mini < 3; mini++)
        {
            if(CheckForCandidates(solverData, mini, Unit.Row)) return true;
            if(CheckForCandidates(solverData, mini, Unit.Column)) return true;
        }

        return false;
    }

    public bool Apply(ISudokuSolverData solverData)
    {
        foreach (var candidate in _candidates)
        {
            if (Try(solverData, candidate)) return true;
        }

        return false;
    }

    private bool CheckForCandidates(ISudokuSolverData solverData, int mini, Unit unit)
    {
        foreach (var pattern in _collection)
        {
            if (!DoesClueNumbersMatch(solverData, mini, unit, pattern.ClueCount, pattern.DifferentClueCount)) continue;

            foreach (var boxKey in OrderKeyGenerator.GenerateAll())
            {
                foreach (var widthKey in OrderKeyGenerator.GenerateAll())
                {
                    foreach (var lengthKey1 in OrderKeyGenerator.GenerateAll())
                    {
                        if(!AreCluesMatching(solverData, pattern, boxKey, widthKey, lengthKey1, mini, unit, 0)) continue;
                        
                        foreach (var lengthKey2 in OrderKeyGenerator.GenerateAll())
                        {
                            if(!AreCluesMatching(solverData, pattern, boxKey, widthKey, lengthKey2, mini, unit, 1)) continue;
                            
                            foreach (var lengthKey3 in OrderKeyGenerator.GenerateAll())
                            {
                                if(!AreCluesMatching(solverData, pattern, boxKey, widthKey, lengthKey3, mini, unit, 2)) continue;
                                
                                if (TryAndAddToCandidates(solverData, pattern, boxKey, widthKey, new []
                                    {
                                        lengthKey1, lengthKey2, lengthKey3
                                    }, mini, unit)) return true;
                            }
                        }
                    }
                }
            }
        }

        return false;
    }
    
    private bool DoesClueNumbersMatch(ISudokuSolverData solverData, int mini, Unit unit, int[] maxClueCount,
        int maxDifferentClueCount)
    {
        int[] ccs = { 0, 0, 0 }; 
        var differentClues = new ReadOnlyBitSet16();
        for (int w = 0; w < 3; w++)
        {
            for (int cc = 0; cc < 3; cc++)
            {
                var start = cc * 3;
                var end = start + 3;
                for (int l = start; l < end; l++)
                {
                    var cell = unit == Unit.Row ? new Cell(mini * 3 + w, l) : new Cell(l, mini * 3 + w);
                    var clue = solverData.StartState[cell.Row, cell.Column];
                    if (clue == 0) continue;

                    ccs[cc]++;
                    differentClues += clue;
                    if (ccs[cc] > maxClueCount[0] || differentClues.Count > maxDifferentClueCount) return false;
                }
            }
        }

        return true;
    }

    private bool AreCluesMatching(ISudokuSolverData solverData, BandPattern pattern, int[] boxKey, int[] widthKey,
        int[] lengthKey, int mini, Unit unit, int boxNumber)
    {
        var box = pattern.PlacementsWithKey(boxKey)[boxNumber];
        for (int w = 0; w < 3; w++)
        {
            for (int l = 0; l < 3; l++)
            {
                var cell = unit == Unit.Row 
                    ? new Cell(mini * 3 + w, boxNumber * 3 + l)
                    : new Cell(boxNumber * 3 + l, mini * 3 + w);
                if (solverData.StartState[cell.Row, cell.Column] == 0) continue;

                var bp = new BoxPosition(w, l).UnTransform(widthKey, lengthKey);
                if(!box.ContainsKey(bp)) return false;
            }
        }

        return true;
    }

    private bool TryAndAddToCandidates(ISudokuSolverData solverData, BandPattern pattern, int[] boxKey, int[] widthKey,
        int[][] lengthKeys, int mini, Unit unit)
    {
        var boxes = pattern.PlacementsWithKey(boxKey);
        int[] numberEquivalence = new int[pattern.DifferentClueCount];
        bool ok = true;

        for (int i = 0; i < 3; i++)
        {
            foreach (var entry in boxes[i])
            {
                var cell = entry.Key.Transform(widthKey, lengthKeys[i]).ToCell(mini, i, unit);

                var solved = solverData.Sudoku[cell.Row, cell.Column];
                if (solved == 0) ok = false;

                if (numberEquivalence[entry.Value] == 0) numberEquivalence[entry.Value] = solved;
                else if (numberEquivalence[entry.Value] != solved) return false;
            }
        }

        var candidate = new BandPatternCandidate(pattern, boxKey, widthKey, lengthKeys, mini, unit);
        _candidates.Add(candidate);

        return ok && Process(solverData, candidate, numberEquivalence);
    }
    
    private bool Try(ISudokuSolverData solverData, BandPatternCandidate candidate)
    {
        var boxes = candidate.Pattern.PlacementsWithKey(candidate.BoxKey);
        int[] numberEquivalence = new int[candidate.Pattern.DifferentClueCount];

        for (int i = 0; i < 3; i++)
        {
            foreach (var entry in boxes[i])
            {
                var cell = entry.Key.Transform(candidate.WidthKey, candidate.LengthKeys[i]).ToCell(candidate.Mini,
                    i, candidate.Unit);

                var solved = solverData.Sudoku[cell.Row, cell.Column];
                if (solved == 0) return false;

                if (numberEquivalence[entry.Value] == 0) numberEquivalence[entry.Value] = solved;
                else if (numberEquivalence[entry.Value] != solved) return false;
            }
        }
        
        return Process(solverData, candidate, numberEquivalence);
    }

    private bool Process(ISudokuSolverData solverData, BandPatternCandidate candidate, int[] numberEquivalence)
    {
        var eliminations = candidate.Pattern.EliminationsWithKey(candidate.BoxKey);

        for (int i = 0; i < 3; i++)
        {
            foreach (var entry in eliminations[i])
            {
                var cell = entry.Key.Transform(candidate.WidthKey, candidate.LengthKeys[i]).ToCell(candidate.Mini,
                    i, candidate.Unit);

                foreach (var p in entry.Value.EveryElimination(numberEquivalence))
                {
                    solverData.ChangeBuffer.ProposePossibilityRemoval(p, cell);
                }
            }
        }

        if (!solverData.ChangeBuffer.NeedCommit()) return false;
        
        solverData.ChangeBuffer.Commit(new BandUniquenessClueCoverReportBuilder(candidate));
        return Strategy!.StopOnFirstCommit;
    }
}

public record BandPatternCandidate(BandPattern Pattern, int[] BoxKey, int[] WidthKey, int[][] LengthKeys, int Mini,
    Unit Unit);

public class BandUniquenessClueCoverReportBuilder : IChangeReportBuilder<NumericChange, ISudokuSolvingState, ISudokuHighlighter>
{
    private readonly BandPatternCandidate _candidate;

    public BandUniquenessClueCoverReportBuilder(BandPatternCandidate candidate)
    {
        _candidate = candidate;
    }

    public ChangeReport<ISudokuHighlighter> BuildReport(IReadOnlyList<NumericChange> changes, ISudokuSolvingState snapshot)
    {
        var cells = GetCells();
        
        return new ChangeReport<ISudokuHighlighter>(Description(cells), lighter =>
        {
            foreach (var cell in cells)
            {
                lighter.HighlightCell(cell, StepColor.Cause2);
            }
            
            ChangeReportHelper.HighlightChanges(lighter, changes);
        }, Explanation(cells));
    }

    private string Description(List<Cell> cells)
    {
        return $"Uniqueness Clue Cover pattern match cells {cells.ToStringSequence(", ")}";
    }

    private static Explanation<ISudokuHighlighter> Explanation(List<Cell> cells)
    {
        if (cells.Count == 0) return Explanation<ISudokuHighlighter>.Empty;

        var result = new Explanation<ISudokuHighlighter>().Append(cells[0]);
        for (int i = 1; i < cells.Count; i++)
        {
            result.Append(", ").Append(cells[i]);
        }

        return result.Append(" matches a UCC pattern, leading to the eliminations made by that specific pattern");
    }
    
    public Clue<ISudokuHighlighter> BuildClue(IReadOnlyList<NumericChange> changes, ISudokuSolvingState snapshot)
    {
        var cells = GetCells();

        return new Clue<ISudokuHighlighter>(lighter =>
        {
            foreach (var cell in cells)
            {
                lighter.EncircleCell(cell);
            }
        }, "Those cells seems to match a certain pattern");
    }

    private List<Cell> GetCells()
    {
        List<Cell> cells = new();
        
        var boxes = _candidate.Pattern.PlacementsWithKey(_candidate.BoxKey);

        for (int i = 0; i < 3; i++)
        {
            foreach (var key in boxes[i].Keys)
            {
                cells.Add(key.Transform(_candidate.WidthKey, _candidate.LengthKeys[i]).ToCell(_candidate.Mini,
                    i, _candidate.Unit));
            }
        }

        return cells;
    }
}