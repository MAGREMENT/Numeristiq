using System.Collections.Generic;
using System.Text;
using Model.Core;
using Model.Core.Changes;
using Model.Core.Explanations;
using Model.Core.Highlighting;
using Model.Sudokus.Solver.Position;
using Model.Sudokus.Solver.Utility;
using Model.Utility;

namespace Model.Sudokus.Solver.Strategies;

public class XWingStrategy : SudokuStrategy
{
    public const string OfficialName = "X-Wing";
    private const InstanceHandling DefaultInstanceHandling = InstanceHandling.UnorderedAll;

    public XWingStrategy() : base(OfficialName, Difficulty.Medium, DefaultInstanceHandling){}

    public override void Apply(ISudokuSolverData solverData)
    {
        Dictionary<IReadOnlyLinePositions, int> dict = new();
        for (int n = 1; n <= 9; n++)
        {
            //Rows
            for (int row = 0; row < 9; row++)
            {
                var ppir = solverData.RowPositionsAt(row, n);
                if (ppir.Count != 2) continue;
                
                if (!dict.TryAdd(ppir, row))
                {
                    if (RemoveFromColumns(solverData, ppir, dict[ppir], row, n)) return;
                }
            }
            dict.Clear();
            
            //Columns
            for (int col = 0; col < 9; col++)
            {
                var ppic = solverData.ColumnPositionsAt(col, n);
                if (ppic.Count != 2) continue;
                
                if (!dict.TryAdd(ppic, col))
                {
                    if (RemoveFromRows(solverData, ppic, dict[ppic], col, n)) return;
                }
            }
            dict.Clear();
        }
    }

    private bool RemoveFromColumns(ISudokuSolverData solverData, IReadOnlyLinePositions cols, int row1, int row2, int number)
    {
        for (int row = 0; row < 9; row++)
        {
            if (row == row1 || row == row2) continue;
            
            foreach (var col in cols)
            {
                solverData.ChangeBuffer.ProposePossibilityRemoval(number, row, col);
            }
        }

        if (!solverData.ChangeBuffer.NeedCommit()) return false;
        solverData.ChangeBuffer.Commit(new XWingReportBuilder(cols, row1, row2, number, Unit.Row));
        return StopOnFirstCommit;
    }

    private bool RemoveFromRows(ISudokuSolverData solverData, IReadOnlyLinePositions rows, int col1, int col2, int number)
    {
        for (int col = 0; col < 9; col++)
        {
            if (col == col1 || col == col2) continue;
            
            foreach (var row in rows)
            {
                solverData.ChangeBuffer.ProposePossibilityRemoval(number, row, col);
            }
        }
        
        if (!solverData.ChangeBuffer.NeedCommit()) return false;
        solverData.ChangeBuffer.Commit(new XWingReportBuilder(rows, col1, col2, number, Unit.Column));
        return StopOnFirstCommit;
    }
}

public class XWingReportBuilder : IChangeReportBuilder<NumericChange, ISudokuSolvingState, ISudokuHighlighter>
{
    private readonly IReadOnlyLinePositions _linePos;
    private readonly int _unit1;
    private readonly int _unit2;
    private readonly int _number;
    private readonly Unit _unit;

    public XWingReportBuilder(IReadOnlyLinePositions linePos, int unit1, int unit2, int number, Unit unit)
    {
        _linePos = linePos;
        _unit1 = unit1;
        _unit2 = unit2;
        _number = number;
        _unit = unit;
    }
    
    public ChangeReport<ISudokuHighlighter> BuildReport(IReadOnlyList<NumericChange> changes, ISudokuSolvingState snapshot)
    {
        return new ChangeReport<ISudokuHighlighter>(Description(), lighter =>
        {
            foreach (var other in _linePos)
            {
                switch (_unit)
                {
                    case Unit.Row :
                        lighter.HighlightPossibility(_number, _unit1, other, StepColor.Cause1);
                        lighter.HighlightPossibility(_number, _unit2, other, StepColor.Cause1);
                        break;
                    case Unit.Column :
                        lighter.HighlightPossibility(_number, other, _unit1, StepColor.Cause1);
                        lighter.HighlightPossibility(_number, other, _unit2, StepColor.Cause1);
                        break;
                }
            }

            ChangeReportHelper.HighlightChanges(lighter, changes);
        }, Explanation());
    }

    private string Description()
    {
        return $"XWing in {GetLineAsString(_unit, _unit1, _linePos)}, {GetLineAsString(_unit, _unit2, _linePos)} for {_number}";
    }

    private string GetLineAsString(Unit unit, int number, IReadOnlyLinePositions linePos)
    {
        var builder = new StringBuilder();
        if (unit == Unit.Row)
        {
            builder.Append($"r{number + 1}c");
            foreach (var col in linePos) builder.Append(col + 1);
        }
        else
        {
            builder.Append('r');
            foreach (var row in linePos) builder.Append(row + 1);
            builder.Append($"c{number + 1}");
        }

        return builder.ToString();
    }

    private Explanation<ISudokuHighlighter> Explanation()
    {
        var result = new Explanation<ISudokuHighlighter>().Append("In ")
            .Append(new House(_unit, _unit1)).Append(" and ").Append(new House(_unit, _unit2))
            .Append($", {_number} is only present in ");
        
        var u = _unit == Unit.Row ? Unit.Column : Unit.Row;
        var i = -1;
        _linePos.Next(ref i);
        var u1 = i;
        _linePos.Next(ref i);
        var u2 = i;

        var ch1 = new House(u, u1);
        var ch2 = new House(u, u2);

        return result.Append(ch1).Append(" and ").Append(ch2).Append(". This means that if ")
            .Append(new Cell(_unit1, u1)).Append($" hold {_number}, ")
            .Append(new Cell(_unit2, u2)).Append($" must also hold {_number}. The same can be said for ")
            .Append(new Cell(_unit1, u2)).Append(" and ").Append(new Cell(_unit2, u1))
            .Append($". We can then remove every {_number} in ").Append(ch1)
            .Append(" and ").Append(ch2).Append(" that is not part of the X-Wing.");
    }
    
    public Clue<ISudokuHighlighter> BuildClue(IReadOnlyList<NumericChange> changes, ISudokuSolvingState snapshot)
    {
        return Clue<ISudokuHighlighter>.Default();
    }
}