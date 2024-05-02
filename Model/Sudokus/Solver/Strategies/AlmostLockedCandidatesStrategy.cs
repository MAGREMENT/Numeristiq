using System.Collections.Generic;
using Model.Helpers;
using Model.Helpers.Changes;
using Model.Helpers.Highlighting;
using Model.Sudokus.Solver.Position;
using Model.Sudokus.Solver.PossibilityPosition;
using Model.Sudokus.Solver.Utility;
using Model.Utility;
using Model.Utility.BitSets;

namespace Model.Sudokus.Solver.Strategies;

public class AlmostLockedCandidatesStrategy : SudokuStrategy
{
    public const string OfficialNameForType2 = "Almost Locked Pair";
    public const string OfficialNameForType3 = "Almost Locked Triple";
    private const InstanceHandling DefaultInstanceHandling = InstanceHandling.UnorderedAll;

    private readonly int _type;
    
    public AlmostLockedCandidatesStrategy(int type) : base("", StepDifficulty.Medium, DefaultInstanceHandling)
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
    
    public override void Apply(ISudokuStrategyUser strategyUser)
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
                        rowPossibilities += strategyUser.PossibilitiesAt(cell);
                    }

                    foreach (var cell in colCenterCells)
                    {
                        colPossibilities += strategyUser.PossibilitiesAt(cell);
                    }
                    
                    foreach (var als in SearchRowForAls(strategyUser, miniRow * 3 + u, miniCol))
                    {
                        if(!rowPossibilities.ContainsAll(als.Possibilities)) continue;
                        
                        var correspondence = MiniGridCorrespondence(strategyUser, als.Possibilities,
                            u, Unit.Row, miniRow, miniCol);
                        if (correspondence.Count != als.PositionsCount) continue;

                        HandleCorrespondence(strategyUser, als.Possibilities, correspondence);
                        HandleAls(strategyUser, als.Possibilities, rowCenterCells, als);

                        if (strategyUser.ChangeBuffer.NotEmpty() && strategyUser.ChangeBuffer.Commit(
                                new AlmostLockedCandidatesReportBuilder(als, correspondence, rowCenterCells)) &&
                            StopOnFirstPush) return;
                    }

                    foreach (var als in SearchColumnForAls(strategyUser, miniCol * 3 + u, miniRow))
                    {
                        if(!colPossibilities.ContainsAll(als.Possibilities)) continue;
                        
                        var correspondence = MiniGridCorrespondence(strategyUser, als.Possibilities,
                            u, Unit.Column, miniRow, miniCol);
                        if (correspondence.Count != als.PositionsCount) continue;

                        HandleCorrespondence(strategyUser, als.Possibilities, correspondence);
                        HandleAls(strategyUser, als.Possibilities, colCenterCells, als);
                        
                        if (strategyUser.ChangeBuffer.NotEmpty() && strategyUser.ChangeBuffer.Commit(
                                new AlmostLockedCandidatesReportBuilder(als, correspondence, colCenterCells)) &&
                            StopOnFirstPush) return;
                    }

                    foreach (var als in SearchMiniGridForAls(strategyUser, miniRow, miniCol,
                                 u, Unit.Row))
                    {
                        if(!rowPossibilities.ContainsAll(als.Possibilities)) continue;
                        
                        var row = miniRow * 3 + u;
                        
                        var correspondence = RowCorrespondence(strategyUser, als.Possibilities, miniCol,
                            row);
                        if (correspondence.Count != als.PositionsCount) continue;

                        var cells = correspondence.ToCellArray(Unit.Row, row);
                        HandleCorrespondence(strategyUser, als.Possibilities, cells);
                        HandleAls(strategyUser, als.Possibilities, rowCenterCells, als);
                        
                        if (strategyUser.ChangeBuffer.NotEmpty() && strategyUser.ChangeBuffer.Commit(
                                new AlmostLockedCandidatesReportBuilder(als, cells, rowCenterCells)) &&
                            StopOnFirstPush) return;
                    }
                    
                    foreach (var als in SearchMiniGridForAls(strategyUser, miniRow, miniCol,
                                 u, Unit.Column))
                    {
                        if(!colPossibilities.ContainsAll(als.Possibilities)) continue;
                        
                        var col = miniCol * 3 + u;
                        
                        var correspondence = ColumnCorrespondence(strategyUser, als.Possibilities, miniRow,
                            col);
                        if (correspondence.Count != als.PositionsCount) continue;

                        var cells = correspondence.ToCellArray(Unit.Column, col);
                        HandleCorrespondence(strategyUser, als.Possibilities, cells);
                        HandleAls(strategyUser, als.Possibilities, colCenterCells, als);
                        
                        if (strategyUser.ChangeBuffer.NotEmpty() && strategyUser.ChangeBuffer.Commit(
                                new AlmostLockedCandidatesReportBuilder(als, cells, colCenterCells)) &&
                            StopOnFirstPush) return;
                    }
                }
            }
        }
    }

    private void HandleCorrespondence(ISudokuStrategyUser strategyUser, ReadOnlyBitSet16 possibilities,
        IEnumerable<Cell> cells)
    {
        foreach (var cell in cells)
        {
            foreach (var p in strategyUser.PossibilitiesAt(cell).EnumeratePossibilities())
            {
                if (!possibilities.Contains(p)) strategyUser.ChangeBuffer.ProposePossibilityRemoval(p, cell);
            }
        }
    }

    private void HandleAls(ISudokuStrategyUser strategyUser, ReadOnlyBitSet16 possibilities,
        Cell[] centerCells, IPossibilitiesPositions als)
    {
        List<Cell> total = new List<Cell>(centerCells);
        total.AddRange(als.EachCell());
        foreach (var ssc in SudokuCellUtility.SharedSeenEmptyCells(strategyUser, total))
        {
            foreach (var p in possibilities.EnumeratePossibilities())
            {
                strategyUser.ChangeBuffer.ProposePossibilityRemoval(p, ssc);
            }
        }
    }

    private LinePositions RowCorrespondence(ISudokuStrategyUser strategyUser, ReadOnlyBitSet16 possibilities,
        int exceptMiniCol, int row)
    {
        LinePositions result = new LinePositions();

        foreach (var p in possibilities.EnumeratePossibilities())
        {
            var pos = strategyUser.RowPositionsAt(row, p).Copy();
            pos.VoidMiniGrid(exceptMiniCol);

            result = result.Or(pos);
        }
        
        return result;
    }
    
    private LinePositions ColumnCorrespondence(ISudokuStrategyUser strategyUser, ReadOnlyBitSet16 possibilities,
        int exceptMiniRow, int col)
    {
        LinePositions result = new LinePositions();

        foreach (var p in possibilities.EnumeratePossibilities())
        {
            var pos = strategyUser.ColumnPositionsAt(col, p).Copy();
            pos.VoidMiniGrid(exceptMiniRow);

            result = result.Or(pos);
        }
        
        return result;
    }

    private MiniGridPositions MiniGridCorrespondence(ISudokuStrategyUser strategyUser, ReadOnlyBitSet16 possibilities,
        int exceptNumber, Unit unitExcept, int miniRow, int miniCol)
    {
        MiniGridPositions result = new(miniRow, miniCol);

        foreach (var p in possibilities.EnumeratePossibilities())
        {
            var pos = strategyUser.MiniGridPositionsAt(miniRow, miniCol, p).Copy();
            if (unitExcept == Unit.Row) pos.VoidGridRow(exceptNumber);
            else pos.VoidGridColumn(exceptNumber);

            result = result.Or(pos);
        }

        return result;
    }

    private List<IPossibilitiesPositions> SearchRowForAls(ISudokuStrategyUser strategyUser, int row,
        int miniColExcept)
    {
        var result = new List<IPossibilitiesPositions>();

        SearchRowForAls(strategyUser, row, miniColExcept, 0, new ReadOnlyBitSet16(), new LinePositions(), result);
        
        return result;
    }

    private void SearchRowForAls(ISudokuStrategyUser strategyUser, int row, int miniColExcept, int start,
        ReadOnlyBitSet16 currentPossibilities, LinePositions currentPositions, List<IPossibilitiesPositions> result)
    {
        for (int col = start; col < 9; col++)
        {
            if (col / 3 == miniColExcept) continue;

            var poss = strategyUser.PossibilitiesAt(row, col);
            if (poss.Count == 0) continue;
            
            var newPossibilities = currentPossibilities | poss;
            if (newPossibilities.Count > _type) continue;

            currentPositions.Add(col);
            
            if(currentPositions.Count == _type - 1 && newPossibilities.Count == _type) result.Add(
                new CAPPossibilitiesPositions(currentPositions.ToCellArray(Unit.Row, row), newPossibilities, strategyUser));
            else if (currentPositions.Count < _type - 1) SearchRowForAls(strategyUser, row, miniColExcept, col + 1,
                    newPossibilities, currentPositions, result);

            currentPositions.Remove(col);
        }
    }
    
    private List<IPossibilitiesPositions> SearchColumnForAls(ISudokuStrategyUser strategyUser, int col,
        int miniRowExcept)
    {
        var result = new List<IPossibilitiesPositions>();

        SearchColumnForAls(strategyUser, col, miniRowExcept, 0, new ReadOnlyBitSet16(),
            new LinePositions(), result);

        return result;
    }
    
    private void SearchColumnForAls(ISudokuStrategyUser strategyUser, int col, int miniRowExcept, int start,
        ReadOnlyBitSet16 currentPossibilities, LinePositions currentPositions, List<IPossibilitiesPositions> result)
    {
        for (int row = start; row < 9; row++)
        {
            if (row / 3 == miniRowExcept) continue;

            var poss = strategyUser.PossibilitiesAt(row, col);
            if (poss.Count == 0) continue;
            
            var newPossibilities = currentPossibilities | poss;
            if (newPossibilities.Count > _type) continue;

            currentPositions.Add(row);
            
            if(currentPositions.Count == _type - 1 && newPossibilities.Count == _type) result.Add(
                new CAPPossibilitiesPositions(currentPositions.ToCellArray(Unit.Column, col), newPossibilities, strategyUser));
            else if (currentPositions.Count < _type - 1) SearchColumnForAls(strategyUser, col, miniRowExcept, row + 1,
                newPossibilities, currentPositions, result);

            currentPositions.Remove(row);
        }
    }

    private List<IPossibilitiesPositions> SearchMiniGridForAls(ISudokuStrategyUser strategyUser, int miniRow,
        int miniCol, int exceptNumber, Unit exceptUnit)
    {
        var result = new List<IPossibilitiesPositions>();

        SearchMiniGridForAls(strategyUser, miniRow, miniCol, exceptNumber, exceptUnit, 0, new ReadOnlyBitSet16(),
            new MiniGridPositions(miniRow, miniCol), result);

        return result;
    }

    private void SearchMiniGridForAls(ISudokuStrategyUser strategyUser, int miniRow,
        int miniCol, int exceptNumber, Unit exceptUnit, int start, ReadOnlyBitSet16 currentPossibilities,
        MiniGridPositions currentPositions, List<IPossibilitiesPositions> result)
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

            var poss = strategyUser.PossibilitiesAt(r, c);
            if (poss.Count == 0) continue;
            
            var newPossibilities = currentPossibilities | poss;
            if (newPossibilities.Count > _type) continue;

            currentPositions.Add(number);
            
            if(currentPositions.Count == _type - 1 && newPossibilities.Count == _type) result.Add(
                new CAPPossibilitiesPositions(currentPositions.ToCellArray(), newPossibilities, strategyUser));
            else if (currentPositions.Count < _type - 1) SearchMiniGridForAls(strategyUser, miniRow, miniCol,
                    exceptNumber, exceptUnit, number + 1, newPossibilities, currentPositions, result);

            currentPositions.Remove(number);
        }
    }
}

public class AlmostLockedCandidatesReportBuilder : IChangeReportBuilder<IUpdatableSudokuSolvingState, ISudokuHighlighter>
{
    private readonly IPossibilitiesPositions _als;
    private readonly IEnumerable<Cell> _correspondence;
    private readonly Cell[] _centerCells;

    public AlmostLockedCandidatesReportBuilder(IPossibilitiesPositions als, IEnumerable<Cell> correspondence, Cell[] centerCells)
    {
        _als = als;
        _correspondence = correspondence;
        _centerCells = centerCells;
    }

    public ChangeReport<ISudokuHighlighter> BuildReport(IReadOnlyList<SolverProgress> changes, IUpdatableSudokuSolvingState snapshot)
    {
        return new ChangeReport<ISudokuHighlighter>( "", lighter =>
        {
            foreach (var cell in _centerCells)
            {
                lighter.HighlightCell(cell, ChangeColoration.Neutral);
            }
            
            foreach (var cell in _als.EachCell())
            {
                lighter.HighlightCell(cell, ChangeColoration.CauseOffOne);
            }

            foreach (var cell in _correspondence)
            {
                foreach (var p in _als.Possibilities.EnumeratePossibilities())
                {
                    if (snapshot.PossibilitiesAt(cell).Contains(p))
                        lighter.HighlightPossibility(p, cell.Row, cell.Column, ChangeColoration.CauseOffOne);
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