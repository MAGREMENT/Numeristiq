﻿using System.Collections.Generic;
using Model.Core;
using Model.Core.Changes;
using Model.Core.Explanations;
using Model.Core.Highlighting;
using Model.Core.Settings;
using Model.Core.Settings.Types;
using Model.Sudokus.Solver.PossibilitySets;
using Model.Sudokus.Solver.Utility;
using Model.Sudokus.Solver.Utility.AlmostLockedSets;
using Model.Utility;
using Model.Utility.BitSets;
using Model.Utility.Collections;

namespace Model.Sudokus.Solver.Strategies;

public class AlignedPairExclusionStrategy : SudokuStrategy
{
    public const string OfficialName = "Aligned Pair Exclusion";
    private const InstanceHandling DefaultInstanceHandling = InstanceHandling.FirstOnly;

    private readonly IntSetting _maxAlsSize;
    
    public AlignedPairExclusionStrategy(int maxAlsSize) : base(OfficialName, Difficulty.Hard, DefaultInstanceHandling)
    {
        _maxAlsSize = new IntSetting("Max ALS Size", "The maximum size for the almost locked sets",
            new SliderInteractionInterface(2, 5, 1), maxAlsSize);
    }

    public override IEnumerable<ISetting> EnumerateSettings()
    {
        yield return _maxAlsSize;
    }

    public override void Apply(ISudokuSolverData solverData)
    {
        for (int start1 = 0; start1 < 9; start1 += 3)
        {
            for (int start2 = 0; start2 < 9; start2 += 3)
            {
                for (int i = 0; i < 3; i++)
                {
                    for (int j = 0; j < 2; j++)
                    {
                        for (int k = j + 1; k < 3; k++)
                        {
                            var r = start1 + i;
                            var c1 = start2 + j;
                            var c2 = start2 + k;

                            if (solverData.Sudoku[r, c1] == 0 && solverData.Sudoku[r, c2] == 0 &&
                                Search(solverData, r, c1, r, c2)) return;

                            var c = start2 + i;
                            var r1 = start1 + j;
                            var r2 = start1 + k;

                            if (solverData.Sudoku[r1, c] == 0 && solverData.Sudoku[r2, c] == 0 &&
                                Search(solverData, r1, c, r2, c)) return;
                        }
                    }
                }
            }

            for (int u = 0; u < 2; u++)
            {
                for (int v = u + 1; v < 3; v++)
                {
                    var unit1 = start1 + u;
                    var unit2 = start1 + v;

                    for (int i = 0; i < 9; i++)
                    {
                        if (solverData.Sudoku[unit1, i] == 0)
                        {
                            for (int j = 0; j < 9; j++)
                            {
                                if (i / 3 == j / 3 || solverData.Sudoku[unit2, j] != 0) continue;

                                if (Search(solverData, unit1, i, unit2, j)) return;
                            }
                        }

                        if (solverData.Sudoku[i, unit1] == 0)
                        {
                            for (int j = 0; j < 9; j++)
                            {
                                if (i / 3 == j / 3 || solverData.Sudoku[j, unit2] != 0) continue;

                                if (Search(solverData, i, unit1, j, unit2)) return;
                            }
                        }
                    }
                }
            }
        }
    }

    private bool Search(ISudokuSolverData solverData, int row1, int col1, int row2, int col2)
    {
        var shared = new List<Cell>(SudokuUtility.SharedSeenEmptyCells(solverData, row1, col1, row2, col2));

        var poss1 = solverData.PossibilitiesAt(row1, col1);
        var poss2 = solverData.PossibilitiesAt(row2, col2);
        var or = poss1 | poss2;
        
        if (shared.Count < poss1.Count || shared.Count < poss2.Count) return false;

        var inSameUnit = SudokuUtility.ShareAUnit(row1, col1, row2, col2);
        
        List<IPossibilitySet> usefulAls = new();
        HashSet<BiValue> forbidden = new();

        foreach (var als in AlmostNakedSetSearcher.InCells(solverData, _maxAlsSize.Value, 1, shared))
        {
            int i = 0;
            bool useful = false;
            while (als.EveryPossibilities().HasNextPossibility(ref i))
            {
                if (!or.Contains(i)) continue;
                
                int j = i;
                while (als.EveryPossibilities().HasNextPossibility(ref j))
                {
                    if (!or.Contains(j)) continue;
                    
                    if(forbidden.Add(new BiValue(i, j))) useful = true;
                }
            }

            if (useful) usefulAls.Add(als);
        }

        SearchForElimination(solverData, poss1, poss2, forbidden, row1, col1, inSameUnit);
        SearchForElimination(solverData, poss2, poss1, forbidden, row2, col2, inSameUnit);

        if (!solverData.ChangeBuffer.NeedCommit()) return false;

        solverData.ChangeBuffer.Commit(new AlignedPairExclusionReportBuilder(usefulAls, row1, col1, row2, col2));
        return StopOnFirstCommit;
    }

    private void SearchForElimination(ISudokuSolverData solverData, ReadOnlyBitSet16 poss1,
        ReadOnlyBitSet16 poss2, HashSet<BiValue> forbidden, int row, int col, bool inSameUnit)
    {
        foreach (var p1 in poss1.EnumeratePossibilities())
        {
            bool toDelete = true;
            foreach (var p2 in poss2.EnumeratePossibilities())
            {
                if (p1 == p2 && inSameUnit) continue;
                if (!forbidden.Contains(new BiValue(p1, p2)))
                {
                    toDelete = false;
                    break;
                }
            }
            
            if(toDelete) solverData.ChangeBuffer.ProposePossibilityRemoval(p1, row, col);
        }
    }
}

public class AlignedPairExclusionReportBuilder : IChangeReportBuilder<NumericChange, ISudokuSolvingState, ISudokuHighlighter>
{
    private readonly List<IPossibilitySet> _als;
    private readonly int _row1;
    private readonly int _col1;
    private readonly int _row2;
    private readonly int _col2;

    public AlignedPairExclusionReportBuilder(List<IPossibilitySet> als, int row1, int col1, int row2, int col2)
    {
        _als = als;
        _row1 = row1;
        _col1 = col1;
        _row2 = row2;
        _col2 = col2;
    }
    
    public ChangeReport<ISudokuHighlighter> BuildReport(IReadOnlyList<NumericChange> changes, ISudokuSolvingState snapshot)
    {
        return new ChangeReport<ISudokuHighlighter>($"Aligned Pair Exclusion in r{_row1 + 1}c{_col1 + 1} and " +
                                                    $"r{_row2 + 1}c{_col2 + 1} with {_als.ToStringSequence(", ")}",
            lighter =>
        {
            lighter.HighlightCell(_row1, _col1, StepColor.Neutral);
            lighter.HighlightCell(_row2, _col2, StepColor.Neutral);

            var removed = new ReadOnlyBitSet16();
            foreach (var change in changes) removed += change.Number;
            
            int color = (int) StepColor.Cause1;
            foreach (var als in _als)
            {
                if (!removed.ContainsAny(als.EveryPossibilities())) continue;
                foreach (var coord in als.EnumerateCells())
                {
                    lighter.HighlightCell(coord.Row, coord.Column, (StepColor) color);
                }

                color++;
            }

            ChangeReportHelper.HighlightChanges(lighter, changes);
        }, Explanation(changes, snapshot));
    }

    private Explanation<ISudokuHighlighter> Explanation(IReadOnlyList<NumericChange> changes,
        ISudokuSolvingState snapshot)
    {
        var result = new Explanation<ISudokuHighlighter>().Append(new Cell(_row1, _col1))
            .Append(" and ").Append(new Cell(_row2, _col2))
            .Append(" both see almost locked sets that prevents ").Append(SudokuUtility.Cast(changes))
            .Append(" from being possible.\nThe almost locked sets are the following :");

        foreach (var als in _als)
        {
            result.Append("\n - ").Append(als).Append(" prevents ");

            bool firstDone = false;
            int i = 0;
            while (als.EveryPossibilities().HasNextPossibility(ref i))
            {
                int j = i;
                while (als.EveryPossibilities().HasNextPossibility(ref j))
                {
                    if (snapshot.PossibilitiesAt(_row1, _col1).Contains(i)
                        && snapshot.PossibilitiesAt(_row2, _col2).Contains(j))
                    {
                        if (firstDone) result.Append(" and ");
                        else firstDone = true;
                        
                        result.Append(new CellPossibility(_row1, _col1, i),
                            new CellPossibility(_row2, _col2, j));
                    }


                    if (snapshot.PossibilitiesAt(_row2, _col2).Contains(i)
                        && snapshot.PossibilitiesAt(_row1, _col1).Contains(j))
                    {
                        if (firstDone) result.Append(" and ");
                        else firstDone = true;
                        
                        result.Append(new CellPossibility(_row2, _col2, i),
                            new CellPossibility(_row1, _col1, j));
                    }
                }
            }
        }
        
        return result;
    }
    
    public Clue<ISudokuHighlighter> BuildClue(IReadOnlyList<NumericChange> changes, ISudokuSolvingState snapshot)
    {
        return new Clue<ISudokuHighlighter>(lighter =>
        {
            lighter.EncircleCell(_row1, _col1);
            lighter.EncircleCell(_row2, _col2);
        }, "These 2 cells might have eliminations");
    }
}

