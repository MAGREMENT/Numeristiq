using System.Collections.Generic;
using Model.Core;
using Model.Core.Changes;
using Model.Core.Highlighting;
using Model.Sudokus.Solver.Position;
using Model.Utility;
using Model.Utility.BitSets;

namespace Model.Sudokus.Solver.Strategies;

/// <summary>
/// If a group of all instances of N different digits in a band (aka 3 rows or 3 columns in the same 3 boxes)
/// is spread over max N+1 mini-rows/-columns, then the group will contain at least one unavoidable set.
/// </summary>
public class MiniUniquenessStrategy : SudokuStrategy
{
    public const string OfficialName = "Mini-Uniqueness";
    private const InstanceHandling DefaultInstanceHandling = InstanceHandling.UnorderedAll;

    public MiniUniquenessStrategy() : base(OfficialName, StepDifficulty.Medium, DefaultInstanceHandling)
    {
        UniquenessDependency = UniquenessDependency.FullyDependent;
    }
    
    public override void Apply(ISudokuSolverData solverData)
    {
        for (int mini = 0; mini < 3; mini++)
        {
            var presence = new ReadOnlyBitSet16();
            var availability = new LinePositions();
            var singleAvailability = new LinePositions();

            for (int col = 0; col < 9; col++)
            {
                var availabilityCount = 0;
                
                for (int r = 0; r < 3; r++)
                {
                    var row = mini * 3 + r;
                    var solved = solverData.Sudoku[row, col];
                    
                    if (solved == 0) availabilityCount++;
                    else presence += solved;
                }

                if (availabilityCount > 0)
                {
                    availability.Add(col);
                    if (availabilityCount == 1) singleAvailability.Add(col);
                }
            }

            if (presence.Count <= 6 && availability.Count <= 9 - presence.Count + 2)
            {
                foreach (var col in singleAvailability)
                {
                    for (int r = 0; r < 3; r++)
                    {
                        var row = mini * 3 + r;
                        if (solverData.Sudoku[row, col] != 0) continue;

                        foreach (var p in presence.EnumeratePossibilities())
                        {
                            solverData.ChangeBuffer.ProposePossibilityRemoval(p, row, col);
                        }
                    }
                }

                if (solverData.ChangeBuffer.NotEmpty() && solverData.ChangeBuffer.Commit(
                        new MiniUniquenessReportBuilder(mini, Unit.Row, presence)) &&
                            StopOnFirstPush) return;
            }

            presence = new ReadOnlyBitSet16();
            availability.Void();
            singleAvailability.Void();
            for (int row = 0; row < 9; row++)
            {
                var availabilityCount = 0;
                
                for (int c = 0; c < 3; c++)
                {
                    var col = mini * 3 + c;
                    var solved = solverData.Sudoku[row, col];
                    
                    if (solved == 0) availabilityCount++;
                    else presence += solved;
                }

                if (availabilityCount > 0)
                {
                    availability.Add(row);
                    if (availabilityCount == 1) singleAvailability.Add(row);
                }
            }

            if (presence.Count <= 6 && availability.Count <= 9 - presence.Count + 2)
            {
                foreach (var row in singleAvailability)
                {
                    for (int c = 0; c < 3; c++)
                    {
                        var col = mini * 3 + c;
                        if (solverData.Sudoku[row, col] != 0) continue;

                        foreach (var p in presence.EnumeratePossibilities())
                        {
                            solverData.ChangeBuffer.ProposePossibilityRemoval(p, row, col);
                        }
                    }
                }

                if (solverData.ChangeBuffer.NotEmpty() && solverData.ChangeBuffer.Commit(
                        new MiniUniquenessReportBuilder(mini, Unit.Row, presence)) &&
                    StopOnFirstPush) return;
            }
        }
    }
}

public class MiniUniquenessReportBuilder : IChangeReportBuilder<NumericChange, ISudokuSolvingState, ISudokuHighlighter>
{
    private readonly int _mini;
    private readonly Unit _unit;
    private readonly ReadOnlyBitSet16 _presence;

    public MiniUniquenessReportBuilder(int mini, Unit unit, ReadOnlyBitSet16 presence)
    {
        _mini = mini;
        _unit = unit;
        _presence = presence;
    }

    public ChangeReport<ISudokuHighlighter> BuildReport(IReadOnlyList<NumericChange> changes, ISudokuSolvingState snapshot)
    {
        return new ChangeReport<ISudokuHighlighter>( "", lighter =>
        {
            int color = (int)ChangeColoration.CauseOffOne;
            foreach (var p in _presence.EnumeratePossibilities())
            {
                for (int u = 0; u < 3; u++)
                {
                    for (int o = 0; o < 9; o++)
                    {
                        var cell = _unit == Unit.Row ? new Cell(_mini * 3 + u, o) : new Cell(o, _mini * 3 + u);
                        if (snapshot[cell.Row, cell.Column] == p) lighter.HighlightCell(cell, (ChangeColoration)color);
                    }
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