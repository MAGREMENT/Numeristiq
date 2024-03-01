using System.Collections.Generic;
using Model.Helpers.Changes;
using Model.Helpers.Settings;
using Model.Helpers.Settings.Types;
using Model.Sudoku.Solver.BitSets;
using Model.Sudoku.Solver.Position;
using Model.Sudoku.Solver.StrategiesUtility;
using Model.Utility;

namespace Model.Sudoku.Solver.Strategies;

public class NonColorablePatternStrategy : SudokuStrategy
{
    public const string OfficialName = "Non-Colorable Pattern";
    private const OnCommitBehavior DefaultBehavior = OnCommitBehavior.Return;

    public override OnCommitBehavior DefaultOnCommitBehavior => DefaultBehavior;

    private readonly MinMaxSetting _possCount;
    private readonly IntSetting _maxNotInPatternCell;
    
    public NonColorablePatternStrategy(int minPossCount, int maxPossCount, int maxNotInPatternCell) : base(OfficialName, StrategyDifficulty.Extreme, DefaultBehavior)
    {
        _possCount = new MinMaxSetting("Possibility count", 2, 5, 2, 5, 1, minPossCount, maxPossCount);
        _maxNotInPatternCell = new IntSetting("Max out of pattern cells", new SliderViewInterface(1, 5, 1),
            maxNotInPatternCell);
        ModifiableSettings.Add(_maxNotInPatternCell);
    }

    
    public override void Apply(IStrategyUser strategyUser)
    {
        List<Cell> perfect = new();
        List<Cell> notPerfect = new();
        var poss = new ReadOnlyBitSet16();

        for (int possCount = _possCount.Value.Min; possCount <= _possCount.Value.Max; possCount++)
        {
            foreach (var combination in CombinationCalculator.EveryCombinationWithSpecificCount(3,
                         CombinationCalculator.NumbersSample))
            {
                foreach (var i in combination)
                {
                    poss += i;
                }

                for (int row = 0; row < 9; row++)
                {
                    for (int col = 0; col < 9; col++)
                    {
                        var p = strategyUser.PossibilitiesAt(row, col);
                        if (p.Count == 0) continue;

                        var cell = new Cell(row, col);
                        if (poss.ContainsAll(p)) perfect.Add(cell);
                        else if (poss.ContainsAny(p)) notPerfect.Add(cell);
                    }
                }

                if (perfect.Count > possCount && Try(strategyUser, perfect, notPerfect, poss)) return;

                poss = new ReadOnlyBitSet16();
                perfect.Clear();
                notPerfect.Clear();
            }
        }
    }

    private bool Try(IStrategyUser strategyUser, List<Cell> perfect, List<Cell> notPerfect, ReadOnlyBitSet16 poss)
    {
        List<CellPossibility> outPossibilities = new();
        foreach (var combination in
                 CombinationCalculator.EveryCombinationWithMaxCount(_maxNotInPatternCell.Value, notPerfect))
        {
            foreach (var cell in combination)
            {
                foreach (var p in strategyUser.PossibilitiesAt(cell).EnumeratePossibilities())
                {
                    if (!poss.Contains(p)) outPossibilities.Add(new CellPossibility(cell, p));
                }
            }

            var targets = outPossibilities.Count == 1 
                ? outPossibilities 
                : Cells.SharedSeenExistingPossibilities(strategyUser, outPossibilities);
            if (targets.Count == 0 || IsPatternValid(perfect, combination, poss.Count))
            {
                outPossibilities.Clear();
                continue;
            }

            foreach (var cp in targets)
            {
                if(outPossibilities.Count == 1) strategyUser.ChangeBuffer.ProposeSolutionAddition(outPossibilities[0]);
                else strategyUser.ChangeBuffer.ProposePossibilityRemoval(cp);
            }

            if (strategyUser.ChangeBuffer.NotEmpty() && strategyUser.ChangeBuffer.Commit(
                    new NonColorablePatternReportBuilder(perfect.ToArray(), combination, poss)) &&
                        OnCommitBehavior == OnCommitBehavior.Return) return true;
        }
        
        return false;
    }

    private bool IsPatternValid(IReadOnlyList<Cell> one, IReadOnlyList<Cell> two, int count)
    {
        var forbidden = new GridPositions[count];
        for (int i = 0; i < count; i++)
        {
            forbidden[i] = new GridPositions();
        }

        forbidden[0].Add(one[0]);
        return ValiditySearch(one, two, 1, forbidden);
    }

    private bool ValiditySearch(IReadOnlyList<Cell> one, IReadOnlyList<Cell> two, int cursor, GridPositions[] forbidden)
    {
        Cell current;
        if (cursor < one.Count) current = one[cursor];
        else if (cursor < one.Count + two.Count) current = two[cursor - one.Count];
        else return true;

        foreach (var f in forbidden)
        {
            if (f.RowCount(current.Row) > 0
                || f.ColumnCount(current.Column) > 0
                || f.MiniGridCount(current.Row / 3, current.Column / 3) > 0) continue;

            f.Add(current);
            if (ValiditySearch(one, two, cursor + 1, forbidden)) return true;
            f.Remove(current);
        }
        
        return false;
    }
}

public class NonColorablePatternReportBuilder : IChangeReportBuilder
{
    private readonly IReadOnlyList<Cell> _perfect;
    private readonly IReadOnlyList<Cell> _notPerfect;
    private readonly ReadOnlyBitSet16 _possibilities;

    public NonColorablePatternReportBuilder(IReadOnlyList<Cell> perfect, IReadOnlyList<Cell> notPerfect, ReadOnlyBitSet16 possibilities)
    {
        _perfect = perfect;
        _notPerfect = notPerfect;
        _possibilities = possibilities;
    }

    public ChangeReport Build(IReadOnlyList<SolverProgress> changes, IPossibilitiesHolder snapshot)
    {
        return new ChangeReport( "", lighter =>
        {
            foreach (var cell in _perfect)
            {
                foreach (var p in snapshot.PossibilitiesAt(cell).EnumeratePossibilities())
                {
                    lighter.HighlightPossibility(p, cell.Row, cell.Column, ChangeColoration.CauseOffTwo);
                }
            }
            foreach (var cell in _notPerfect)
            {
                foreach (var p in snapshot.PossibilitiesAt(cell).EnumeratePossibilities())
                {
                    lighter.HighlightPossibility(p, cell.Row, cell.Column, _possibilities.Contains(p)
                    ? ChangeColoration.CauseOffTwo : ChangeColoration.CauseOnOne);
                }
            }

            IChangeReportBuilder.HighlightChanges(lighter, changes);
        });
    }
}