using System.Collections.Generic;
using Global;
using Global.Enums;
using Model.Solver.Helpers.Changes;
using Model.Solver.Position;
using Model.Solver.Possibility;
using Model.Solver.PossibilityPosition;
using Model.Solver.StrategiesUtility;

namespace Model.Solver.Strategies;

public class AlmostLockedCandidatesStrategy : AbstractStrategy
{
    public const string OfficialNameForType2 = "Almost Locked Pair";
    public const string OfficialNameForType3 = "Almost Locked Triple";
    private const OnCommitBehavior DefaultBehavior = OnCommitBehavior.WaitForAll;

    public override OnCommitBehavior DefaultOnCommitBehavior => DefaultBehavior;

    private readonly int _type;
    
    public AlmostLockedCandidatesStrategy(int type) : base("", StrategyDifficulty.Medium, DefaultBehavior)
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
    
    public override void Apply(IStrategyManager strategyManager)
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
                    var rowPossibilities = Possibilities.NewEmpty();
                    var colPossibilities = Possibilities.NewEmpty();

                    foreach (var cell in rowCenterCells)
                    {
                        rowPossibilities.Add(strategyManager.PossibilitiesAt(cell));
                    }

                    foreach (var cell in colCenterCells)
                    {
                        colPossibilities.Add(strategyManager.PossibilitiesAt(cell));
                    }
                    
                    foreach (var als in SearchRowForAls(strategyManager, miniRow * 3 + u, miniCol))
                    {
                        if(!rowPossibilities.PeekAll(als.Possibilities)) continue;
                        
                        var correspondence = MiniGridCorrespondence(strategyManager, als.Possibilities,
                            u, Unit.Row, miniRow, miniCol);
                        if (correspondence.Count != als.PositionsCount) continue;

                        HandleCorrespondence(strategyManager, als.Possibilities, correspondence);
                        HandleAls(strategyManager, als.Possibilities, rowCenterCells, als);

                        if (strategyManager.ChangeBuffer.NotEmpty() && strategyManager.ChangeBuffer.Commit(this,
                                new AlmostLockedCandidatesReportBuilder(als, correspondence, rowCenterCells)) &&
                            OnCommitBehavior == OnCommitBehavior.Return) return;
                    }

                    foreach (var als in SearchColumnForAls(strategyManager, miniCol * 3 + u, miniRow))
                    {
                        if(!colPossibilities.PeekAll(als.Possibilities)) continue;
                        
                        var correspondence = MiniGridCorrespondence(strategyManager, als.Possibilities,
                            u, Unit.Column, miniRow, miniCol);
                        if (correspondence.Count != als.PositionsCount) continue;

                        HandleCorrespondence(strategyManager, als.Possibilities, correspondence);
                        HandleAls(strategyManager, als.Possibilities, colCenterCells, als);
                        
                        if (strategyManager.ChangeBuffer.NotEmpty() && strategyManager.ChangeBuffer.Commit(this,
                                new AlmostLockedCandidatesReportBuilder(als, correspondence, colCenterCells)) &&
                            OnCommitBehavior == OnCommitBehavior.Return) return;
                    }

                    foreach (var als in SearchMiniGridForAls(strategyManager, miniRow, miniCol,
                                 u, Unit.Row))
                    {
                        if(!rowPossibilities.PeekAll(als.Possibilities)) continue;
                        
                        var row = miniRow * 3 + u;
                        
                        var correspondence = RowCorrespondence(strategyManager, als.Possibilities, miniCol,
                            row);
                        if (correspondence.Count != als.PositionsCount) continue;

                        var cells = correspondence.ToCellArray(Unit.Row, row);
                        HandleCorrespondence(strategyManager, als.Possibilities, cells);
                        HandleAls(strategyManager, als.Possibilities, rowCenterCells, als);
                        
                        if (strategyManager.ChangeBuffer.NotEmpty() && strategyManager.ChangeBuffer.Commit(this,
                                new AlmostLockedCandidatesReportBuilder(als, cells, rowCenterCells)) &&
                            OnCommitBehavior == OnCommitBehavior.Return) return;
                    }
                    
                    foreach (var als in SearchMiniGridForAls(strategyManager, miniRow, miniCol,
                                 u, Unit.Column))
                    {
                        if(!colPossibilities.PeekAll(als.Possibilities)) continue;
                        
                        var col = miniCol * 3 + u;
                        
                        var correspondence = ColumnCorrespondence(strategyManager, als.Possibilities, miniRow,
                            col);
                        if (correspondence.Count != als.PositionsCount) continue;

                        var cells = correspondence.ToCellArray(Unit.Column, col);
                        HandleCorrespondence(strategyManager, als.Possibilities, cells);
                        HandleAls(strategyManager, als.Possibilities, colCenterCells, als);
                        
                        if (strategyManager.ChangeBuffer.NotEmpty() && strategyManager.ChangeBuffer.Commit(this,
                                new AlmostLockedCandidatesReportBuilder(als, cells, colCenterCells)) &&
                            OnCommitBehavior == OnCommitBehavior.Return) return;
                    }
                }
            }
        }
    }

    private void HandleCorrespondence(IStrategyManager strategyManager, IReadOnlyPossibilities possibilities,
        IEnumerable<Cell> cells)
    {
        foreach (var cell in cells)
        {
            foreach (var p in strategyManager.PossibilitiesAt(cell))
            {
                if (!possibilities.Peek(p)) strategyManager.ChangeBuffer.ProposePossibilityRemoval(p, cell);
            }
        }
    }

    private void HandleAls(IStrategyManager strategyManager, IReadOnlyPossibilities possibilities,
        Cell[] centerCells, IPossibilitiesPositions als)
    {
        List<Cell> total = new List<Cell>(centerCells);
        total.AddRange(als.EachCell());
        foreach (var ssc in Cells.SharedSeenEmptyCells(strategyManager, total))
        {
            foreach (var p in possibilities)
            {
                strategyManager.ChangeBuffer.ProposePossibilityRemoval(p, ssc);
            }
        }
    }

    private LinePositions RowCorrespondence(IStrategyManager strategyManager, IReadOnlyPossibilities possibilities,
        int exceptMiniCol, int row)
    {
        LinePositions result = new LinePositions();

        foreach (var p in possibilities)
        {
            var pos = strategyManager.RowPositionsAt(row, p).Copy();
            pos.VoidMiniGrid(exceptMiniCol);

            result = result.Or(pos);
        }
        
        return result;
    }
    
    private LinePositions ColumnCorrespondence(IStrategyManager strategyManager, IReadOnlyPossibilities possibilities,
        int exceptMiniRow, int col)
    {
        LinePositions result = new LinePositions();

        foreach (var p in possibilities)
        {
            var pos = strategyManager.ColumnPositionsAt(col, p).Copy();
            pos.VoidMiniGrid(exceptMiniRow);

            result = result.Or(pos);
        }
        
        return result;
    }

    private MiniGridPositions MiniGridCorrespondence(IStrategyManager strategyManager, IReadOnlyPossibilities possibilities,
        int exceptNumber, Unit unitExcept, int miniRow, int miniCol)
    {
        MiniGridPositions result = new(miniRow, miniCol);

        foreach (var p in possibilities)
        {
            var pos = strategyManager.MiniGridPositionsAt(miniRow, miniCol, p).Copy();
            if (unitExcept == Unit.Row) pos.VoidGridRow(exceptNumber);
            else pos.VoidGridColumn(exceptNumber);

            result = result.Or(pos);
        }

        return result;
    }

    private List<IPossibilitiesPositions> SearchRowForAls(IStrategyManager strategyManager, int row,
        int miniColExcept)
    {
        var result = new List<IPossibilitiesPositions>();

        SearchRowForAls(strategyManager, row, miniColExcept, 0, Possibilities.NewEmpty(), new LinePositions(), result);
        
        return result;
    }

    private void SearchRowForAls(IStrategyManager strategyManager, int row, int miniColExcept, int start,
        Possibilities currentPossibilities, LinePositions currentPositions, List<IPossibilitiesPositions> result)
    {
        for (int col = start; col < 9; col++)
        {
            if (col / 3 == miniColExcept) continue;

            var poss = strategyManager.PossibilitiesAt(row, col);
            if (poss.Count == 0) continue;
            
            var newPossibilities = currentPossibilities.Or(poss);
            if (newPossibilities.Count > _type) continue;

            currentPositions.Add(col);
            
            if(currentPositions.Count == _type - 1 && newPossibilities.Count == _type) result.Add(
                new CAPPossibilitiesPositions(currentPositions.ToCellArray(Unit.Row, row), newPossibilities, strategyManager));
            else if (currentPositions.Count < _type - 1) SearchRowForAls(strategyManager, row, miniColExcept, col + 1,
                    newPossibilities, currentPositions, result);

            currentPositions.Remove(col);
        }
    }
    
    private List<IPossibilitiesPositions> SearchColumnForAls(IStrategyManager strategyManager, int col,
        int miniRowExcept)
    {
        var result = new List<IPossibilitiesPositions>();

        SearchColumnForAls(strategyManager, col, miniRowExcept, 0, Possibilities.NewEmpty(),
            new LinePositions(), result);

        return result;
    }
    
    private void SearchColumnForAls(IStrategyManager strategyManager, int col, int miniRowExcept, int start,
        Possibilities currentPossibilities, LinePositions currentPositions, List<IPossibilitiesPositions> result)
    {
        for (int row = start; row < 9; row++)
        {
            if (row / 3 == miniRowExcept) continue;

            var poss = strategyManager.PossibilitiesAt(row, col);
            if (poss.Count == 0) continue;
            
            var newPossibilities = currentPossibilities.Or(poss);
            if (newPossibilities.Count > _type) continue;

            currentPositions.Add(row);
            
            if(currentPositions.Count == _type - 1 && newPossibilities.Count == _type) result.Add(
                new CAPPossibilitiesPositions(currentPositions.ToCellArray(Unit.Column, col), newPossibilities, strategyManager));
            else if (currentPositions.Count < _type - 1) SearchColumnForAls(strategyManager, col, miniRowExcept, row + 1,
                newPossibilities, currentPositions, result);

            currentPositions.Remove(row);
        }
    }

    private List<IPossibilitiesPositions> SearchMiniGridForAls(IStrategyManager strategyManager, int miniRow,
        int miniCol, int exceptNumber, Unit exceptUnit)
    {
        var result = new List<IPossibilitiesPositions>();

        SearchMiniGridForAls(strategyManager, miniRow, miniCol, exceptNumber, exceptUnit, 0, Possibilities.NewEmpty(),
            new MiniGridPositions(miniRow, miniCol), result);

        return result;
    }

    private void SearchMiniGridForAls(IStrategyManager strategyManager, int miniRow,
        int miniCol, int exceptNumber, Unit exceptUnit, int start, Possibilities currentPossibilities,
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

            var poss = strategyManager.PossibilitiesAt(r, c);
            if (poss.Count == 0) continue;
            
            var newPossibilities = currentPossibilities.Or(poss);
            if (newPossibilities.Count > _type) continue;

            currentPositions.Add(number);
            
            if(currentPositions.Count == _type - 1 && newPossibilities.Count == _type) result.Add(
                new CAPPossibilitiesPositions(currentPositions.ToCellArray(), newPossibilities, strategyManager));
            else if (currentPositions.Count < _type - 1) SearchMiniGridForAls(strategyManager, miniRow, miniCol,
                    exceptNumber, exceptUnit, number + 1, newPossibilities, currentPositions, result);

            currentPositions.Remove(number);
        }
    }
}

public class AlmostLockedCandidatesReportBuilder : IChangeReportBuilder
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

    public ChangeReport Build(IReadOnlyList<SolverChange> changes, IPossibilitiesHolder snapshot)
    {
        return new ChangeReport(IChangeReportBuilder.ChangesToString(changes), "", lighter =>
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
                foreach (var p in _als.Possibilities)
                {
                    if (snapshot.PossibilitiesAt(cell).Peek(p))
                        lighter.HighlightPossibility(p, cell.Row, cell.Column, ChangeColoration.CauseOffOne);
                }
            }

            IChangeReportBuilder.HighlightChanges(lighter, changes);
        });
    }
}