using System.Collections.Generic;
using Model.Core;
using Model.Core.Changes;
using Model.Core.Highlighting;
using Model.Sudokus.Solver.Utility;
using Model.Sudokus.Solver.Utility.Graphs;
using Model.Utility;
using Model.Utility.BitSets;

namespace Model.Sudokus.Solver.Strategies;

public class EmptyRectangleStrategy : SudokuStrategy
{
    public const string OfficialName = "Empty Rectangle";
    private const InstanceHandling DefaultInstanceHandling = InstanceHandling.UnorderedAll;

    public EmptyRectangleStrategy() : base(OfficialName, StepDifficulty.Medium, DefaultInstanceHandling)
    {
    }

    
    public override void Apply(ISudokuSolverData solverData)
    {
        for (int row = 0; row < 9; row++)
        {
            for (int col = 0; col < 9; col++)
            {
                foreach (var possibility in solverData.PossibilitiesAt(row, col).EnumeratePossibilities())
                {
                    var rowPositions = solverData.RowPositionsAt(row, possibility).Copy();
                    var columnPositions = solverData.ColumnPositionsAt(col, possibility).Copy();
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
                            if (Check(solverData, possibility, current, new Cell(row, rowPositions.First(col)),
                                    new Cell(columnPositions.First(row), col), true, true)) return;
                            break;
                        case (true, false) :
                            var rCell = new Cell(row, rowPositions.First(col));
                            foreach (var r in columnPositions)
                            {
                                if (r == row) continue;

                                if (Check(solverData, possibility, current, rCell, new Cell(
                                        r, col), true, false)) return;
                            }

                            break;
                        case (false, true) :
                            var cCell = new Cell(columnPositions.First(row), col);
                            foreach (var c in rowPositions)
                            {
                                if (c == col) continue;

                                if (Check(solverData, possibility, current, cCell, new Cell(
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

    private bool Check(ISudokuSolverData solverData, int possibility, Cell hinge, Cell one, Cell two,
        bool isOneLinkStrong, bool isTwoLinkStrong)
    {
        var positions = solverData.PositionsFor(possibility).Copy();
        
        int miniRow = one.Row == hinge.Row ? two.Row / 3 : one.Row / 3;
        int miniCol = one.Column == hinge.Column ? two.Column / 3 : one.Column / 3;

        if (positions.MiniGridCount(miniRow, miniCol) == 0) return false;

        positions.VoidRow(one.Row);
        positions.VoidColumn(one.Column);
        positions.VoidRow(two.Row);
        positions.VoidColumn(two.Column);

        if (positions.MiniGridCount(miniRow, miniCol) != 0) return false;

        if (isOneLinkStrong) solverData.ChangeBuffer.ProposePossibilityRemoval(possibility, two);
        if (isTwoLinkStrong) solverData.ChangeBuffer.ProposePossibilityRemoval(possibility, one);

        return solverData.ChangeBuffer.NotEmpty() && solverData.ChangeBuffer.Commit(
            new EmptyRectangleReportBuilder(hinge, one, two, isOneLinkStrong, isTwoLinkStrong,
                miniRow, miniCol, possibility)) && StopOnFirstPush;
    }
}

public class EmptyRectangleReportBuilder : IChangeReportBuilder<NumericChange, ISudokuSolvingState, ISudokuHighlighter>
{
    private readonly int _possibility;
    private readonly Cell _hinge;
    private readonly Cell _one;
    private readonly Cell _two;
    private readonly bool _isOneLinkStrong;
    private readonly bool _isTwoLinkStrong;
    private readonly int _miniRow;
    private readonly int _miniCol;

    public EmptyRectangleReportBuilder(Cell hinge, Cell one, Cell two, bool isOneLinkStrong, bool isTwoLinkStrong,
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

    public ChangeReport<ISudokuHighlighter> BuildReport(IReadOnlyList<NumericChange> changes, ISudokuSolvingState snapshot)
    {
        return new ChangeReport<ISudokuHighlighter>( "", lighter =>
        {
            lighter.HighlightCell(_hinge, StepColor.Cause1);
            lighter.HighlightCell(_one, StepColor.Cause2);
            lighter.HighlightCell(_two, StepColor.Cause2);

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
                    
                    if(snapshot.PossibilitiesAt(row, col).Contains(_possibility)) lighter.HighlightPossibility(
                        _possibility, row, col, StepColor.Cause3);
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