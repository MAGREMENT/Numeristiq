using System.Collections.Generic;
using System.Text;
using Model.Core;
using Model.Core.Changes;
using Model.Core.Highlighting;
using Model.Sudokus.Solver.Position;
using Model.Utility;
using Model.Utility.BitSets;

namespace Model.Sudokus.Solver.Strategies;

public class HiddenDoublesStrategy : SudokuStrategy
{
    public const string OfficialName = "Hidden Doubles";
    private const InstanceHandling DefaultInstanceHandling = InstanceHandling.UnorderedAll;

    public HiddenDoublesStrategy() : base(OfficialName, StepDifficulty.Easy, DefaultInstanceHandling){}
    
    public override void Apply(ISudokuSolverData solverData)
    {
        Dictionary<IReadOnlyLinePositions, int> lines = new();
        Dictionary<IReadOnlyBoxPositions, int> minis = new();
        for (int row = 0; row < 9; row++)
        {
            for (int number = 1; number <= 9; number++)
            {
                var pos = solverData.RowPositionsAt(row, number);
                if (pos.Count != 2) continue;

                if (lines.TryGetValue(pos, out var n))
                {
                    if (ProcessRow(solverData, pos, row, number, n)) return;
                }
                else lines.Add(pos, number);
            }

            lines.Clear();
        }

        for (int col = 0; col < 9; col++)
        {
            for (int number = 1; number <= 9; number++)
            {
                var pos = solverData.ColumnPositionsAt(col, number);
                if (pos.Count != 2) continue;

                if (lines.TryGetValue(pos, out var n))
                {
                    if (ProcessColumn(solverData, pos, col, number, n)) return;
                }
                else lines.Add(pos, number);
            }

            lines.Clear();
        }

        for (int miniRow = 0; miniRow < 3; miniRow++)
        {
            for (int miniCol = 0; miniCol < 3; miniCol++)
            {
                for (int number = 1; number <= 9; number++)
                {
                    var pos = solverData.MiniGridPositionsAt(miniRow, miniCol, number);
                    if (pos.Count != 2) continue;

                    if (minis.TryGetValue(pos, out var n))
                    {
                        if (ProcessMiniGrid(solverData, pos, number, n)) return;
                    }
                    else minis.Add(pos, number);
                }

                minis.Clear();
            }
        }
    }
    
    private bool ProcessRow(ISudokuSolverData solverData, IReadOnlyLinePositions positions, int row, int n1,
        int n2)
    {
        foreach (var col in positions)
        {
            foreach (var possibility in solverData.PossibilitiesAt(row, col).EnumeratePossibilities())
            {
                if(possibility == n1 || possibility == n2) continue;

                solverData.ChangeBuffer.ProposePossibilityRemoval(possibility, row, col);
            }
        }

        return solverData.ChangeBuffer.Commit(
            new LineHiddenDoublesReportBuilder(row, positions, n1, n2, Unit.Row)) && StopOnFirstPush;
    }

    private bool ProcessColumn(ISudokuSolverData solverData, IReadOnlyLinePositions positions, int col,
        int n1, int n2)
    {
        foreach(var row in positions)
        {
            foreach (var possibility in solverData.PossibilitiesAt(row, col).EnumeratePossibilities())
            {
                if(possibility == n1 || possibility == n2) continue;

                solverData.ChangeBuffer.ProposePossibilityRemoval(possibility, row, col);
            }
        }

        return solverData.ChangeBuffer.Commit(
            new LineHiddenDoublesReportBuilder(col, positions, n1, n2, Unit.Column)) && StopOnFirstPush;
    }

    private bool ProcessMiniGrid(ISudokuSolverData solverData, IReadOnlyBoxPositions positions, int n1, int n2)
    {
        foreach (var cell in positions)
        {
            foreach (var possibility in solverData.PossibilitiesAt(cell.Row, cell.Column).EnumeratePossibilities())
            {
                if(possibility == n1 || possibility == n2) continue;

                solverData.ChangeBuffer.ProposePossibilityRemoval(possibility, cell.Row, cell.Column);
            }
        }

        return solverData.ChangeBuffer.Commit(
            new MiniGridHiddenDoublesReportBuilder(positions, n1, n2)) && StopOnFirstPush;
    }
}

public class LineHiddenDoublesReportBuilder : IChangeReportBuilder<NumericChange, ISudokuSolvingState, ISudokuHighlighter>
{
    private readonly int _unitNumber;
    private readonly IReadOnlyLinePositions _pos;
    private readonly int _n1;
    private readonly int _n2;
    private readonly Unit _unit;

    public LineHiddenDoublesReportBuilder(int unitNumber, IReadOnlyLinePositions pos, int n1, int n2, Unit unit)
    {
        _unitNumber = unitNumber;
        _pos = pos;
        _n1 = n1;
        _n2 = n2;
        _unit = unit;
    }

    public ChangeReport<ISudokuHighlighter> BuildReport(IReadOnlyList<NumericChange> changes, ISudokuSolvingState snapshot)
    {
        var cells = _pos.ToCellArray(_unit, _unitNumber);

        return new ChangeReport<ISudokuHighlighter>(Description(cells), lighter =>
        {
            foreach (var cell in cells)
            {
                lighter.HighlightPossibility(_n1, cell.Row, cell.Column, ChangeColoration.CauseOffOne);
                lighter.HighlightPossibility(_n2, cell.Row, cell.Column, ChangeColoration.CauseOffOne);
            }

            ChangeReportHelper.HighlightChanges(lighter, changes);
        });
    }

    private string Description(IReadOnlyList<Cell> cells)
    {
        var builder = new StringBuilder($"Hidden Doubles in {cells[0]}, {cells[1]} for ");

        var n = 0;
        _pos.Next(ref n);
        var f = n;
        _pos.Next(ref n);
        builder.Append(f + ", " + n);

        return builder.ToString();
    }
    
    public Clue<ISudokuHighlighter> BuildClue(IReadOnlyList<NumericChange> changes, ISudokuSolvingState snapshot)
    {
        return Clue<ISudokuHighlighter>.Default();
    }
}

public class MiniGridHiddenDoublesReportBuilder : IChangeReportBuilder<NumericChange, ISudokuSolvingState, ISudokuHighlighter>
{
    private readonly IReadOnlyBoxPositions _pos;
    private readonly int _n1;
    private readonly int _n2;

    public MiniGridHiddenDoublesReportBuilder(IReadOnlyBoxPositions pos, int n1, int n2)
    {
        _pos = pos;
        _n1 = n1;
        _n2 = n2;
    }

    public ChangeReport<ISudokuHighlighter> BuildReport(IReadOnlyList<NumericChange> changes, ISudokuSolvingState snapshot)
    {
        var cells = _pos.ToCellArray();

        return new ChangeReport<ISudokuHighlighter>(Description(cells), lighter =>
        {
            foreach (var cell in cells)
            {
                lighter.HighlightPossibility(_n1, cell.Row, cell.Column, ChangeColoration.CauseOffOne);
                lighter.HighlightPossibility(_n2, cell.Row, cell.Column, ChangeColoration.CauseOffOne);
            }

            ChangeReportHelper.HighlightChanges(lighter, changes);
        });
    }
    
    private string Description(IReadOnlyList<Cell> cells)
    {
        var builder = new StringBuilder($"Hidden Doubles in {cells[0]}, {cells[1]} for ");

        var n = 0;
        _pos.Next(ref n);
        var f = n;
        _pos.Next(ref n);
        builder.Append(f + ", " + n);

        return builder.ToString();
    }
    
    public Clue<ISudokuHighlighter> BuildClue(IReadOnlyList<NumericChange> changes, ISudokuSolvingState snapshot)
    {
        return Clue<ISudokuHighlighter>.Default();
    }
}