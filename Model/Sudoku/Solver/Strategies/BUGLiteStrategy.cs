using System;
using System.Collections.Generic;
using Model.Helpers;
using Model.Helpers.Changes;
using Model.Helpers.Highlighting;
using Model.Helpers.Settings;
using Model.Helpers.Settings.Types;
using Model.Sudoku.Solver.Position;
using Model.Sudoku.Solver.StrategiesUtility;
using Model.Utility;
using Model.Utility.BitSets;

namespace Model.Sudoku.Solver.Strategies;

public class BUGLiteStrategy : SudokuStrategy
{
    public const string OfficialName = "BUG-Lite";
    private const InstanceHandling DefaultInstanceHandling = InstanceHandling.FirstOnly;

    private readonly IntSetting _maxStructSize;
    
    public BUGLiteStrategy(int maxStructSize) : base(OfficialName, StrategyDifficulty.Hard, DefaultInstanceHandling)
    {
        _maxStructSize = new IntSetting("Maximum cell count",
            new SliderInteractionInterface(4, 20, 1), maxStructSize);
        UniquenessDependency = UniquenessDependency.FullyDependent;

        AddSetting(_maxStructSize);
    }
    
    public override void Apply(IStrategyUser strategyUser)
    {
        var structuresDone = new HashSet<GridPositions>();
        
        for (int row = 0; row < 9; row++)
        {
            for (int col = 0; col < 9; col++)
            {
                var poss = strategyUser.PossibilitiesAt(row, col);
                if (poss.Count != 2) continue;

                var first = new Cell(row, col);
                var startR = row / 3 * 3;
                var startC = col / 3 * 3;

                for (int r = row % 3; r < 3; r++)
                {
                    var row2 = startR + r;
                    
                    for (int c = col % 3; c < 3; c++)
                    {
                        var col2 = startC + c;
                        if ((row2 == row && col2 == col) || !strategyUser.PossibilitiesAt(row2, col2).Equals(poss)) continue;

                        var second = new Cell(row2, col2);
                        var bcp = new BiCellPossibilities(first, second, poss);
                        var conditionsToMeet = new List<IBUGLiteCondition>();
                        if (row != row2)
                        {
                            foreach (var p in poss.EnumeratePossibilities())
                            {
                                conditionsToMeet.Add(new RowBUGLiteCondition(first, second, p));
                            }
                        }

                        if (col != col2)
                        {
                            foreach (var p in poss.EnumeratePossibilities())
                            {
                                conditionsToMeet.Add(new ColumnBUGLiteCondition(first, second, p));
                            }
                        }
                        
                        if (Search(strategyUser, new HashSet<BiCellPossibilities> {bcp},
                            new GridPositions {first, second}, conditionsToMeet,
                            new HashSet<IBUGLiteCondition>(), structuresDone)) return;
                    }
                }
            }
        }
    }

    private bool Search(IStrategyUser strategyUser, HashSet<BiCellPossibilities> bcp, GridPositions structure, 
        List<IBUGLiteCondition> conditionsToMeet, HashSet<IBUGLiteCondition> conditionsMet, HashSet<GridPositions> structuresDone)
    {
        var current = conditionsToMeet[0];
        conditionsToMeet.RemoveAt(0);
        conditionsMet.Add(current);

        foreach (var match in current.ConditionMatches(strategyUser, structure))
        {
            bool ok = true;
            foreach (var otherCondition in match.OtherConditions)
            {
                if (conditionsMet.Contains(otherCondition))
                {
                    ok = false;
                    break;
                }
            }

            if (!ok) continue;
            
            structure.Add(match.BiCellPossibilities.One);
            structure.Add(match.BiCellPossibilities.Two);
            if (structuresDone.Contains(structure))
            {
                structure.Remove(match.BiCellPossibilities.One);
                structure.Remove(match.BiCellPossibilities.Two);
                continue;
            }

            structuresDone.Add(structure.Copy());

            List<IBUGLiteCondition> met = new();
            foreach (var otherCondition in match.OtherConditions)
            {
                var i = conditionsToMeet.IndexOf(otherCondition);
                if (i == -1) conditionsToMeet.Add(otherCondition);
                else
                {
                    conditionsToMeet.RemoveAt(i);
                    conditionsMet.Add(otherCondition);
                    met.Add(otherCondition);
                }
            }

            bcp.Add(match.BiCellPossibilities);

            if (conditionsToMeet.Count == 0)
            {
                if (Process(strategyUser, bcp)) return true;
            }
            else if (structure.Count < _maxStructSize.Value &&
                      Search(strategyUser, bcp, structure, conditionsToMeet, conditionsMet, structuresDone)) return true;
            
            structure.Remove(match.BiCellPossibilities.One);
            structure.Remove(match.BiCellPossibilities.Two);
            bcp.Remove(match.BiCellPossibilities);
            var count = match.OtherConditions.Length - met.Count;
            conditionsToMeet.RemoveRange(conditionsToMeet.Count - count, count);
            foreach (var c in met)
            {
                conditionsMet.Remove(c);
                conditionsToMeet.Add(c);
            }
        }

        conditionsToMeet.Insert(0, current);
        conditionsMet.Remove(current);
        
        return false;
    }

    private bool Process(IStrategyUser strategyUser, HashSet<BiCellPossibilities> bcp)
    {
        var cellsNotInStructure = new List<Cell>();
        var possibilitiesNotInStructure = new ReadOnlyBitSet16();

        foreach (var b in bcp)
        {
            var no1 = strategyUser.PossibilitiesAt(b.One) - b.Possibilities;
            if (no1.Count > 0)
            {
                cellsNotInStructure.Add(b.One);
                possibilitiesNotInStructure |= no1;
            }

            var no2 = strategyUser.PossibilitiesAt(b.Two) - b.Possibilities;
            if (no2.Count > 0)
            {
                cellsNotInStructure.Add(b.Two);
                possibilitiesNotInStructure |= no2;
            }
        }

        if (cellsNotInStructure.Count == 1)
        {
            var c = cellsNotInStructure[0];
            foreach (var p in FindStructurePossibilitiesFor(c, bcp).EnumeratePossibilities())
            {
                strategyUser.ChangeBuffer.ProposePossibilityRemoval(p, c);
            }
        }
        else if (cellsNotInStructure.Count == 2)
        {
            var p1 = FindStructurePossibilitiesFor(cellsNotInStructure[0], bcp);
            var p2 = FindStructurePossibilitiesFor(cellsNotInStructure[1], bcp);
            if(p1.Equals(p2))
            {
                var asArray = p1.ToArray();
                for (int i = 0; i < 2; i++)
                {
                    var cp1 = new CellPossibility(cellsNotInStructure[0], asArray[i]);
                    var cp2 = new CellPossibility(cellsNotInStructure[1], asArray[i]);
                    if (Cells.AreStronglyLinked(strategyUser, cp1, cp2))
                    {
                        var other = asArray[(i + 1) % 2];
                        strategyUser.ChangeBuffer.ProposePossibilityRemoval(other, cellsNotInStructure[0]);
                        strategyUser.ChangeBuffer.ProposePossibilityRemoval(other, cellsNotInStructure[1]);
                    }
                }
            }
        }

        if (possibilitiesNotInStructure.Count == 1)
        {
            var p = possibilitiesNotInStructure.FirstPossibility();
            foreach (var ssc in Cells.SharedSeenCells(cellsNotInStructure))
            {
                strategyUser.ChangeBuffer.ProposePossibilityRemoval(p, ssc);
            }
        }

        return strategyUser.ChangeBuffer.NotEmpty() && strategyUser.ChangeBuffer.Commit(
            new BUGLiteReportBuilder(bcp)) && StopOnFirstPush;
    }

    private static ReadOnlyBitSet16 FindStructurePossibilitiesFor(Cell cell, IEnumerable<BiCellPossibilities> bcp)
    {
        foreach (var b in bcp)
        {
            if (b.One == cell || b.Two == cell) return b.Possibilities;
        }

        return new ReadOnlyBitSet16();
    }
}

public record BiCellPossibilities(Cell One, Cell Two, ReadOnlyBitSet16 Possibilities);

public record BUGLiteConditionMatch(BiCellPossibilities BiCellPossibilities, params IBUGLiteCondition[] OtherConditions);

public interface IBUGLiteCondition
{ 
    IEnumerable<BUGLiteConditionMatch> ConditionMatches(IStrategyUser strategyUser, GridPositions done);
}

public class RowBUGLiteCondition : IBUGLiteCondition
{
    private readonly Cell _one;
    private readonly Cell _two;
    private readonly int _possibility;

    public RowBUGLiteCondition(Cell one, Cell two, int possibility)
    {
        _one = one;
        _two = two;
        _possibility = possibility;
    }

    public IEnumerable<BUGLiteConditionMatch> ConditionMatches(IStrategyUser strategyUser, GridPositions done)
    {
        var miniCol = _one.Column / 3;

        for (int c = 0; c < 3; c++)
        {
            if (c == miniCol) continue;

            for (int i = 0; i < 3; i++)
            {
                var first = new Cell(_one.Row, c * 3 + i);
                if (done.Contains(first) || strategyUser.Sudoku[first.Row, first.Column] != 0) continue;

                for (int j = 0; j < 3; j++)
                {
                    var second = new Cell(_two.Row, c * 3 + j);
                    if (done.Contains(first) || strategyUser.Sudoku[second.Row, second.Column] != 0) continue;

                    var and = strategyUser.PossibilitiesAt(first) & strategyUser.PossibilitiesAt(second);
                    if (and.Count < 2 || !and.Contains(_possibility)) continue;

                    foreach (var p in and.EnumeratePossibilities())
                    {
                        if (p == _possibility) continue;

                        var poss = new ReadOnlyBitSet16(_possibility, p);
                        var bcp = new BiCellPossibilities(first, second, poss);

                        if (first.Column == second.Column) yield return new BUGLiteConditionMatch(
                            bcp, new RowBUGLiteCondition(first, second, p));
                        else yield return new BUGLiteConditionMatch(bcp, new RowBUGLiteCondition(
                                first, second, p), new ColumnBUGLiteCondition(first, second, p),
                            new ColumnBUGLiteCondition(first, second, _possibility));
                    }
                }
            }
        }
    }

    public override bool Equals(object? obj)
    {
        return obj is RowBUGLiteCondition rblc && rblc._possibility == _possibility &&
               rblc._one.Row == _one.Row && rblc._two.Row == _two.Row;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(_possibility, _one.Row, _two.Row);
    }

    public override string ToString()
    {
        return $"{_possibility}r{_one.Row}r{_two.Row}";
    }
}

public class ColumnBUGLiteCondition : IBUGLiteCondition
{
    private readonly Cell _one;
    private readonly Cell _two;
    private readonly int _possibility;

    public ColumnBUGLiteCondition(Cell one, Cell two, int possibility)
    {
        _one = one;
        _two = two;
        _possibility = possibility;
    }

    public IEnumerable<BUGLiteConditionMatch> ConditionMatches(IStrategyUser strategyUser, GridPositions done)
    {
        var miniRow = _one.Row / 3;

        for (int r = 0; r < 3; r++)
        {
            if (r == miniRow) continue;

            for (int i = 0; i < 3; i++)
            {
                var first = new Cell(r * 3 + i, _one.Column);
                if (done.Contains(first) || strategyUser.Sudoku[first.Row, first.Column] != 0) continue;

                for (int j = 0; j < 3; j++)
                {
                    var second = new Cell(r * 3 + j, _two.Column);
                    if (done.Contains(first) || strategyUser.Sudoku[second.Row, second.Column] != 0) continue;

                    var and = strategyUser.PossibilitiesAt(first) & strategyUser.PossibilitiesAt(second);
                    if (and.Count < 2 || !and.Contains(_possibility)) continue;

                    foreach (var p in and.EnumeratePossibilities())
                    {
                        if (p == _possibility) continue;

                        var poss = new ReadOnlyBitSet16(_possibility, p);
                        var bcp = new BiCellPossibilities(first, second, poss);

                        if (first.Row == second.Row)
                            yield return new BUGLiteConditionMatch(
                                bcp, new ColumnBUGLiteCondition(first, second, p));
                        else
                            yield return new BUGLiteConditionMatch(bcp, new ColumnBUGLiteCondition(
                                    first, second, p), new RowBUGLiteCondition(first, second, p),
                                new RowBUGLiteCondition(first, second, _possibility));
                    }
                }
            }
        }
    }
    
    public override bool Equals(object? obj)
    {
        return obj is ColumnBUGLiteCondition cblc && cblc._possibility == _possibility &&
               cblc._one.Column == _one.Column && cblc._two.Column == _two.Column;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(_possibility, _one.Column, _two.Column);
    }

    public override string ToString()
    {
        return $"{_possibility}c{_one.Column}c{_two.Column}";
    }
}

public class BUGLiteReportBuilder : IChangeReportBuilder<IUpdatableSudokuSolvingState, ISudokuHighlighter>
{
    private readonly IEnumerable<BiCellPossibilities> _bcp;

    public BUGLiteReportBuilder(IEnumerable<BiCellPossibilities> bcp)
    {
        _bcp = bcp;
    }

    public ChangeReport<ISudokuHighlighter> BuildReport(IReadOnlyList<SolverProgress> changes, IUpdatableSudokuSolvingState snapshot)
    {
        return new ChangeReport<ISudokuHighlighter>( "", lighter =>
        {
            foreach (var b in _bcp)
            {
                foreach (var p in b.Possibilities.EnumeratePossibilities())
                {
                    lighter.HighlightPossibility(p, b.One.Row, b.One.Column, ChangeColoration.CauseOffTwo);
                    lighter.HighlightPossibility(p, b.Two.Row, b.Two.Column, ChangeColoration.CauseOffTwo);
                }
            }

            ChangeReportHelper.HighlightChanges(lighter, changes);
        });
    }
    
    public Clue<ISudokuHighlighter> BuildClue(IReadOnlyList<SolverProgress> changes, IUpdatableSudokuSolvingState snapshot)
    {
        return Clue<ISudokuHighlighter>.Default();
    }
}