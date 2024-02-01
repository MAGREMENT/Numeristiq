using System.Collections.Generic;
using Model.Sudoku.Solver.Helpers.Changes;
using Model.Sudoku.Solver.StrategiesUtility;
using Model.Sudoku.Solver.StrategiesUtility.Graphs;
using Model.Utility;

namespace Model.Sudoku.Solver.Strategies;

public class EmptyRectangleStrategy : AbstractStrategy
{
    public const string OfficialName = "Empty Rectangle";
    private const OnCommitBehavior DefaultBehavior = OnCommitBehavior.WaitForAll;

    public override OnCommitBehavior DefaultOnCommitBehavior => DefaultBehavior;
    
    public EmptyRectangleStrategy() : base(OfficialName, StrategyDifficulty.Medium, DefaultBehavior)
    {
    }

    
    public override void Apply(IStrategyManager strategyManager)
    {
        for (int row = 0; row < 9; row++)
        {
            for (int col = 0; col < 9; col++)
            {
                foreach (var possibility in strategyManager.PossibilitiesAt(row, col))
                {
                    var rowPositions = strategyManager.RowPositionsAt(row, possibility).Copy();
                    var columnPositions = strategyManager.ColumnPositionsAt(col, possibility).Copy();
                    if (rowPositions.AreAllInSameMiniGrid() || columnPositions.AreAllInSameMiniGrid()) continue;

                    bool rowStrong = rowPositions.Count == 2;
                    bool colStrong = columnPositions.Count == 2;

                    rowPositions.VoidMiniGrid(col / 3);
                    rowPositions.Add(col);
                    columnPositions.VoidMiniGrid(row / 3);
                    columnPositions.Add(row);

                    var current = new Cell(row, col);

                    switch (rowStrong, colStrong)
                    {
                        case (true, true) :
                            if (Check(strategyManager, possibility, current, new Cell(row, rowPositions.First(col)),
                                    new Cell(columnPositions.First(row), col), true, true)) return;
                            break;
                        case (true, false) :
                            var rCell = new Cell(row, rowPositions.First(col));
                            foreach (var r in columnPositions)
                            {
                                if (r == row) continue;

                                if (Check(strategyManager, possibility, current, rCell, new Cell(
                                        r, col), true, false)) return;
                            }

                            break;
                        case (false, true) :
                            var cCell = new Cell(columnPositions.First(row), col);
                            foreach (var c in rowPositions)
                            {
                                if (c == col) continue;

                                if (Check(strategyManager, possibility, current, cCell, new Cell(
                                        row, c), true, false)) return;
                            }

                            break;
                        default :
                            continue;
                    }
                }
            }
        }
    }

    private bool Check(IStrategyManager strategyManager, int possibility, Cell hinge, Cell one, Cell two,
        bool isOneLinkStrong, bool isTwoLinkStrong)
    {
        var positions = strategyManager.PositionsFor(possibility).Copy();
        
        int miniRow = one.Row == hinge.Row ? two.Row / 3 : one.Row / 3;
        int miniCol = one.Column == hinge.Column ? two.Column / 3 : one.Column / 3;

        if (positions.MiniGridCount(miniRow, miniCol) == 0) return false;

        positions.VoidRow(one.Row);
        positions.VoidColumn(one.Column);
        positions.VoidRow(two.Row);
        positions.VoidColumn(two.Column);

        if (positions.MiniGridCount(miniRow, miniCol) != 0) return false;

        if (isOneLinkStrong) strategyManager.ChangeBuffer.ProposePossibilityRemoval(possibility, two);
        if (isTwoLinkStrong) strategyManager.ChangeBuffer.ProposePossibilityRemoval(possibility, one);

        return strategyManager.ChangeBuffer.NotEmpty() && strategyManager.ChangeBuffer.Commit(this,
            new RectangleEliminationReportBuilder(hinge, one, two, isOneLinkStrong, isTwoLinkStrong,
                miniRow, miniCol, possibility)) && OnCommitBehavior == OnCommitBehavior.Return;
    }
}

public class RectangleEliminationReportBuilder : IChangeReportBuilder
{
    private readonly int _possibility;
    private readonly Cell _hinge;
    private readonly Cell _one;
    private readonly Cell _two;
    private readonly bool _isOneLinkStrong;
    private readonly bool _isTwoLinkStrong;
    private readonly int _miniRow;
    private readonly int _miniCol;

    public RectangleEliminationReportBuilder(Cell hinge, Cell one, Cell two, bool isOneLinkStrong, bool isTwoLinkStrong,
        int miniRow, int miniCol, int possibility)
    {
        _hinge = hinge;
        _one = one;
        _two = two;
        _isOneLinkStrong = isOneLinkStrong;
        _isTwoLinkStrong = isTwoLinkStrong;
        _miniRow = miniRow;
        _miniCol = miniCol;
        _possibility = possibility;
    }

    public ChangeReport Build(IReadOnlyList<SolverChange> changes, IPossibilitiesHolder snapshot)
    {
        return new ChangeReport(IChangeReportBuilder.ChangesToString(changes), "", lighter =>
        {
            lighter.HighlightCell(_hinge, ChangeColoration.CauseOffOne);
            lighter.HighlightCell(_one, ChangeColoration.CauseOffTwo);
            lighter.HighlightCell(_two, ChangeColoration.CauseOffTwo);

            lighter.CreateLink(new CellPossibility(_hinge, _possibility), new CellPossibility(_one, _possibility),
                _isOneLinkStrong ? LinkStrength.Strong : LinkStrength.Weak);
            lighter.CreateLink(new CellPossibility(_hinge, _possibility), new CellPossibility(_two, _possibility),
                _isTwoLinkStrong ? LinkStrength.Strong : LinkStrength.Weak);

            for (int gridRow = 0; gridRow < 3; gridRow++)
            {
                for (int gridCol = 0; gridCol < 3; gridCol++)
                {
                    var row = _miniRow * 3 + gridRow;
                    var col = _miniCol * 3 + gridCol;
                    
                    if(snapshot.PossibilitiesAt(row, col).Peek(_possibility)) lighter.HighlightPossibility(
                        _possibility, row, col, ChangeColoration.CauseOffThree);
                }
            }

            IChangeReportBuilder.HighlightChanges(lighter, changes);
        });
    }
}