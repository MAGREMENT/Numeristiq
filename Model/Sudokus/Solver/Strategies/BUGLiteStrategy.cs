using System.Collections.Generic;
using Model.Core;
using Model.Core.Changes;
using Model.Core.Highlighting;
using Model.Core.Settings;
using Model.Core.Settings.Types;
using Model.Sudokus.Solver.Position;
using Model.Sudokus.Solver.Utility;
using Model.Utility;
using Model.Utility.BitSets;
using Model.Utility.Collections;

namespace Model.Sudokus.Solver.Strategies;

public class BUGLiteStrategy : SudokuStrategy
{
    public const string OfficialName = "BUG-Lite";
    private const InstanceHandling DefaultInstanceHandling = InstanceHandling.FirstOnly;

    private readonly IntSetting _maxStructSize;
    
    public BUGLiteStrategy(int maxStructSize) : base(OfficialName, Difficulty.Hard, DefaultInstanceHandling)
    {
        _maxStructSize = new IntSetting("Maximum cell count", "The maximum amount of cell of a pattern",
            new SliderInteractionInterface(4, 20, 1), maxStructSize);
        UniquenessDependency = UniquenessDependency.FullyDependent;
    }
    
    public override IEnumerable<ISetting> EnumerateSettings()
    {
        yield return _maxStructSize;
    }
    
    public override void Apply(ISudokuSolverData solverData)
    {
        var structuresDone = new HashSet<GridPositions>();
        
        for (int row = 0; row < 9; row++)
        {
            for (int col = 0; col < 9; col++)
            {
                var poss = solverData.PossibilitiesAt(row, col);
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
                        if ((row2 == row && col2 == col) 
                            || !solverData.PossibilitiesAt(row2, col2).Equals(poss)) continue;

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
                        
                        if (Search(solverData, new List<BiCellPossibilities> {bcp},
                            new GridPositions {first, second}, conditionsToMeet,
                            new ReadOnlyBUGLiteConditionSet(), structuresDone)) return;
                    }
                }
            }
        }
    }

    private bool Search(ISudokuSolverData solverData, List<BiCellPossibilities> bcp, GridPositions structure, 
        List<IBUGLiteCondition> conditionsToMeet, ReadOnlyBUGLiteConditionSet conditionsMet, HashSet<GridPositions> structuresDone)
    {
        var current = conditionsToMeet[0];
        conditionsToMeet.RemoveAt(0);

        foreach (var match in current.SearchMatches(solverData, structure))
        {
            var newMet = conditionsMet + current;
            if(newMet.Contains(match.OtherConditions)) continue;
            
            structure.Add(match.BiCellPossibilities.One);
            structure.Add(match.BiCellPossibilities.Two);
            if (structuresDone.Contains(structure))
            {
                structure.Remove(match.BiCellPossibilities.One);
                structure.Remove(match.BiCellPossibilities.Two);
                continue;
            }

            structuresDone.Add(structure.Copy());

            var toMeet = new List<IBUGLiteCondition>(conditionsToMeet);
            bool valid = true;
            foreach (var otherCondition in match.OtherConditions)
            {
                bool stop = false;
                for (int i = 0; i < toMeet.Count; i++)
                {
                    var result = toMeet[i].DoesMatch(otherCondition);
                    switch (result)
                    {
                        case MatchingResult.DoesNot : break;
                        case MatchingResult.Does :
                            toMeet.RemoveAt(i);
                            newMet += otherCondition;
                            stop = true;
                            break;
                        case MatchingResult.Incompatible :
                            stop = true;
                            valid = false;
                            break;
                    }
                    
                    if(stop) break;
                }

                if (!stop) toMeet.Add(otherCondition);
            }

            if (valid && toMeet.Count * 2 + structure.Count <= _maxStructSize.Value)
            {
                bcp.Add(match.BiCellPossibilities);

                if (toMeet.Count == 0)
                {
                    if (Process(solverData, bcp)) return true;
                }
                else if (Search(solverData, bcp, structure, toMeet, newMet, structuresDone)) return true;
                
                bcp.RemoveAt(bcp.Count - 1);
            }

            structure.Remove(match.BiCellPossibilities.One);
            structure.Remove(match.BiCellPossibilities.Two);
        }

        conditionsToMeet.Insert(0, current);
        return false;
    }

    private bool Process(ISudokuSolverData solverData, IReadOnlyList<BiCellPossibilities> bcp)
    {
        var cellsNotInStructure = new List<CellPossibilities>();
        var possibilitiesNotInStructure = new ReadOnlyBitSet16();

        foreach (var b in bcp)
        {
            var no1 = solverData.PossibilitiesAt(b.One) - b.Possibilities;
            if (no1.Count > 0)
            {
                cellsNotInStructure.Add(new CellPossibilities(b.One, b.Possibilities));
                possibilitiesNotInStructure |= no1;
            }

            var no2 = solverData.PossibilitiesAt(b.Two) - b.Possibilities;
            if (no2.Count > 0)
            {
                cellsNotInStructure.Add(new CellPossibilities(b.Two, b.Possibilities));
                possibilitiesNotInStructure |= no2;
            }
        }

        if (cellsNotInStructure.Count == 1)
        {
            var c = cellsNotInStructure[0];
            foreach (var p in c.Possibilities.EnumeratePossibilities())
            {
                solverData.ChangeBuffer.ProposePossibilityRemoval(p, c.Cell);
            }
        }
        else if (cellsNotInStructure.Count == 2)
        {
            var c1 = cellsNotInStructure[0];
            var c2 = cellsNotInStructure[1];
            if(c1.Possibilities == c2.Possibilities)
            {
                var asArray = c1.Possibilities.ToArray();
                for (int i = 0; i < 2; i++)
                {
                    var cp1 = new CellPossibility(c1.Cell, asArray[i]);
                    var cp2 = new CellPossibility(c2.Cell, asArray[i]);
                    if (SudokuCellUtility.AreStronglyLinked(solverData, cp1, cp2))
                    {
                        var other = asArray[(i + 1) % 2];
                        solverData.ChangeBuffer.ProposePossibilityRemoval(other, c1.Cell);
                        solverData.ChangeBuffer.ProposePossibilityRemoval(other, c2.Cell);
                    }
                }
            }
        }

        if (possibilitiesNotInStructure.Count == 1)
        {
            var p = possibilitiesNotInStructure.FirstPossibility();
            foreach (var ssc in SudokuCellUtility.SharedSeenCells(cellsNotInStructure))
            {
                solverData.ChangeBuffer.ProposePossibilityRemoval(p, ssc);
            }
        }

        return solverData.ChangeBuffer.NeedCommit() && solverData.ChangeBuffer.Commit(
            new BUGLiteReportBuilder(bcp)) && StopOnFirstCommit;
    }
}

public record BiCellPossibilities(Cell One, Cell Two, ReadOnlyBitSet16 Possibilities)
{
    public override string ToString()
    {
        return Possibilities.EnumeratePossibilities().ToStringSequence("") + "{" + One + ", " + Two + "}";
    }
}

public record BUGLiteConditionMatch(BiCellPossibilities BiCellPossibilities, params IBUGLiteCondition[] OtherConditions);

public readonly struct ReadOnlyBUGLiteConditionSet
{
    private readonly uint _bits;

    private ReadOnlyBUGLiteConditionSet(uint bits)
    {
        _bits = bits;
    }

    public bool Contains(IBUGLiteCondition condition)
    {
        return ((_bits >> condition.ToBitIndex()) & 1) > 0;
    }

    public bool Contains(IEnumerable<IBUGLiteCondition> conditions)
    {
        foreach (var condition in conditions)
        {
            if (Contains(condition)) return true;
        }

        return false;
    }

    public static ReadOnlyBUGLiteConditionSet operator +(ReadOnlyBUGLiteConditionSet set, IBUGLiteCondition condition)
        => new(set._bits | ((uint)1 << condition.ToBitIndex()));

    public static ReadOnlyBUGLiteConditionSet operator -(ReadOnlyBUGLiteConditionSet set, IBUGLiteCondition condition)
        => new(set._bits & ~((uint)1 << condition.ToBitIndex()));
}

public interface IBUGLiteCondition
{ 
    IEnumerable<BUGLiteConditionMatch> SearchMatches(ISudokuSolverData solverData, GridPositions done);
    MatchingResult DoesMatch(IBUGLiteCondition condition);
    int ToBitIndex();
}

public enum MatchingResult
{
    Incompatible = -1, DoesNot = 0, Does = 1
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

    public IEnumerable<BUGLiteConditionMatch> SearchMatches(ISudokuSolverData solverData, GridPositions done)
    {
        var miniCol = _one.Column / 3;

        for (int c = 0; c < 3; c++)
        {
            if (c == miniCol) continue;

            for (int i = 0; i < 3; i++)
            {
                var first = new Cell(_one.Row, c * 3 + i);
                if (done.Contains(first) || solverData.Sudoku[first.Row, first.Column] != 0) continue;

                for (int j = 0; j < 3; j++)
                {
                    var second = new Cell(_two.Row, c * 3 + j);
                    if (done.Contains(first) || solverData.Sudoku[second.Row, second.Column] != 0) continue;

                    var and = solverData.PossibilitiesAt(first) & solverData.PossibilitiesAt(second);
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

    public MatchingResult DoesMatch(IBUGLiteCondition condition)
    {
        if (condition is not RowBUGLiteCondition r) return MatchingResult.DoesNot;
        if (r._possibility == _possibility && r._one.Row / 3 == _one.Row / 3)
        {
            return r._one.Row == _one.Row && r._two.Row == _two.Row && r._one.Column / 3 != _one.Column / 3
                ? MatchingResult.Does
                : MatchingResult.Incompatible;
        }

        return MatchingResult.DoesNot;
    }

    public int ToBitIndex()
    {
        return _one.Row / 3 * 9 + _possibility;
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

    public IEnumerable<BUGLiteConditionMatch> SearchMatches(ISudokuSolverData solverData, GridPositions done)
    {
        var miniRow = _one.Row / 3;

        for (int r = 0; r < 3; r++)
        {
            if (r == miniRow) continue;

            for (int i = 0; i < 3; i++)
            {
                var first = new Cell(r * 3 + i, _one.Column);
                if (done.Contains(first) || solverData.Sudoku[first.Row, first.Column] != 0) continue;

                for (int j = 0; j < 3; j++)
                {
                    var second = new Cell(r * 3 + j, _two.Column);
                    if (done.Contains(first) || solverData.Sudoku[second.Row, second.Column] != 0) continue;

                    var and = solverData.PossibilitiesAt(first) & solverData.PossibilitiesAt(second);
                    if (and.Count < 2 || !and.Contains(_possibility)) continue;

                    foreach (var p in and.EnumeratePossibilities())
                    {
                        if (p == _possibility) continue;

                        var poss = new ReadOnlyBitSet16(_possibility, p);
                        var bcp = new BiCellPossibilities(first, second, poss);

                        if (first.Row == second.Row) yield return new BUGLiteConditionMatch(
                                bcp, new ColumnBUGLiteCondition(first, second, p));
                        else yield return new BUGLiteConditionMatch(bcp, new ColumnBUGLiteCondition(
                                    first, second, p), new RowBUGLiteCondition(first, second, p),
                                new RowBUGLiteCondition(first, second, _possibility));
                    }
                }
            }
        }
    }

    public MatchingResult DoesMatch(IBUGLiteCondition condition)
    {
        if (condition is not ColumnBUGLiteCondition c) return MatchingResult.DoesNot;
        if (c._possibility == _possibility && c._one.Column / 3 == _one.Column / 3)
        {
            return c._one.Column == _one.Column && c._two.Column == _two.Column && c._one.Row / 3 != _one.Row / 3
                ? MatchingResult.Does
                : MatchingResult.Incompatible;
        }

        return MatchingResult.DoesNot;
    }

    public int ToBitIndex()
    {
        return 27 + _one.Column / 3 * 9 + _possibility;
    }

    public override string ToString()
    {
        return $"{_possibility}c{_one.Column}c{_two.Column}";
    }
}

public class BUGLiteReportBuilder : IChangeReportBuilder<NumericChange, ISudokuSolvingState, ISudokuHighlighter>
{
    private readonly IEnumerable<BiCellPossibilities> _bcp;

    public BUGLiteReportBuilder(IEnumerable<BiCellPossibilities> bcp)
    {
        _bcp = bcp;
    }

    public ChangeReport<ISudokuHighlighter> BuildReport(IReadOnlyList<NumericChange> changes, ISudokuSolvingState snapshot)
    {
        return new ChangeReport<ISudokuHighlighter>("BUG-Lite in " + _bcp.ToStringSequence(", "), lighter =>
        {
            foreach (var b in _bcp)
            {
                foreach (var p in b.Possibilities.EnumeratePossibilities())
                {
                    lighter.HighlightPossibility(p, b.One.Row, b.One.Column, StepColor.Cause2);
                    lighter.HighlightPossibility(p, b.Two.Row, b.Two.Column, StepColor.Cause2);
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