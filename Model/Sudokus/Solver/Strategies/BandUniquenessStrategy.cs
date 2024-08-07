using System.Collections.Generic;
using Model.Core;
using Model.Core.Changes;
using Model.Core.Highlighting;
using Model.Sudokus.Solver.Position;
using Model.Sudokus.Solver.Utility;
using Model.Utility;
using Model.Utility.BitSets;

namespace Model.Sudokus.Solver.Strategies;

public class BandUniquenessStrategy : SudokuStrategy
{
    public const string OfficialName = "Band-Uniqueness";
    private const InstanceHandling DefaultInstanceHandling = InstanceHandling.UnorderedAll;

    public BandUniquenessStrategy() : base(OfficialName, Difficulty.Medium, DefaultInstanceHandling)
    {
        UniquenessDependency = UniquenessDependency.FullyDependent;
    }
    
    public override void Apply(ISudokuSolverData solverData)
    {
        List<CellPossibility> buffer = new();
        
        for (int mini = 0; mini < 3; mini++)
        {
            var presence = new ReadOnlyBitSet16();
            var availability = new LinePositions();

            for (int col = 0; col < 9; col++)
            {
                for (int r = 0; r < 3; r++)
                {
                    var row = mini * 3 + r;
                    var solved = solverData.Sudoku[row, col];

                    if (solved == 0) availability.Add(col);
                    else presence += solved;
                }
            }

            if (presence.Count < 9 && availability.Count == 9 - presence.Count + 2)
            {
                foreach (var col in availability)
                {
                    for (int r = 0; r < 3; r++)
                    {
                        var row = mini * 3 + r;
                        var poss = solverData.PossibilitiesAt(row, col) - presence;

                        foreach (var p in poss.EnumeratePossibilities())
                        {
                            buffer.Add(new CellPossibility(row, col, p));
                        }
                    }
                    
                    if (buffer.Count == 1) solverData.ChangeBuffer.ProposeSolutionAddition(buffer[0]);
                    else
                    {
                        foreach (var cp in SudokuCellUtility.SharedSeenExistingPossibilities(solverData, buffer))
                        {
                            solverData.ChangeBuffer.ProposePossibilityRemoval(cp);
                        }
                    }
                    buffer.Clear();
                }

                if (solverData.ChangeBuffer.NeedCommit())
                {
                    solverData.ChangeBuffer.Commit(new BandUniquenessReportBuilder(mini, Unit.Row, presence));
                    if (StopOnFirstCommit) return;
                }
            }

            presence = new ReadOnlyBitSet16();
            availability.Void();
            for (int row = 0; row < 9; row++)
            {
                for (int c = 0; c < 3; c++)
                {
                    var col = mini * 3 + c;
                    var solved = solverData.Sudoku[row, col];
                    
                    if (solved == 0) availability.Add(row);
                    else presence += solved;
                }
            }

            if (presence.Count < 9 && availability.Count == 9 - presence.Count + 2)
            {
                foreach (var row in availability)
                {
                    for (int c = 0; c < 3; c++)
                    {
                        var col = mini * 3 + c;
                        var poss = solverData.PossibilitiesAt(row, col) - presence;

                        foreach (var p in poss.EnumeratePossibilities())
                        {
                            buffer.Add(new CellPossibility(row, col, p));
                        }
                    }
                    
                    if (buffer.Count == 1) solverData.ChangeBuffer.ProposeSolutionAddition(buffer[0]);
                    else
                    {
                        foreach (var cp in SudokuCellUtility.SharedSeenExistingPossibilities(solverData, buffer))
                        {
                            solverData.ChangeBuffer.ProposePossibilityRemoval(cp);
                        }
                    }
                    buffer.Clear();
                }

                if (solverData.ChangeBuffer.NeedCommit())
                {
                    solverData.ChangeBuffer.Commit(new BandUniquenessReportBuilder(mini, Unit.Row, presence));
                    if (StopOnFirstCommit) return;
                }
            }
        }
    }
}

public class BandUniquenessReportBuilder : IChangeReportBuilder<NumericChange, ISudokuSolvingState, ISudokuHighlighter>
{
    private readonly int _mini;
    private readonly Unit _unit;
    private readonly ReadOnlyBitSet16 _presence;

    public BandUniquenessReportBuilder(int mini, Unit unit, ReadOnlyBitSet16 presence)
    {
        _mini = mini;
        _unit = unit;
        _presence = presence;
    }

    public ChangeReport<ISudokuHighlighter> BuildReport(IReadOnlyList<NumericChange> changes, ISudokuSolvingState snapshot)
    {
        return new ChangeReport<ISudokuHighlighter>($"Band-Uniqueness in {_unit.ToString().ToLower()} band " + 
                                                    $"{_mini + 1}", lighter =>
        {
            int color = (int)StepColor.Cause1;
            foreach (var p in _presence.EnumeratePossibilities())
            {
                for (int u = 0; u < 3; u++)
                {
                    for (int o = 0; o < 9; o++)
                    {
                        var cell = _unit == Unit.Row ? new Cell(_mini * 3 + u, o) : new Cell(o, _mini * 3 + u);
                        if (snapshot[cell.Row, cell.Column] == p) lighter.HighlightCell(cell, (StepColor)color);
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