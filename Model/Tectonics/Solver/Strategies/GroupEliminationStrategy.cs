using System.Collections.Generic;
using Model.Core;
using Model.Core.Changes;
using Model.Core.Highlighting;
using Model.Utility;
using Model.Utility.BitSets;

namespace Model.Tectonics.Solver.Strategies;

public class GroupEliminationStrategy : Strategy<ITectonicSolverData>, ICommitComparer<NumericChange>
{
    private const int Limit = 4;
    
    public GroupEliminationStrategy() : base("Group Elimination", StepDifficulty.Hard, InstanceHandling.BestOnly)
    {
    }

    public override void Apply(ITectonicSolverData data)
    {
        HashSet<Cell> done = new();
        List<Cell> cells = new();
        for (int row = 0; row < data.Tectonic.RowCount; row++)
        {
            for (int col = 0; col < data.Tectonic.ColumnCount; col++)
            {
                var poss = data.PossibilitiesAt(row, col);
                if (poss.Count == 0) continue;

                var cell = new Cell(row, col);
                cells.Add(cell);
                if (SearchForGroup(data, cells, poss, done)) return;

                done.Add(cell);
                cells.Clear();
            }
        }
    }

    private bool SearchForGroup(ITectonicSolverData tectonicSolverData, List<Cell> cells, ReadOnlyBitSet8 possibilities, HashSet<Cell> done)
    {
        if (cells.Count == possibilities.Count && ProcessGroup(tectonicSolverData, cells, possibilities)) return true;

        if (cells.Count == Limit) return false;

        foreach (var cell in TectonicCellUtility.SharedNeighboringCells(tectonicSolverData.Tectonic, cells))
        {
            if (done.Contains(cell)) continue;

            var poss = tectonicSolverData.PossibilitiesAt(cell);
            if (poss.Count == 0) continue;

            var newPoss = possibilities | poss;
            if (newPoss.Count > Limit) continue;

            cells.Add(cell);
            SearchForGroup(tectonicSolverData, cells, newPoss, done);

            cells.RemoveAt(cells.Count - 1);
        }
        
        return false;
    }

    private bool ProcessGroup(ITectonicSolverData tectonicSolverData, List<Cell> cells, ReadOnlyBitSet8 possibilities)
    {
        List<Cell> buffer = new();
        foreach (var possibility in possibilities.EnumeratePossibilities())
        {
            foreach (var cell in cells)
            {
                if (tectonicSolverData.PossibilitiesAt(cell).Contains(possibility)) buffer.Add(cell);
            }

            foreach (var target in TectonicCellUtility.SharedSeenCells(tectonicSolverData.Tectonic, buffer))
            {
                tectonicSolverData.ChangeBuffer.ProposePossibilityRemoval(possibility, target);
            }
            
            buffer.Clear();
        }
        
        return tectonicSolverData.ChangeBuffer.Commit(new GroupEliminationReportBuilder(cells.ToArray()))
            && StopOnFirstPush;
    }

    public int Compare(IChangeCommit<NumericChange> first, IChangeCommit<NumericChange> second)
    {
        if (first.TryGetBuilder<GroupEliminationReportBuilder>(out var gerp1)
            || second.TryGetBuilder<GroupEliminationReportBuilder>(out var gerp2)) return 0;

        return gerp2.Cells.Length - gerp1.Cells.Length;
    }
}

public class GroupEliminationReportBuilder : IChangeReportBuilder<NumericChange, INumericSolvingState, ITectonicHighlighter>
{
    public Cell[] Cells { get; }

    public GroupEliminationReportBuilder(Cell[] cells)
    {
        Cells = cells;
    }

    public ChangeReport<ITectonicHighlighter> BuildReport(IReadOnlyList<NumericChange> changes, INumericSolvingState snapshot)
    {
        return new ChangeReport<ITectonicHighlighter>("", lighter =>
        {
            foreach (var cell in Cells)
            {
                lighter.HighlightCell(cell, ChangeColoration.CauseOffTwo);
            }

            ChangeReportHelper.HighlightChanges(lighter, changes);
        });
    }

    public Clue<ITectonicHighlighter> BuildClue(IReadOnlyList<NumericChange> changes, INumericSolvingState snapshot)
    {
        return Clue<ITectonicHighlighter>.Default();
    }
}