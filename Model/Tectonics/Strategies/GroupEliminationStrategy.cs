using System.Collections.Generic;
using Model.Helpers;
using Model.Helpers.Changes;
using Model.Helpers.Highlighting;
using Model.Sudokus.Solver;
using Model.Utility;
using Model.Utility.BitSets;

namespace Model.Tectonics.Strategies;

public class GroupEliminationStrategy : TectonicStrategy
{
    private const int Limit = 4;
    
    public GroupEliminationStrategy() : base("Group Elimination", StrategyDifficulty.Hard, InstanceHandling.FirstOnly)
    {
    }

    public override void Apply(ITectonicStrategyUser strategyUser)
    {
        HashSet<Cell> done = new();
        List<Cell> cells = new();
        for (int row = 0; row < strategyUser.Tectonic.RowCount; row++)
        {
            for (int col = 0; col < strategyUser.Tectonic.ColumnCount; col++)
            {
                var poss = strategyUser.PossibilitiesAt(row, col);
                if (poss.Count == 0) continue;

                var cell = new Cell(row, col);
                cells.Add(cell);
                if (SearchForGroup(strategyUser, cells, poss, done)) return;

                done.Add(cell);
                cells.Clear();
            }
        }
    }

    private bool SearchForGroup(ITectonicStrategyUser tectonicStrategyUser, List<Cell> cells, ReadOnlyBitSet8 possibilities, HashSet<Cell> done)
    {
        if (cells.Count == possibilities.Count && ProcessGroup(tectonicStrategyUser, cells, possibilities)) return true;

        if (cells.Count == Limit) return false;

        foreach (var cell in TectonicCellUtility.SharedNeighboringCells(tectonicStrategyUser.Tectonic, cells))
        {
            if (done.Contains(cell)) continue;

            var poss = tectonicStrategyUser.PossibilitiesAt(cell);
            if (poss.Count == 0) continue;

            var newPoss = possibilities | poss;
            if (newPoss.Count > Limit) continue;

            cells.Add(cell);
            SearchForGroup(tectonicStrategyUser, cells, newPoss, done);

            cells.RemoveAt(cells.Count - 1);
        }
        
        return false;
    }

    private bool ProcessGroup(ITectonicStrategyUser tectonicStrategyUser, List<Cell> cells, ReadOnlyBitSet8 possibilities)
    {
        List<Cell> buffer = new();
        foreach (var possibility in possibilities.EnumeratePossibilities())
        {
            foreach (var cell in cells)
            {
                if (tectonicStrategyUser.PossibilitiesAt(cell).Contains(possibility)) buffer.Add(cell);
            }

            foreach (var target in TectonicCellUtility.SharedSeenCells(tectonicStrategyUser.Tectonic, buffer))
            {
                tectonicStrategyUser.ChangeBuffer.ProposePossibilityRemoval(possibility, target);
            }
            
            buffer.Clear();
        }
        
        return tectonicStrategyUser.ChangeBuffer.Commit(new GroupEliminationReportBuilder(cells.ToArray()))
            && StopOnFirstPush;
    }
}

public class GroupEliminationReportBuilder : IChangeReportBuilder<IUpdatableTectonicSolvingState, ITectonicHighlighter>
{
    private readonly Cell[] _cells;

    public GroupEliminationReportBuilder(Cell[] cells)
    {
        _cells = cells;
    }

    public ChangeReport<ITectonicHighlighter> BuildReport(IReadOnlyList<SolverProgress> changes, IUpdatableTectonicSolvingState snapshot)
    {
        return new ChangeReport<ITectonicHighlighter>("", lighter =>
        {
            foreach (var cell in _cells)
            {
                lighter.HighlightCell(cell, ChangeColoration.CauseOffTwo);
            }

            ChangeReportHelper.HighlightChanges(lighter, changes);
        });
    }

    public Clue<ITectonicHighlighter> BuildClue(IReadOnlyList<SolverProgress> changes, IUpdatableTectonicSolvingState snapshot)
    {
        return Clue<ITectonicHighlighter>.Default();
    }
}