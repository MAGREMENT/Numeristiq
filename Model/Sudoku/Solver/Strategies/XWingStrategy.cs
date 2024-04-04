using System.Collections.Generic;
using System.Text;
using Model.Helpers;
using Model.Helpers.Changes;
using Model.Helpers.Highlighting;
using Model.Sudoku.Solver.Explanation;
using Model.Sudoku.Solver.Position;
using Model.Sudoku.Solver.StrategiesUtility;
using Model.Utility;

namespace Model.Sudoku.Solver.Strategies;

public class XWingStrategy : SudokuStrategy
{
    public const string OfficialName = "X-Wing";
    private const InstanceHandling DefaultInstanceHandling = InstanceHandling.UnorderedAll;

    public XWingStrategy() : base(OfficialName, StrategyDifficulty.Medium, DefaultInstanceHandling){}

    public override void Apply(IStrategyUser strategyUser)
    {
        Dictionary<IReadOnlyLinePositions, int> dict = new();
        for (int n = 1; n <= 9; n++)
        {
            //Rows
            for (int row = 0; row < 9; row++)
            {
                var ppir = strategyUser.RowPositionsAt(row, n);
                if (ppir.Count != 2) continue;
                
                if (!dict.TryAdd(ppir, row))
                {
                    if (RemoveFromColumns(strategyUser, ppir, dict[ppir], row, n)) return;
                }
            }
            dict.Clear();
            
            //Columns
            for (int col = 0; col < 9; col++)
            {
                var ppic = strategyUser.ColumnPositionsAt(col, n);
                if (ppic.Count != 2) continue;
                
                if (!dict.TryAdd(ppic, col))
                {
                    if (RemoveFromRows(strategyUser, ppic, dict[ppic], col, n)) return;
                }
            }
            dict.Clear();
        }
    }

    private bool RemoveFromColumns(IStrategyUser strategyUser, IReadOnlyLinePositions cols, int row1, int row2, int number)
    {
        for (int row = 0; row < 9; row++)
        {
            if (row == row1 || row == row2) continue;
            
            foreach (var col in cols)
            {
                strategyUser.ChangeBuffer.ProposePossibilityRemoval(number, row, col);
            }
        }
        
        return strategyUser.ChangeBuffer.Commit( new XWingReportBuilder(cols, row1, row2, number, Unit.Row))
            && StopOnFirstPush;
    }

    private bool RemoveFromRows(IStrategyUser strategyUser, IReadOnlyLinePositions rows, int col1, int col2, int number)
    {
        for (int col = 0; col < 9; col++)
        {
            if (col == col1 || col == col2) continue;
            
            foreach (var row in rows)
            {
                strategyUser.ChangeBuffer.ProposePossibilityRemoval(number, row, col);
            }
        }
        
        return strategyUser.ChangeBuffer.Commit( new XWingReportBuilder(rows, col1, col2, number, Unit.Column))
            && StopOnFirstPush;
    }
}

public class XWingReportBuilder : IChangeReportBuilder<IUpdatableSudokuSolvingState, ISudokuHighlighter>
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
    
    public ChangeReport<ISudokuHighlighter> BuildReport(IReadOnlyList<SolverProgress> changes, IUpdatableSudokuSolvingState snapshot)
    {
        return new ChangeReport<ISudokuHighlighter>(Description(), lighter =>
        {
            foreach (var other in _linePos)
            {
                switch (_unit)
                {
                    case Unit.Row :
                        lighter.HighlightPossibility(_number, _unit1, other, ChangeColoration.CauseOffOne);
                        lighter.HighlightPossibility(_number, _unit2, other, ChangeColoration.CauseOffOne);
                        break;
                    case Unit.Column :
                        lighter.HighlightPossibility(_number, other, _unit1, ChangeColoration.CauseOffOne);
                        lighter.HighlightPossibility(_number, other, _unit2, ChangeColoration.CauseOffOne);
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

    private ExplanationElement Explanation()
    {
        var start = new StringExplanationElement("In ");
        var c = start + new House(_unit, _unit1) + " and " + new House(_unit, _unit2) +
                $", {_number} is only present in ";
        var u = _unit == Unit.Row ? Unit.Column : Unit.Row;
        var i = -1;
        _linePos.Next(ref i);
        var u1 = i;
        _linePos.Next(ref i);
        var u2 = i;

        var ch1 = new House(u, u1);
        var ch2 = new House(u, u2);

        _ = c + ch1 + " and " + ch2 + ". This means that if " + new Cell(_unit1, u1) + $" hold {_number}, "
            + new Cell(_unit2, u2) + $" must also hold {_number}. The same can be said for " +
            new Cell(_unit1, u2) + " and " + new Cell(_unit2, u1) + $". We can then remove every {_number} in "
            + ch1 + " and " + ch2 + " that is not part of the X-Wing.";

        return start;
    }
    
    public Clue<ISudokuHighlighter> BuildClue(IReadOnlyList<SolverProgress> changes, IUpdatableSudokuSolvingState snapshot)
    {
        return Clue<ISudokuHighlighter>.Default();
    }
}