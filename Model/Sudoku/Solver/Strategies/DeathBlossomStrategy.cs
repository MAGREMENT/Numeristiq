using System.Collections.Generic;
using System.Text;
using Model.Sudoku.Solver.Helpers.Changes;
using Model.Sudoku.Solver.Possibility;
using Model.Sudoku.Solver.PossibilityPosition;
using Model.Sudoku.Solver.StrategiesUtility;
using Model.Utility;

namespace Model.Sudoku.Solver.Strategies;

public class DeathBlossomStrategy : AbstractStrategy
{
    public const string OfficialName = "Death Blossom";
    private const OnCommitBehavior DefaultBehavior = OnCommitBehavior.Return;
    
    public override OnCommitBehavior DefaultOnCommitBehavior => DefaultBehavior;
    
    public DeathBlossomStrategy() : base(OfficialName, StrategyDifficulty.Extreme, DefaultBehavior)
    {
    }

    public override void Apply(IStrategyManager strategyManager)
    {
        var allAls = strategyManager.PreComputer.AlmostLockedSets();
        Dictionary<int, List<IPossibilitiesPositions>> concernedAls = new();

        for (int row = 0; row < 9; row++)
        {
            for (int col = 0; col < 9; col++)
            {
                var possibilities = strategyManager.PossibilitiesAt(row, col);
                if (possibilities.Count == 0) continue;

                var current = new Cell(row, col);
                foreach (var possibility in possibilities)
                {
                    concernedAls[possibility] = new List<IPossibilitiesPositions>();
                }

                foreach (var als in allAls)
                {
                    if (als.Positions.Peek(current)) continue;
                    
                    var and = als.Possibilities.And(possibilities);
                    if (and.Count == 0) continue;

                    foreach (var possibilityInCommon in and)
                    {
                        var ok = true;

                        foreach (var cell in als.EachCell())
                        {
                            if (strategyManager.PossibilitiesAt(cell).Peek(possibilityInCommon) &&
                                !Cells.ShareAUnit(cell, current))
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

                Dictionary<Cell, Possibilities> eliminations = new();
                Dictionary<Cell, HashSet<IPossibilitiesPositions>> eliminationsCauses = new();
                List<Cell> buffer = new();
                
                foreach (var possibility in possibilities)
                {
                    foreach (var als in concernedAls[possibility])
                    {
                        if (!als.Possibilities.Peek(possibility)) continue;

                        foreach (var alsPossibility in als.Possibilities)
                        {
                            if(alsPossibility == possibility) continue;

                            foreach (var cell in als.EachCell())
                            {
                                if (strategyManager.PossibilitiesAt(cell).Peek(alsPossibility)) buffer.Add(cell);
                            }

                            foreach (var seenCell in Cells.SharedSeenCells(buffer))
                            {
                                if (seenCell == current || strategyManager.Sudoku[seenCell.Row, seenCell.Column] != 0) continue;

                                if (!eliminations.TryGetValue(seenCell, out var value))
                                {
                                    value = strategyManager.PossibilitiesAt(seenCell).Copy();
                                    eliminations[seenCell] = value;
                                    eliminationsCauses[seenCell] = new HashSet<IPossibilitiesPositions>();
                                }

                                if (value.Remove(alsPossibility)) eliminationsCauses[seenCell].Add(als);
                                if (value.Count != 0) continue;
                                
                                Process(strategyManager, current, seenCell, eliminationsCauses[seenCell],
                                    possibility);
                                if (OnCommitBehavior == OnCommitBehavior.Return) return;
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

    private void Process(IStrategyManager strategyManager, Cell stem, Cell target, HashSet<IPossibilitiesPositions> sets, int possibility)
    {
        List<Cell> buffer = new();
        strategyManager.ChangeBuffer.ProposePossibilityRemoval(possibility, stem.Row, stem.Column);

        foreach (var als in sets)
        {
            foreach (var cell in als.EachCell())
            {
                if (strategyManager.PossibilitiesAt(cell).Peek(possibility)) buffer.Add(cell);
            }
        }

        var allStems = Cells.SharedSeenCells(buffer);
        if (allStems.Count > 1)
        {
            foreach (var cell in allStems)
            {
                if (cell == stem) continue;

                strategyManager.ChangeBuffer.ProposePossibilityRemoval(possibility, cell.Row, cell.Column);
            }
        }
        
        strategyManager.ChangeBuffer.Commit(this, new DeathBlossomReportBuilder(allStems, target, sets));
    }
}

public class DeathBlossomReportBuilder : IChangeReportBuilder
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

    public ChangeReport Build(IReadOnlyList<SolverChange> changes, IPossibilitiesHolder snapshot)
    {
        return new ChangeReport(IChangeReportBuilder.ChangesToString(changes), Explanation(), lighter =>
        {
            foreach (var stem in _stems)
            {
                if (snapshot.Sudoku[stem.Row, stem.Column] != 0) continue;
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
            
            IChangeReportBuilder.HighlightChanges(lighter, changes);
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
}