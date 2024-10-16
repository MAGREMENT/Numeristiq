﻿using System.Collections.Generic;
using Model.Core;
using Model.Core.Changes;
using Model.Core.Highlighting;
using Model.Core.Settings;
using Model.Core.Settings.Types;
using Model.Sudokus.Solver.PossibilitySets;
using Model.Sudokus.Solver.Utility;
using Model.Utility;
using Model.Utility.BitSets;

namespace Model.Sudokus.Solver.Strategies;

public class AlmostLockedSetsStrategy : SudokuStrategy
{
    public const string OfficialName = "Almost Locked Sets";
    private const InstanceHandling DefaultInstanceHandling = InstanceHandling.FirstOnly;

    private readonly IntSetting _maxAlsSize;
    
    public AlmostLockedSetsStrategy(int maxAlsSize) : base(OfficialName, Difficulty.Extreme, DefaultInstanceHandling)
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
        foreach (var linked in solverData.PreComputer.ConstructAlmostLockedSetGraph(_maxAlsSize.Value))
        {
            if (linked.RestrictedCommons.Count > 2) continue;

            var restrictedCommons = linked.RestrictedCommons;
            var one = linked.One;
            var two = linked.Two;
            
            foreach (var restrictedCommon in restrictedCommons.EnumeratePossibilities())
            {
                foreach (var possibility in one.EnumeratePossibilities())
                {
                    if (!two.Contains(possibility) || possibility == restrictedCommon) continue;

                    List<Cell> coords = new();
                    coords.AddRange(one.EnumerateCells(possibility));
                    coords.AddRange(two.EnumerateCells(possibility));

                    foreach (var coord in SudokuUtility.SharedSeenCells(coords))
                    {
                        solverData.ChangeBuffer.ProposePossibilityRemoval(possibility, coord.Row, coord.Column);
                    }
                }
            }

            if (restrictedCommons.Count == 2)
            {
                foreach (var possibility in one.EnumeratePossibilities())
                {
                    if (restrictedCommons.Contains(possibility) || two.Contains(possibility)) continue;

                    foreach (var coord in SudokuUtility.SharedSeenCells(new List<Cell>(one.EnumerateCells(possibility))))
                    {
                        solverData.ChangeBuffer.ProposePossibilityRemoval(possibility, coord.Row, coord.Column);
                    }
                }
                    
                foreach (var possibility in two.EnumeratePossibilities())
                {
                    if (restrictedCommons.Contains(possibility) || one.Contains(possibility)) continue;

                    foreach (var coord in SudokuUtility.SharedSeenCells(new List<Cell>(two.EnumerateCells(possibility))))
                    {
                        solverData.ChangeBuffer.ProposePossibilityRemoval(possibility, coord.Row, coord.Column);
                    }
                }
            }

            if (solverData.ChangeBuffer.NeedCommit())
            {
                solverData.ChangeBuffer.Commit(new AlmostLockedSetsReportBuilder(one, two, restrictedCommons));
                if (StopOnFirstCommit) return;
            }
        }
    }
}

public class AlmostLockedSetsReportBuilder : IChangeReportBuilder<NumericChange, ISudokuSolvingState, ISudokuHighlighter>
{
    private readonly IPossibilitySet _one;
    private readonly IPossibilitySet _two;
    private readonly ReadOnlyBitSet16 _restrictedCommons;

    public AlmostLockedSetsReportBuilder(IPossibilitySet one, IPossibilitySet two, ReadOnlyBitSet16 restrictedCommons)
    {
        _one = one;
        _two = two;
        _restrictedCommons = restrictedCommons;
    }

    public ChangeReport<ISudokuHighlighter> BuildReport(IReadOnlyList<NumericChange> changes, ISudokuSolvingState snapshot)
    {
        return new ChangeReport<ISudokuHighlighter>($"Almost Hidden Sets : {_one} and {_two}", lighter =>
        {
            foreach (var coord in _one.EnumerateCells())
            {
                lighter.HighlightCell(coord.Row, coord.Column, StepColor.Cause1);
            }

            foreach (var coord in _two.EnumerateCells())
            {
                lighter.HighlightCell(coord.Row, coord.Column, StepColor.Cause2);
            }

            foreach (var possibility in _restrictedCommons.EnumeratePossibilities())
            {
                foreach (var coord in _one.EnumerateCells())
                {
                    if(snapshot.PossibilitiesAt(coord).Contains(possibility))
                        lighter.HighlightPossibility(possibility, coord.Row, coord.Column, StepColor.Neutral);
                }
                
                foreach (var coord in _two.EnumerateCells())
                {
                    if(snapshot.PossibilitiesAt(coord).Contains(possibility))
                        lighter.HighlightPossibility(possibility, coord.Row, coord.Column, StepColor.Neutral);
                }
            }

            ChangeReportHelper.HighlightChanges(lighter, changes);
        });
    }
    
    public Clue<ISudokuHighlighter> BuildClue(IReadOnlyList<NumericChange> changes, ISudokuSolvingState snapshot)
    {
        return Clue<ISudokuHighlighter>.Default();
    }
}