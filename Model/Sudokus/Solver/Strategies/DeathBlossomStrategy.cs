using System.Collections.Generic;
using System.Text;
using Model.Core;
using Model.Core.Changes;
using Model.Core.Highlighting;
using Model.Sudokus.Solver.PossibilityPosition;
using Model.Sudokus.Solver.Utility;
using Model.Utility;
using Model.Utility.BitSets;

namespace Model.Sudokus.Solver.Strategies;

public class DeathBlossomStrategy : SudokuStrategy
{
    public const string OfficialName = "Death Blossom";
    private const InstanceHandling DefaultInstanceHandling = InstanceHandling.FirstOnly;

    public DeathBlossomStrategy() : base(OfficialName, StepDifficulty.Extreme, DefaultInstanceHandling)
    {
    }

    public override void Apply(ISudokuStrategyUser strategyUser)
    {
        var allAls = strategyUser.PreComputer.AlmostLockedSets();
        Dictionary<int, List<IPossibilitiesPositions>> concernedAls = new();

        for (int row = 0; row < 9; row++)
        {
            for (int col = 0; col < 9; col++)
            {
                var possibilities = strategyUser.PossibilitiesAt(row, col);
                if (possibilities.Count == 0) continue;

                var current = new Cell(row, col);
                foreach (var possibility in possibilities.EnumeratePossibilities())
                {
                    concernedAls[possibility] = new List<IPossibilitiesPositions>();
                }

                foreach (var als in allAls)
                {
                    if (als.Positions.Contains(current)) continue;
                    
                    var and = als.Possibilities & possibilities;
                    if (and.Count == 0) continue;

                    foreach (var possibilityInCommon in and.EnumeratePossibilities())
                    {
                        var ok = true;

                        foreach (var cell in als.EachCell())
                        {
                            if (strategyUser.PossibilitiesAt(cell).Contains(possibilityInCommon) &&
                                !SudokuCellUtility.ShareAUnit(cell, current))
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
                Dictionary<Cell, HashSet<IPossibilitiesPositions>> eliminationsCauses = new();
                List<Cell> buffer = new();
                
                foreach (var possibility in possibilities.EnumeratePossibilities())
                {
                    foreach (var als in concernedAls[possibility])
                    {
                        if (!als.Possibilities.Contains(possibility)) continue;

                        foreach (var alsPossibility in als.Possibilities.EnumeratePossibilities())
                        {
                            if(alsPossibility == possibility) continue;

                            foreach (var cell in als.EachCell())
                            {
                                if (strategyUser.PossibilitiesAt(cell).Contains(alsPossibility)) buffer.Add(cell);
                            }

                            foreach (var seenCell in SudokuCellUtility.SharedSeenCells(buffer))
                            {
                                if (seenCell == current || strategyUser.Sudoku[seenCell.Row, seenCell.Column] != 0) continue;

                                if (!eliminations.TryGetValue(seenCell, out var value))
                                {
                                    value = strategyUser.PossibilitiesAt(seenCell);
                                    eliminations[seenCell] = value;
                                    eliminationsCauses[seenCell] = new HashSet<IPossibilitiesPositions>();
                                }

                                if (value.Contains(alsPossibility))
                                {
                                    value -= alsPossibility;
                                    eliminations[seenCell] = value;
                                    eliminationsCauses[seenCell].Add(als);
                                }
                                if (value.Count != 0) continue;
                                
                                Process(strategyUser, current, seenCell, eliminationsCauses[seenCell],
                                    possibility);
                                if (StopOnFirstPush) return;
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

    private void Process(ISudokuStrategyUser strategyUser, Cell stem, Cell target, HashSet<IPossibilitiesPositions> sets, int possibility)
    {
        List<Cell> buffer = new();
        strategyUser.ChangeBuffer.ProposePossibilityRemoval(possibility, stem.Row, stem.Column);

        foreach (var als in sets)
        {
            foreach (var cell in als.EachCell())
            {
                if (strategyUser.PossibilitiesAt(cell).Contains(possibility)) buffer.Add(cell);
            }
        }

        var allStems = SudokuCellUtility.SharedSeenCells(buffer);
        if (allStems.Count > 1)
        {
            foreach (var cell in allStems)
            {
                if (cell == stem) continue;

                strategyUser.ChangeBuffer.ProposePossibilityRemoval(possibility, cell.Row, cell.Column);
            }
        }
        
        strategyUser.ChangeBuffer.Commit( new DeathBlossomReportBuilder(allStems, target, sets));
    }
}

public class DeathBlossomReportBuilder : IChangeReportBuilder<IUpdatableSudokuSolvingState, ISudokuHighlighter>
{
    private readonly List<Cell> _stems;
    private readonly Cell _target;
    private readonly IEnumerable<IPossibilitiesPositions> _als;

    public DeathBlossomReportBuilder(List<Cell> stems, Cell target, IEnumerable<IPossibilitiesPositions> als)
    {
        _stems = stems;
        _target = target;
        _als = als;
    }

    public ChangeReport<ISudokuHighlighter> BuildReport(IReadOnlyList<SolverProgress> changes, IUpdatableSudokuSolvingState snapshot)
    {
        return new ChangeReport<ISudokuHighlighter>( Explanation(), lighter =>
        {
            foreach (var stem in _stems)
            {
                if (snapshot[stem.Row, stem.Column] != 0) continue;
                lighter.HighlightCell(stem, ChangeColoration.Neutral);
            }
            
            lighter.HighlightCell(_target, ChangeColoration.CauseOnOne);

            int start = (int)ChangeColoration.CauseOffOne;
            foreach (var als in _als)
            {
                var coloration = (ChangeColoration)start;

                foreach (var cell in als.EachCell())
                {
                    lighter.HighlightCell(cell, coloration);
                }

                start++;
            }
            
            ChangeReportHelper.HighlightChanges(lighter, changes);
        });
    }

    private string Explanation()
    {
        var builder = new StringBuilder();
        var asList = new List<IPossibilitiesPositions>(_als);

        for (int i = 0; i < asList.Count; i++)
        {
            builder.Append($"#{i + 1} : {asList[i]}\n");
        }

        return builder.ToString();
    }
    
    public Clue<ISudokuHighlighter> BuildClue(IReadOnlyList<SolverProgress> changes, IUpdatableSudokuSolvingState snapshot)
    {
        return Clue<ISudokuHighlighter>.Default();
    }
}