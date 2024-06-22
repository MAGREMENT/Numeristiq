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

namespace Model.Sudokus.Solver.Strategies;

public class NonColorablePatternStrategy : SudokuStrategy
{
    public const string OfficialName = "Non-Colorable Pattern";
    private const InstanceHandling DefaultInstanceHandling = InstanceHandling.FirstOnly;

    private readonly MinMaxSetting _possCount;
    private readonly IntSetting _maxUnPerfectPatternCell;
    
    public NonColorablePatternStrategy(int minPossCount, int maxPossCount, int maxNotInPatternCell) : base(OfficialName, StepDifficulty.Extreme, DefaultInstanceHandling)
    {
        _possCount = new MinMaxSetting("Possibility count", 2, 5, 2, 5, 1, minPossCount, maxPossCount);
        _maxUnPerfectPatternCell = new IntSetting("Max un-perfect pattern cells", new SliderInteractionInterface(1, 5, 1),
            maxNotInPatternCell);
        AddSetting(_possCount);
        AddSetting(_maxUnPerfectPatternCell);
    }

    
    public override void Apply(ISudokuSolverData solverData)
    {
        List<Cell> perfect = new();
        List<Cell> notPerfect = new();
        List<Cell> multiNotPerfect = new();

        for (int possCount = _possCount.Value.Min; possCount <= _possCount.Value.Max; possCount++)
        {
            foreach (var poss in CombinationCalculator.EveryPossibilityCombinationWithSpecificCount(3))
            {
                for (int row = 0; row < 9; row++)
                {
                    for (int col = 0; col < 9; col++)
                    {
                        var p = solverData.PossibilitiesAt(row, col);
                        if (p.Count == 0) continue;

                        var cell = new Cell(row, col);
                        if (poss.ContainsAll(p)) perfect.Add(cell);
                        else if (poss.ContainsAny(p))
                        {
                            if ((poss - p).Count > 1) multiNotPerfect.Add(cell);
                            else notPerfect.Add(cell);
                        }
                    }
                }

                if (perfect.Count > possCount && Try(solverData, perfect, notPerfect, poss)) return;
                foreach (var cell in multiNotPerfect)
                {
                    if (Try(solverData, perfect, cell, poss)) return;
                }
                
                perfect.Clear();
                notPerfect.Clear();
                multiNotPerfect.Clear();
            }
        }
    }

    private bool Try(ISudokuSolverData solverData, List<Cell> perfect, Cell multiNotPerfect, ReadOnlyBitSet16 poss)
    {
        var list = new List<Cell>(1) { multiNotPerfect };
        if (IsPatternValid(perfect, list, poss.Count)) return false;

        foreach (var p in poss.EnumeratePossibilities())
        {
            solverData.ChangeBuffer.ProposePossibilityRemoval(p, multiNotPerfect);
        }
        
        return solverData.ChangeBuffer.NotEmpty() && solverData.ChangeBuffer.Commit(
                    new NonColorablePatternReportBuilder(perfect.ToArray(), list, poss)) &&
                StopOnFirstPush;
    }

    private bool Try(ISudokuSolverData solverData, List<Cell> perfect, List<Cell> notPerfect, ReadOnlyBitSet16 poss)
    {
        List<CellPossibility> outPossibilities = new();
        foreach (var combination in
                 CombinationCalculator.EveryCombinationWithMaxCount(_maxUnPerfectPatternCell.Value, notPerfect))
        {
            foreach (var cell in combination)
            {
                foreach (var p in solverData.PossibilitiesAt(cell).EnumeratePossibilities())
                {
                    if (!poss.Contains(p)) outPossibilities.Add(new CellPossibility(cell, p));
                }
            }

            var targets = outPossibilities.Count == 1 
                ? outPossibilities 
                : SudokuCellUtility.SharedSeenExistingPossibilities(solverData, outPossibilities);
            if (targets.Count == 0 || IsPatternValid(perfect, combination, poss.Count))
            {
                outPossibilities.Clear();
                continue;
            }

            foreach (var cp in targets)
            {
                if(outPossibilities.Count == 1) solverData.ChangeBuffer.ProposeSolutionAddition(outPossibilities[0]);
                else solverData.ChangeBuffer.ProposePossibilityRemoval(cp);
            }

            if (solverData.ChangeBuffer.NotEmpty() && solverData.ChangeBuffer.Commit(
                    new NonColorablePatternReportBuilder(perfect.ToArray(), combination, poss)) &&
                        StopOnFirstPush) return true;
        }
        
        return false;
    }

    private bool IsPatternValid(IReadOnlyList<Cell> one, IReadOnlyList<Cell> two, int count)
    {
        if (one.Count + two.Count <= count) return true;
        
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

public class NonColorablePatternReportBuilder : IChangeReportBuilder<IUpdatableSudokuSolvingState, ISudokuHighlighter>
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

    public ChangeReport<ISudokuHighlighter> BuildReport(IReadOnlyList<NumericChange> changes, IUpdatableSudokuSolvingState snapshot)
    {
        return new ChangeReport<ISudokuHighlighter>( "", lighter =>
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

            ChangeReportHelper.HighlightChanges(lighter, changes);
        });
    }
    
    public Clue<ISudokuHighlighter> BuildClue(IReadOnlyList<NumericChange> changes, IUpdatableSudokuSolvingState snapshot)
    {
        return Clue<ISudokuHighlighter>.Default();
    }
}