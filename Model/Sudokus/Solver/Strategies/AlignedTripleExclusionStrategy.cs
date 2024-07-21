using System;
using System.Collections.Generic;
using System.Linq;
using Model.Core;
using Model.Core.Changes;
using Model.Core.Highlighting;
using Model.Core.Settings;
using Model.Core.Settings.Types;
using Model.Sudokus.Solver.PossibilityPosition;
using Model.Sudokus.Solver.Utility;
using Model.Utility;
using Model.Utility.BitSets;
using Model.Utility.Collections;

namespace Model.Sudokus.Solver.Strategies;

public class AlignedTripleExclusionStrategy : SudokuStrategy
{
    public const string OfficialName = "Aligned Triple Exclusion";
    private const InstanceHandling DefaultInstanceHandling = InstanceHandling.FirstOnly;

    private readonly IntSetting _minSharedSeenCells;
    private readonly IntSetting _maxAlsSize;
    private readonly IntSetting _maxAalsSize;
    
    public AlignedTripleExclusionStrategy(int minSharedSeenCells, int maxAlsSize, int maxAalsSize) : base(OfficialName, StepDifficulty.Hard, DefaultInstanceHandling)
    {
        _minSharedSeenCells = new IntSetting("Minimum shared seen cells", "The minimum amount of cells" +
                                                                          "the base of the pattern must see together",
            new SliderInteractionInterface(5, 12, 1), minSharedSeenCells);
        AddSetting(_minSharedSeenCells);
        _maxAlsSize = new IntSetting("Max ALS Size", "The maximum size for the almost locked sets",
            new SliderInteractionInterface(2, 5, 1), maxAlsSize);
        AddSetting(_maxAlsSize);
        _maxAalsSize = new IntSetting("Max AALS Size", "The maximum size for the almost almost locked sets",
            new SliderInteractionInterface(2, 5, 1), maxAalsSize);
        AddSetting(_maxAalsSize);
    }

    public override void Apply(ISudokuSolverData solverData)
    {
        for (int start1 = 0; start1 < 9; start1 += 3)
        {
            if (_minSharedSeenCells.Value > 12) continue;
            
            //Aligned in unit & box => 12 ssc
            for (int start2 = 0; start2 < 9; start2 += 3)
            {
                for (int i = 0; i < 3; i++)
                {
                    var c1 = new Cell(start1 + i, start2);
                    var c2 = new Cell(start1 + i, start2 + 1);
                    var c3 = new Cell(start1 + i, start2 + 2);
                    
                    if (solverData.Sudoku[c1.Row, c1.Column] == 0 && solverData.Sudoku[c2.Row, c2.Column] == 0 &&
                        solverData.Sudoku[c3.Row, c3.Column] == 0 && Search(solverData, c1, c2, c3)) return;

                    c1 = new Cell(start1, start2 + i);
                    c2 = new Cell(start1 + 1, start2 + i);
                    c3 = new Cell(start1 + 2, start2 + i);
                    
                    if (solverData.Sudoku[c1.Row, c1.Column] == 0 && solverData.Sudoku[c2.Row, c2.Column] == 0 &&
                        solverData.Sudoku[c3.Row, c3.Column] == 0 && Search(solverData, c1, c2, c3)) return;
                }
            }
            
            if (_minSharedSeenCells.Value > 6) continue;
            
            //2 aligned boxes & 2 in same unit => 6 ssc
            for (int u = 0; u < 2; u++)
            {
                for (int v = u + 1; v < 3; v++)
                {
                    var unit1 = start1 + u;
                    var unit2 = start1 + v;

                    for (int i = 0; i < 9; i++)
                    {
                        int s = i / 3 * 3;
                        for (int j = s; j < s + 3; j++)
                        {
                            if (i == j) continue;
                            
                            if (solverData.Sudoku[unit1, i] == 0 && solverData.Sudoku[unit1, j] == 0)
                            {
                                for (int k = 0; k < 9; k++)
                                {
                                    if(k / 3 == i / 3 || solverData.Sudoku[unit2, k] != 0) continue;

                                    if (Search(solverData, new Cell(unit1, i), new Cell(unit1, j),
                                            new Cell(unit2, k))) return;
                                }
                            }
                            
                            if (solverData.Sudoku[unit2, i] == 0 && solverData.Sudoku[unit2, j] == 0)
                            {
                                for (int k = 0; k < 9; k++)
                                {
                                    if(k / 3 == i / 3 || solverData.Sudoku[unit1, k] != 0) continue;

                                    if (Search(solverData, new Cell(unit2, i), new Cell(unit2, j),
                                            new Cell(unit1, k))) return;
                                }
                            }
                            
                            if (solverData.Sudoku[i, unit1] == 0 && solverData.Sudoku[j, unit1] == 0)
                            {
                                for (int k = 0; k < 9; k++)
                                {
                                    if(k / 3 == i / 3 || solverData.Sudoku[k, unit2] != 0) continue;

                                    if (Search(solverData, new Cell(i, unit1), new Cell(j, unit1),
                                            new Cell(k, unit2))) return;
                                }
                            }
                            
                            if (solverData.Sudoku[i, unit2] == 0 && solverData.Sudoku[j, unit2] == 0)
                            {
                                for (int k = 0; k < 9; k++)
                                {
                                    if(k / 3 == i / 3 || solverData.Sudoku[k, unit1] != 0) continue;

                                    if (Search(solverData, new Cell(i, unit2), new Cell(j, unit2),
                                            new Cell(k, unit1))) return;
                                }
                            }
                        }
                    }
                }
            }
            
            if (_minSharedSeenCells.Value > 5) continue;

            //2 aligned boxes & different units => 5 ssc
            for (int start2 = 0; start2 < 9; start2 += 3)
            {
                for (int u = 0; u < 2; u++)
                {
                    for (int v = u + 1; v < 3; v++)
                    {
                        var unit1 = start1 + u;
                        var unit2 = start1 + v;
                        var unit3 = start1 + LastUnitInBox(u, v);

                        for (int i = 0; i < 3; i++)
                        {
                            for (int j = 0; j < 3; j++)
                            {
                                var other1 = start2 + i;
                                var other2 = start2 + j;

                                if (solverData.Sudoku[unit1, other1] == 0 & solverData.Sudoku[unit2, other2] == 0)
                                {
                                    for (int startOther = 0; startOther < 9; startOther += 3)
                                    {
                                        if (startOther == start2) continue;

                                        for (int k = 0; k < 3; k++)
                                        {
                                            var other3 = startOther + k;
                                            if(solverData.Sudoku[unit3, other3] != 0) continue;
                                            
                                            if(Search(solverData, new Cell(unit1, other1),
                                                   new Cell(unit2, other2), new Cell(unit3, other3))) return;
                                        }
                                    }
                                }
                                
                                if (solverData.Sudoku[other1, unit1] == 0 & solverData.Sudoku[other2, unit2] == 0)
                                {
                                    for (int startOther = 0; startOther < 3; startOther++)
                                    {
                                        if (startOther == start2) continue;

                                        for (int k = 0; k < 3; k++)
                                        {
                                            var other3 = startOther + k;
                                            if(solverData.Sudoku[other3, unit3] != 0) continue;
                                            
                                            if(Search(solverData, new Cell(other1, unit1),
                                                   new Cell(other2, unit2), new Cell(other3, unit3))) return;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
    }

    private int LastUnitInBox(int one, int two)
    {
        var add = one + two;
        switch (add)
        {
            case 1 : return 2;
            case 2 : return 1;
            case 3 : return 0;
            default: throw new ArgumentException();
        }
    }

    private bool Search(ISudokuSolverData solverData, Cell c1, Cell c2, Cell c3)
    {
        var ssc = SudokuCellUtility.SharedSeenEmptyCells(solverData, c1, c2, c3).ToArray();

        var poss1 = solverData.PossibilitiesAt(c1);
        var poss2 = solverData.PossibilitiesAt(c2);
        var poss3 = solverData.PossibilitiesAt(c3);
        var or = poss1.OrMulti(poss2, poss3);

        if (ssc.Length < poss1.Count || ssc.Length < poss2.Count || ssc.Length < poss3.Count) return false;

        List<IPossibilitiesPositions> usefulThings = new();
        HashSet<TriValue> forbiddenTri = new();
        HashSet<BiValue> forbiddenBi = new();

        var searcher = solverData.AlmostNakedSetSearcher;

        foreach (var aals in searcher.InCells(ssc, _maxAalsSize.Value, 2))
        {
            int i = 0;
            bool useful = false;
            while (aals.Possibilities.HasNextPossibility(ref i))
            {
                if (!or.Contains(i)) continue;

                int j = i;
                while (aals.Possibilities.HasNextPossibility(ref j))
                {
                    if (!or.Contains(j)) continue;

                    int k = j;
                    while (aals.Possibilities.HasNextPossibility(ref k))
                    {
                        if (!or.Contains(k)) continue;

                        if (forbiddenTri.Add(new TriValue(i, j, k))) useful = true;
                    }
                }
            }

            if (useful) usefulThings.Add(aals);
        }

        foreach (var als in searcher.InCells(ssc, _maxAlsSize.Value, 1))
        {
            int i = 0;
            bool useful = false;
            while (als.Possibilities.HasNextPossibility(ref i))
            {
                if (!or.Contains(i)) continue;
                
                int j = i;
                while (als.Possibilities.HasNextPossibility(ref j))
                {
                    if (!or.Contains(j)) continue;

                    if (forbiddenBi.Add(new BiValue(i, j))) useful = true;
                }
            }

            if (useful) usefulThings.Add(als);
        }
        
        SearchForElimination(solverData, poss1, poss2, poss3, c1, c2, c3, forbiddenTri, forbiddenBi);
        SearchForElimination(solverData, poss2, poss1, poss3, c2, c1, c3, forbiddenTri, forbiddenBi);
        SearchForElimination(solverData, poss3, poss2, poss1, c3, c2, c1, forbiddenTri, forbiddenBi);

        return solverData.ChangeBuffer.NotEmpty() && solverData.ChangeBuffer.Commit( 
            new AlignedTripleExclusionReportBuilder(c1, c2, c3, usefulThings)) && StopOnFirstPush;
    }

    private void SearchForElimination(ISudokuSolverData solverData, ReadOnlyBitSet16 poss1, ReadOnlyBitSet16 poss2,
        ReadOnlyBitSet16 poss3, Cell c1, Cell c2, Cell c3, HashSet<TriValue> forbiddenTri, HashSet<BiValue> forbiddenBi)
    {
        foreach (var p1 in poss1.EnumeratePossibilities())
        {
            var toDelete = true;
            foreach (var p2 in poss2.EnumeratePossibilities())
            {
                if (p1 == p2 && SudokuCellUtility.ShareAUnit(c1, c2)) continue;

                if (forbiddenBi.Contains(new BiValue(p1, p2))) continue;

                foreach (var p3 in poss3.EnumeratePossibilities())
                {
                    if((p1 == p3 && SudokuCellUtility.ShareAUnit(c1, c3)) || (p2 == p3 && SudokuCellUtility.ShareAUnit(c2, c3))) continue;

                    if (forbiddenBi.Contains(new BiValue(p1, p3)) 
                        || forbiddenBi.Contains(new BiValue(p2, p3))) continue;

                    if (!forbiddenTri.Contains(new TriValue(p1, p2, p3)))
                    {
                        toDelete = false;
                        break;
                    }
                }

                if (!toDelete) break;
            }

            if (toDelete) solverData.ChangeBuffer.ProposePossibilityRemoval(p1, c1);
        }
    }
}

public readonly struct TriValue
{
    public TriValue(int one, int two, int three)
    {
        One = one;
        Two = two;
        Three = three;
    }

    public int One { get; }
    public int Two { get; }
    public int Three { get; }

    public override int GetHashCode()
    {
        return One ^ Two ^ Three;
    }

    public override bool Equals(object? obj)
    {
        if (obj is not TriValue tri) return false;
        int i = 1 << One | 1 << Two | 1 << Three;
        int j = 1 << tri.One | 1 << tri.Two | 1 << tri.Three;
        return i == j;
    }

    public static bool operator ==(TriValue left, TriValue right)
    {
        int i = 1 << left.One | 1 << left.Two | 1 << left.Three;
        int j = 1 << right.One | 1 << right.Two | 1 << right.Three;
        return i == j;
    }

    public static bool operator !=(TriValue left, TriValue right)
    {
        return !(left == right);
    }
}

public class AlignedTripleExclusionReportBuilder : IChangeReportBuilder<NumericChange, ISudokuSolvingState, ISudokuHighlighter>
{
    private readonly Cell _c1;
    private readonly Cell _c2;
    private readonly Cell _c3;
    private readonly List<IPossibilitiesPositions> _useful;

    public AlignedTripleExclusionReportBuilder(Cell c1, Cell c2, Cell c3, List<IPossibilitiesPositions> useful)
    {
        _c1 = c1;
        _c2 = c2;
        _c3 = c3;
        _useful = useful;
    }

    public ChangeReport<ISudokuHighlighter> BuildReport(IReadOnlyList<NumericChange> changes, ISudokuSolvingState snapshot)
    {
        return new ChangeReport<ISudokuHighlighter>($"Aligned Triple Exclusion in {_c1}, {_c2} and {_c3} with "
            + _useful.ToStringSequence(", "), lighter =>
        {
            lighter.HighlightCell(_c1, StepColor.Neutral);
            lighter.HighlightCell(_c2, StepColor.Neutral);
            lighter.HighlightCell(_c3, StepColor.Neutral);

            var removed = new ReadOnlyBitSet16();
            foreach (var change in changes) removed += change.Number;
            
            int color = (int) StepColor.Cause1;
            foreach (var als in _useful)
            {
                if (!removed.ContainsAny(als.Possibilities)) continue;
                foreach (var coord in als.EnumerateCells())
                {
                    lighter.HighlightCell(coord.Row, coord.Column, (StepColor) color);
                }

                color++;
            }

            ChangeReportHelper.HighlightChanges(lighter, changes);
        });
    }
    
    public Clue<ISudokuHighlighter> BuildClue(IReadOnlyList<NumericChange> changes, ISudokuSolvingState snapshot)
    {
        return Clue<ISudokuHighlighter>.Default();
    }
}