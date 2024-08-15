using System.Collections.Generic;
using System.Linq;
using Model.Core;
using Model.Core.Changes;
using Model.Core.Highlighting;
using Model.Sudokus.Solver.PossibilitySets;
using Model.Sudokus.Solver.Utility;
using Model.Utility;
using Model.Utility.BitSets;
using Model.Utility.Collections;

namespace Model.Sudokus.Solver.Strategies;

public class DeathBlossomStrategy : SudokuStrategy
{
    public const string OfficialName = "Death Blossom";
    private const InstanceHandling DefaultInstanceHandling = InstanceHandling.FirstOnly;

    public DeathBlossomStrategy() : base(OfficialName, Difficulty.Extreme, DefaultInstanceHandling)
    {
    }

    public override void Apply(ISudokuSolverData solverData)
    {
        var allAls = solverData.PreComputer.AlmostLockedSets();
        Dictionary<int, List<IPossibilitySet>> concernedAls = new();

        for (int row = 0; row < 9; row++)
        {
            for (int col = 0; col < 9; col++)
            {
                var possibilities = solverData.PossibilitiesAt(row, col);
                if (possibilities.Count == 0) continue;

                var current = new Cell(row, col);
                foreach (var possibility in possibilities.EnumeratePossibilities())
                {
                    concernedAls[possibility] = new List<IPossibilitySet>();
                }

                foreach (var als in allAls)
                {
                    if (als.Positions.Contains(current)) continue;
                    
                    var and = als.Possibilities & possibilities;
                    if (and.Count == 0) continue;

                    foreach (var possibilityInCommon in and.EnumeratePossibilities())
                    {
                        var ok = true;

                        foreach (var cell in als.EnumerateCells())
                        {
                            if (solverData.PossibilitiesAt(cell).Contains(possibilityInCommon) &&
                                !SudokuUtility.ShareAUnit(cell, current))
                            {
                                ok = false;
                                break;
                            }
                        }

                        if (ok)
                        {
                            concernedAls[possibilityInCommon].Add(als);
                        }
                    }
                }

                Dictionary<Cell, ReadOnlyBitSet16> eliminations = new();
                Dictionary<Cell, HashSet<IPossibilitySet>> eliminationsCauses = new();
                List<Cell> buffer = new();
                
                foreach (var possibility in possibilities.EnumeratePossibilities())
                {
                    foreach (var als in concernedAls[possibility])
                    {
                        if (!als.Possibilities.Contains(possibility)) continue;

                        foreach (var alsPossibility in als.Possibilities.EnumeratePossibilities())
                        {
                            if(alsPossibility == possibility) continue;

                            foreach (var cell in als.EnumerateCells())
                            {
                                if (solverData.PossibilitiesAt(cell).Contains(alsPossibility)) buffer.Add(cell);
                            }

                            foreach (var seenCell in SudokuUtility.SharedSeenCells(buffer))
                            {
                                if (seenCell == current || solverData.Sudoku[seenCell.Row, seenCell.Column] != 0) continue;

                                if (!eliminations.TryGetValue(seenCell, out var value))
                                {
                                    value = solverData.PossibilitiesAt(seenCell);
                                    eliminations[seenCell] = value;
                                    eliminationsCauses[seenCell] = new HashSet<IPossibilitySet>();
                                }

                                if (value.Contains(alsPossibility))
                                {
                                    value -= alsPossibility;
                                    eliminations[seenCell] = value;
                                    eliminationsCauses[seenCell].Add(als);
                                }
                                if (value.Count != 0) continue;

                                if (Process(solverData, current, seenCell, eliminationsCauses[seenCell],
                                        possibility)) return;
                            }
                            
                            buffer.Clear();
                        }
                    }
                    
                    eliminations.Clear();
                    eliminationsCauses.Clear();
                }
                
                concernedAls.Clear();
            }
        }
    }

    private bool Process(ISudokuSolverData solverData, Cell stem, Cell target, HashSet<IPossibilitySet> sets, int possibility)
    {
        List<Cell> buffer = new();
        solverData.ChangeBuffer.ProposePossibilityRemoval(possibility, stem.Row, stem.Column);

        foreach (var als in sets)
        {
            foreach (var cell in als.EnumerateCells())
            {
                if (solverData.PossibilitiesAt(cell).Contains(possibility)) buffer.Add(cell);
            }
        }

        var allStems = SudokuUtility.SharedSeenCells(buffer).ToArray();
        if (allStems.Length > 1)
        {
            foreach (var cell in allStems)
            {
                if (cell == stem) continue;

                solverData.ChangeBuffer.ProposePossibilityRemoval(possibility, cell.Row, cell.Column);
            }
        }
        
        if (solverData.ChangeBuffer.NeedCommit())
        {
            solverData.ChangeBuffer.Commit(new DeathBlossomReportBuilder(allStems, target, sets));
            if (StopOnFirstCommit) return true;
        }

        return false;
    }
}

public class DeathBlossomReportBuilder : IChangeReportBuilder<NumericChange, ISudokuSolvingState, ISudokuHighlighter>
{
    private readonly IReadOnlyList<Cell> _stems;
    private readonly Cell _target;
    private readonly IEnumerable<IPossibilitySet> _als;

    public DeathBlossomReportBuilder(IReadOnlyList<Cell> stems, Cell target, IEnumerable<IPossibilitySet> als)
    {
        _stems = stems;
        _target = target;
        _als = als;
    }

    public ChangeReport<ISudokuHighlighter> BuildReport(IReadOnlyList<NumericChange> changes, ISudokuSolvingState snapshot)
    {
        return new ChangeReport<ISudokuHighlighter>($"Death Blossom from {_stems.ToStringSequence(" ,")}," +
                                                    $"targeting {_target.ToString()} with {_als.ToStringSequence(", ")}",
            lighter =>
        {
            foreach (var stem in _stems)
            {
                if (snapshot[stem.Row, stem.Column] != 0) continue;
                lighter.HighlightCell(stem, StepColor.Neutral);
            }
            
            lighter.HighlightCell(_target, StepColor.On);

            int start = (int)StepColor.Cause1;
            foreach (var als in _als)
            {
                var coloration = (StepColor)start;

                foreach (var cell in als.EnumerateCells())
                {
                    lighter.HighlightCell(cell, coloration);
                }

                start++;
            }
            
            ChangeReportHelper.HighlightChanges(lighter, changes);
        });
    }
    
    public Clue<ISudokuHighlighter> BuildClue(IReadOnlyList<NumericChange> changes, ISudokuSolvingState snapshot)
    {
        return Clue<ISudokuHighlighter>.Default();
    }
}