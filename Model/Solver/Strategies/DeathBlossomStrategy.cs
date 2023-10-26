using System.Collections.Generic;
using System.Text;
using Model.Solver.Helpers.Changes;
using Model.Solver.Helpers.Highlighting;
using Model.Solver.Possibilities;
using Model.Solver.StrategiesUtil;
using Model.Solver.StrategiesUtil.AlmostLockedSets;

namespace Model.Solver.Strategies;

public class DeathBlossomStrategy : AbstractStrategy
{
    public const string OfficialName = "Death Blossom";
    
    public DeathBlossomStrategy() : base(OfficialName, StrategyDifficulty.Extreme)
    {
    }

    public override void ApplyOnce(IStrategyManager strategyManager)
    {
        var allAls = strategyManager.PreComputer.AlmostLockedSets();
        Dictionary<int, List<AlmostLockedSet>> concernedAls = new();

        for (int row = 0; row < 9; row++)
        {
            for (int col = 0; col < 9; col++)
            {
                var possibilities = strategyManager.PossibilitiesAt(row, col);
                if (possibilities.Count == 0) continue;

                var current = new Cell(row, col);
                foreach (var possibility in possibilities)
                {
                    concernedAls[possibility] = new List<AlmostLockedSet>();
                }

                foreach (var als in allAls)
                {
                    if (als.Contains(current)) continue;
                    
                    var and = als.Possibilities.And(possibilities);
                    if (and.Count == 0) continue;

                    foreach (var possibilityInCommon in and)
                    {
                        var ok = true;

                        foreach (var cell in als.Cells)
                        {
                            if (strategyManager.PossibilitiesAt(cell).Peek(possibilityInCommon) &&
                                !cell.ShareAUnit(current))
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

                Dictionary<Cell, IPossibilities> eliminations = new();
                Dictionary<Cell, HashSet<AlmostLockedSet>> eliminationsCauses = new();
                List<Cell> buffer = new();
                
                foreach (var possibility in possibilities)
                {
                    foreach (var als in concernedAls[possibility])
                    {
                        if (!als.Possibilities.Peek(possibility)) continue;

                        foreach (var alsPossibility in als.Possibilities)
                        {
                            if(alsPossibility == possibility) continue;

                            foreach (var cell in als.Cells)
                            {
                                if (strategyManager.PossibilitiesAt(cell).Peek(alsPossibility)) buffer.Add(cell);
                            }

                            foreach (var seenCell in Cells.SharedSeenCells(buffer))
                            {
                                if (seenCell == current || strategyManager.Sudoku[seenCell.Row, seenCell.Col] != 0) continue;

                                if (!eliminations.TryGetValue(seenCell, out var value))
                                {
                                    value = strategyManager.PossibilitiesAt(seenCell).Copy();
                                    eliminations[seenCell] = value;
                                    eliminationsCauses[seenCell] = new HashSet<AlmostLockedSet>();
                                }

                                if (value.Remove(alsPossibility)) eliminationsCauses[seenCell].Add(als);
                                
                                
                                if (value.Count == 0)
                                {
                                    strategyManager.ChangeBuffer.AddPossibilityToRemove(possibility, current.Row,
                                        current.Col);
                                    strategyManager.ChangeBuffer.Push(this,
                                        new DeathBlossomReportBuilder(current, seenCell,
                                            eliminationsCauses[seenCell]));
                                    return;
                                }
                            }
                            
                            buffer.Clear();
                        }
                    }
                    
                    eliminations.Clear();
                }
                
                concernedAls.Clear();
            }
        }
    }

    private void Process(IStrategyManager strategyManager, Cell stem, Cell target, IEnumerable<AlmostLockedSet> sets)
    {
        //TODO use;
    }
}

public class DeathBlossomReportBuilder : IChangeReportBuilder
{
    private readonly Cell _stem;
    private readonly Cell _target;
    private readonly IEnumerable<AlmostLockedSet> _als;

    public DeathBlossomReportBuilder(Cell stem, Cell target, IEnumerable<AlmostLockedSet> als)
    {
        _stem = stem;
        _target = target;
        _als = als;
    }

    public ChangeReport Build(List<SolverChange> changes, IPossibilitiesHolder snapshot)
    {
        return new ChangeReport(IChangeReportBuilder.ChangesToString(changes), Explanation(), lighter =>
        {
            lighter.HighlightCell(_stem, ChangeColoration.Neutral);
            lighter.HighlightCell(_target, ChangeColoration.CauseOnOne);

            int start = (int)ChangeColoration.CauseOffOne;
            foreach (var als in _als)
            {
                var coloration = (ChangeColoration)start;

                foreach (var cell in als.Cells)
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
        var asList = new List<AlmostLockedSet>(_als);

        for (int i = 0; i < asList.Count; i++)
        {
            builder.Append($"#{i + 1} : {asList[i]}\n");
        }

        return builder.ToString();
    }
}