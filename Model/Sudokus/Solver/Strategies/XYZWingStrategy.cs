using System.Collections.Generic;
using Model.Core;
using Model.Core.Changes;
using Model.Core.Highlighting;
using Model.Sudokus.Solver.Utility;
using Model.Utility;
using Model.Utility.BitSets;

namespace Model.Sudokus.Solver.Strategies;

public class XYZWingStrategy : SudokuStrategy
{
    public const string OfficialName = "XYZ-Wing";
    private const InstanceHandling DefaultInstanceHandling = InstanceHandling.FirstOnly;

    public XYZWingStrategy() : base(OfficialName, StepDifficulty.Medium, DefaultInstanceHandling) {}

    public override void Apply(ISudokuSolverData solverData)
    {
        var map = new PositionsMap(solverData, Only2Possibilities);

        for (int row = 0; row < 9; row++)
        {
            for (int col = 0; col < 9; col++)
            {
                var hinge = solverData.PossibilitiesAt(row, col);
                if(hinge.Count != 3) continue;

                var miniRow = row / 3;
                var miniCol = col / 3;
                foreach (var mini in map.Minis[row / 3, col / 3])
                {
                    if (mini.Row == row && mini.Column == col) continue;

                    var firstCorner = solverData.PossibilitiesAt(mini.Row, mini.Column);
                    ReadOnlyBitSet16 and;
                    if ((and = firstCorner & hinge).Count != 2) continue;

                    foreach (var otherCol in map.Rows[row])
                    {
                        if(otherCol / 3 == miniCol) continue;

                        var secondCorner = solverData.PossibilitiesAt(row, otherCol);
                        if((secondCorner | firstCorner) != hinge) continue;

                        if (Process(solverData, row, col, mini.Row, mini.Column, row,
                                otherCol, (and & secondCorner).FirstPossibility())) return;
                    }

                    foreach (var otherRow in map.Columns[col])
                    {
                        if(otherRow / 3 == miniRow) continue;

                        var secondCorner = solverData.PossibilitiesAt(otherRow, col);
                        if((secondCorner | firstCorner) != hinge) continue;

                        if (Process(solverData, row, col, mini.Row, mini.Column,
                                otherRow, col, (and & secondCorner).FirstPossibility())) return;
                    }
                }
            }
        }
    }

    private bool Process(ISudokuSolverData solverData, int hingeRow, int hingeCol, int row1, int col1, int row2,
        int col2, int number)
    {
        foreach (var cell in SudokuCellUtility.SharedSeenCells(new Cell(hingeRow, hingeCol),
                     new Cell(row1, col1), new Cell(row2, col2)))
        {
            solverData.ChangeBuffer.ProposePossibilityRemoval(number, cell.Row, cell.Column);
        }

        return solverData.ChangeBuffer.Commit(
            new XYZWingReportBuilder(hingeRow, hingeCol, row1, col1, row2, col2)) && StopOnFirstPush;
    }
    
    private static bool Only2Possibilities(ReadOnlyBitSet16 possibilities)
    {
        return possibilities.Count == 2;
    }
}

public class XYZWingReportBuilder : IChangeReportBuilder<NumericChange, IUpdatableSudokuSolvingState, ISudokuHighlighter>
{
    private readonly int _hingeRow;
    private readonly int _hingeCol;
    private readonly int _row1;
    private readonly int _col1;
    private readonly int _row2;
    private readonly int _col2;

    public XYZWingReportBuilder(int hingeRow, int hingeCol, int row1, int col1, int row2, int col2)
    {
        _hingeRow = hingeRow;
        _hingeCol = hingeCol;
        _row1 = row1;
        _col1 = col1;
        _row2 = row2;
        _col2 = col2;
    }
    
    public ChangeReport<ISudokuHighlighter> BuildReport(IReadOnlyList<NumericChange> changes, IUpdatableSudokuSolvingState snapshot)
    {
        return new ChangeReport<ISudokuHighlighter>( "", lighter =>
        {
            lighter.HighlightCell(_hingeRow, _hingeCol, ChangeColoration.CauseOffTwo);
            lighter.HighlightCell(_row1, _col1, ChangeColoration.CauseOffOne);
            lighter.HighlightCell(_row2, _col2, ChangeColoration.CauseOffOne);

            ChangeReportHelper.HighlightChanges(lighter, changes);
        });
    }
    
    public Clue<ISudokuHighlighter> BuildClue(IReadOnlyList<NumericChange> changes, IUpdatableSudokuSolvingState snapshot)
    {
        return Clue<ISudokuHighlighter>.Default();
    }
}