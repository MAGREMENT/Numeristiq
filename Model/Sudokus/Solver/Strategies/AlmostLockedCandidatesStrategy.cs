using System.Collections.Generic;
using Model.Core;
using Model.Core.Changes;
using Model.Core.Highlighting;
using Model.Sudokus.Solver.Position;
using Model.Sudokus.Solver.PossibilitySets;
using Model.Sudokus.Solver.Utility;
using Model.Utility;
using Model.Utility.BitSets;
using Model.Utility.Collections;

namespace Model.Sudokus.Solver.Strategies;

public class AlmostLockedCandidatesStrategy : SudokuStrategy
{
    public const string OfficialNameForType2 = "Almost Locked Pair";
    public const string OfficialNameForType3 = "Almost Locked Triple";
    private const InstanceHandling DefaultInstanceHandling = InstanceHandling.UnorderedAll;

    private readonly int _type;
    
    public AlmostLockedCandidatesStrategy(int type) : base("", Difficulty.Medium, DefaultInstanceHandling)
    {
        _type = type;
        switch (type)
        {
            case 2 : Name = OfficialNameForType2;
                break;
            case 3 : Name = OfficialNameForType3;
                break;
        }
    }
    
    public override void Apply(ISudokuSolverData solverData)
    {
        for (int miniRow = 0; miniRow < 3; miniRow++)
        {
            for (int miniCol = 0; miniCol < 3; miniCol++)
            {
                for (int u = 0; u < 3; u++)
                {
                    Cell[] rowCenterCells =
                    {
                        new(miniRow * 3 + u, miniCol * 3 + 0),
                        new(miniRow * 3 + u, miniCol * 3 + 1),
                        new(miniRow * 3 + u, miniCol * 3 + 2)
                    };
                    Cell[] colCenterCells =
                    {
                        new(miniRow * 3 + 0, miniCol * 3 + u),
                        new(miniRow * 3 + 1, miniCol * 3 + u),
                        new(miniRow * 3 + 2, miniCol * 3 + u)
                    };
                    var rowPossibilities = new ReadOnlyBitSet16();
                    var colPossibilities = new ReadOnlyBitSet16();

                    foreach (var cell in rowCenterCells)
                    {
                        rowPossibilities += solverData.PossibilitiesAt(cell);
                    }

                    foreach (var cell in colCenterCells)
                    {
                        colPossibilities += solverData.PossibilitiesAt(cell);
                    }
                    
                    foreach (var als in SearchRowForAls(solverData, miniRow * 3 + u, miniCol))
                    {
                        if(!rowPossibilities.ContainsAll(als.Possibilities)) continue;
                        
                        var correspondence = MiniGridCorrespondence(solverData, als.Possibilities,
                            u, Unit.Row, miniRow, miniCol);
                        if (correspondence.Count != als.PositionsCount) continue;

                        HandleCorrespondence(solverData, als.Possibilities, correspondence);
                        HandleAls(solverData, als.Possibilities, rowCenterCells, als);

                        if (solverData.ChangeBuffer.NotEmpty() && solverData.ChangeBuffer.Commit(
                                new AlmostLockedCandidatesReportBuilder(als, correspondence, rowCenterCells)) &&
                            StopOnFirstPush) return;
                    }

                    foreach (var als in SearchColumnForAls(solverData, miniCol * 3 + u, miniRow))
                    {
                        if(!colPossibilities.ContainsAll(als.Possibilities)) continue;
                        
                        var correspondence = MiniGridCorrespondence(solverData, als.Possibilities,
                            u, Unit.Column, miniRow, miniCol);
                        if (correspondence.Count != als.PositionsCount) continue;

                        HandleCorrespondence(solverData, als.Possibilities, correspondence);
                        HandleAls(solverData, als.Possibilities, colCenterCells, als);
                        
                        if (solverData.ChangeBuffer.NotEmpty() && solverData.ChangeBuffer.Commit(
                                new AlmostLockedCandidatesReportBuilder(als, correspondence, colCenterCells)) &&
                            StopOnFirstPush) return;
                    }

                    foreach (var als in SearchMiniGridForAls(solverData, miniRow, miniCol,
                                 u, Unit.Row))
                    {
                        if(!rowPossibilities.ContainsAll(als.Possibilities)) continue;
                        
                        var row = miniRow * 3 + u;
                        
                        var correspondence = RowCorrespondence(solverData, als.Possibilities, miniCol,
                            row);
                        if (correspondence.Count != als.PositionsCount) continue;

                        var cells = correspondence.ToCellArray(Unit.Row, row);
                        HandleCorrespondence(solverData, als.Possibilities, cells);
                        HandleAls(solverData, als.Possibilities, rowCenterCells, als);
                        
                        if (solverData.ChangeBuffer.NotEmpty() && solverData.ChangeBuffer.Commit(
                                new AlmostLockedCandidatesReportBuilder(als, cells, rowCenterCells)) &&
                            StopOnFirstPush) return;
                    }
                    
                    foreach (var als in SearchMiniGridForAls(solverData, miniRow, miniCol,
                                 u, Unit.Column))
                    {
                        if(!colPossibilities.ContainsAll(als.Possibilities)) continue;
                        
                        var col = miniCol * 3 + u;
                        
                        var correspondence = ColumnCorrespondence(solverData, als.Possibilities, miniRow,
                            col);
                        if (correspondence.Count != als.PositionsCount) continue;

                        var cells = correspondence.ToCellArray(Unit.Column, col);
                        HandleCorrespondence(solverData, als.Possibilities, cells);
                        HandleAls(solverData, als.Possibilities, colCenterCells, als);
                        
                        if (solverData.ChangeBuffer.NotEmpty() && solverData.ChangeBuffer.Commit(
                                new AlmostLockedCandidatesReportBuilder(als, cells, colCenterCells)) &&
                            StopOnFirstPush) return;
                    }
                }
            }
        }
    }

    private void HandleCorrespondence(ISudokuSolverData solverData, ReadOnlyBitSet16 possibilities,
        IEnumerable<Cell> cells)
    {
        foreach (var cell in cells)
        {
            foreach (var p in solverData.PossibilitiesAt(cell).EnumeratePossibilities())
            {
                if (!possibilities.Contains(p)) solverData.ChangeBuffer.ProposePossibilityRemoval(p, cell);
            }
        }
    }

    private void HandleAls(ISudokuSolverData solverData, ReadOnlyBitSet16 possibilities,
        Cell[] centerCells, IPossibilitySet als)
    {
        List<Cell> total = new List<Cell>(centerCells);
        total.AddRange(als.EnumerateCells());
        foreach (var ssc in SudokuCellUtility.SharedSeenEmptyCells(solverData, total))
        {
            foreach (var p in possibilities.EnumeratePossibilities())
            {
                solverData.ChangeBuffer.ProposePossibilityRemoval(p, ssc);
            }
        }
    }

    private LinePositions RowCorrespondence(ISudokuSolverData solverData, ReadOnlyBitSet16 possibilities,
        int exceptMiniCol, int row)
    {
        LinePositions result = new LinePositions();

        foreach (var p in possibilities.EnumeratePossibilities())
        {
            var pos = solverData.RowPositionsAt(row, p).Copy();
            pos.VoidMiniGrid(exceptMiniCol);

            result = result.Or(pos);
        }
        
        return result;
    }
    
    private LinePositions ColumnCorrespondence(ISudokuSolverData solverData, ReadOnlyBitSet16 possibilities,
        int exceptMiniRow, int col)
    {
        LinePositions result = new LinePositions();

        foreach (var p in possibilities.EnumeratePossibilities())
        {
            var pos = solverData.ColumnPositionsAt(col, p).Copy();
            pos.VoidMiniGrid(exceptMiniRow);

            result = result.Or(pos);
        }
        
        return result;
    }

    private BoxPositions MiniGridCorrespondence(ISudokuSolverData solverData, ReadOnlyBitSet16 possibilities,
        int exceptNumber, Unit unitExcept, int miniRow, int miniCol)
    {
        BoxPositions result = new(miniRow, miniCol);

        foreach (var p in possibilities.EnumeratePossibilities())
        {
            var pos = solverData.MiniGridPositionsAt(miniRow, miniCol, p).Copy();
            if (unitExcept == Unit.Row) pos.VoidGridRow(exceptNumber);
            else pos.VoidGridColumn(exceptNumber);

            result = result.Or(pos);
        }

        return result;
    }

    private List<IPossibilitySet> SearchRowForAls(ISudokuSolverData solverData, int row,
        int miniColExcept)
    {
        var result = new List<IPossibilitySet>();

        SearchRowForAls(solverData, row, miniColExcept, 0, new ReadOnlyBitSet16(), new LinePositions(), result);
        
        return result;
    }

    private void SearchRowForAls(ISudokuSolverData solverData, int row, int miniColExcept, int start,
        ReadOnlyBitSet16 currentPossibilities, LinePositions currentPositions, List<IPossibilitySet> result)
    {
        for (int col = start; col < 9; col++)
        {
            if (col / 3 == miniColExcept) continue;

            var poss = solverData.PossibilitiesAt(row, col);
            if (poss.Count == 0) continue;
            
            var newPossibilities = currentPossibilities | poss;
            if (newPossibilities.Count > _type) continue;

            currentPositions.Add(col);
            
            if(currentPositions.Count == _type - 1 && newPossibilities.Count == _type) result.Add(
                new SnapshotPossibilitySet(currentPositions.ToCellArray(Unit.Row, row), newPossibilities, solverData.CurrentState));
            else if (currentPositions.Count < _type - 1) SearchRowForAls(solverData, row, miniColExcept, col + 1,
                    newPossibilities, currentPositions, result);

            currentPositions.Remove(col);
        }
    }
    
    private List<IPossibilitySet> SearchColumnForAls(ISudokuSolverData solverData, int col,
        int miniRowExcept)
    {
        var result = new List<IPossibilitySet>();

        SearchColumnForAls(solverData, col, miniRowExcept, 0, new ReadOnlyBitSet16(),
            new LinePositions(), result);

        return result;
    }
    
    private void SearchColumnForAls(ISudokuSolverData solverData, int col, int miniRowExcept, int start,
        ReadOnlyBitSet16 currentPossibilities, LinePositions currentPositions, List<IPossibilitySet> result)
    {
        for (int row = start; row < 9; row++)
        {
            if (row / 3 == miniRowExcept) continue;

            var poss = solverData.PossibilitiesAt(row, col);
            if (poss.Count == 0) continue;
            
            var newPossibilities = currentPossibilities | poss;
            if (newPossibilities.Count > _type) continue;

            currentPositions.Add(row);
            
            if(currentPositions.Count == _type - 1 && newPossibilities.Count == _type) result.Add(
                new SnapshotPossibilitySet(currentPositions.ToCellArray(Unit.Column, col), newPossibilities, solverData.CurrentState));
            else if (currentPositions.Count < _type - 1) SearchColumnForAls(solverData, col, miniRowExcept, row + 1,
                newPossibilities, currentPositions, result);

            currentPositions.Remove(row);
        }
    }

    private List<IPossibilitySet> SearchMiniGridForAls(ISudokuSolverData solverData, int miniRow,
        int miniCol, int exceptNumber, Unit exceptUnit)
    {
        var result = new List<IPossibilitySet>();

        SearchMiniGridForAls(solverData, miniRow, miniCol, exceptNumber, exceptUnit, 0, new ReadOnlyBitSet16(),
            new BoxPositions(miniRow, miniCol), result);

        return result;
    }

    private void SearchMiniGridForAls(ISudokuSolverData solverData, int miniRow,
        int miniCol, int exceptNumber, Unit exceptUnit, int start, ReadOnlyBitSet16 currentPossibilities,
        BoxPositions currentPositions, List<IPossibilitySet> result)
    {
        for (int number = start; number < 9; number++)
        {
            var r = miniRow * 3 + number / 3;
            var c = miniCol * 3 + number % 3;
            switch (exceptUnit)
            {
                case Unit.Row:
                    if (r % 3 == exceptNumber) continue;
                    break;
                case Unit.Column:
                    if (c % 3 == exceptNumber) continue;
                    break;
            }

            var poss = solverData.PossibilitiesAt(r, c);
            if (poss.Count == 0) continue;
            
            var newPossibilities = currentPossibilities | poss;
            if (newPossibilities.Count > _type) continue;

            currentPositions.Add(number);
            
            if(currentPositions.Count == _type - 1 && newPossibilities.Count == _type) result.Add(
                new SnapshotPossibilitySet(currentPositions.ToCellArray(), newPossibilities, solverData.CurrentState));
            else if (currentPositions.Count < _type - 1) SearchMiniGridForAls(solverData, miniRow, miniCol,
                    exceptNumber, exceptUnit, number + 1, newPossibilities, currentPositions, result);

            currentPositions.Remove(number);
        }
    }
}

public class AlmostLockedCandidatesReportBuilder : IChangeReportBuilder<NumericChange, ISudokuSolvingState, ISudokuHighlighter>
{
    private readonly IPossibilitySet _als;
    private readonly IEnumerable<Cell> _correspondence;
    private readonly Cell[] _centerCells;

    public AlmostLockedCandidatesReportBuilder(IPossibilitySet als, IEnumerable<Cell> correspondence, Cell[] centerCells)
    {
        _als = als;
        _correspondence = correspondence;
        _centerCells = centerCells;
    }

    public ChangeReport<ISudokuHighlighter> BuildReport(IReadOnlyList<NumericChange> changes, ISudokuSolvingState snapshot)
    {
        return new ChangeReport<ISudokuHighlighter>($"Almost Locked Candidates in " +
                                                    $"{_centerCells.ToStringSequence(", ")}", lighter =>
        {
            foreach (var cell in _centerCells)
            {
                lighter.HighlightCell(cell, StepColor.Neutral);
            }
            
            foreach (var cell in _als.EnumerateCells())
            {
                lighter.HighlightCell(cell, StepColor.Cause1);
            }

            foreach (var cell in _correspondence)
            {
                foreach (var p in _als.Possibilities.EnumeratePossibilities())
                {
                    if (snapshot.PossibilitiesAt(cell).Contains(p))
                        lighter.HighlightPossibility(p, cell.Row, cell.Column, StepColor.Cause1);
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