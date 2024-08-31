using System;
using System.Collections.Generic;
using System.Linq;
using Model.Core;
using Model.Core.Changes;
using Model.Core.Explanations;
using Model.Core.Highlighting;
using Model.Sudokus.Solver.Utility;
using Model.Utility;

namespace Model.Sudokus.Solver.Strategies;

public class HiddenSingleStrategy : SudokuStrategy
{
    public const string OfficialName = "Hidden Single";
    private const InstanceHandling DefaultInstanceHandling = InstanceHandling.UnorderedAll;

    public HiddenSingleStrategy() : base(OfficialName, Difficulty.Basic, DefaultInstanceHandling){}
    
    public override void Apply(ISudokuSolverData solverData)
    {
        for (int number = 1; number <= 9; number++)
        {
            for (int i = 0; i < 3; i++)
            {
                for (int j = 0; j < 3; j++)
                {
                    var u = i * 3 + j;
                    
                    var rp = solverData.RowPositionsAt(u, number);
                    if (rp.Count == 1)
                    {
                        solverData.ChangeBuffer.ProposeSolutionAddition(number, u, rp.First());
                        if (solverData.ChangeBuffer.NeedCommit())
                        {
                            solverData.ChangeBuffer.Commit(new HiddenSingleReportBuilder(Unit.Row));
                            if (StopOnFirstCommit) return;
                        }
                    }
                    
                    var cp = solverData.ColumnPositionsAt(u, number);
                    if (cp.Count == 1)
                    {
                        solverData.ChangeBuffer.ProposeSolutionAddition(number, cp.First(), u);
                        if (solverData.ChangeBuffer.NeedCommit())
                        {
                            solverData.ChangeBuffer.Commit(new HiddenSingleReportBuilder(Unit.Column));
                            if (StopOnFirstCommit) return;
                        }
                    }
                    
                    var mp = solverData.MiniGridPositionsAt(i, j, number);
                    if (mp.Count != 1) continue;
                    
                    var pos = mp.First();
                    solverData.ChangeBuffer.ProposeSolutionAddition(number, pos.Row, pos.Column);
                    if (solverData.ChangeBuffer.NeedCommit())
                    {
                        solverData.ChangeBuffer.Commit(new HiddenSingleReportBuilder(Unit.Box));
                        if (StopOnFirstCommit) return;
                    }
                }
            }
        }
    }
}

public class HiddenSingleReportBuilder : IChangeReportBuilder<NumericChange, ISudokuSolvingState, ISudokuHighlighter>
{
    private readonly Unit _unit;

    public HiddenSingleReportBuilder(Unit unit)
    {
        _unit = unit;
    }

    public ChangeReport<ISudokuHighlighter> BuildReport(IReadOnlyList<NumericChange> changes, ISudokuSolvingState snapshot)
    {
        return new ChangeReport<ISudokuHighlighter>( Description(changes),
            lighter => ChangeReportHelper.HighlightChanges(lighter, changes), Explanation(changes));
    }

    private static string Description(IReadOnlyList<NumericChange> changes)
    {
        if (changes.Count != 1) return "";

        return $"Hidden Single in r{changes[0].Row + 1}c{changes[0].Column + 1}";
    }

    private Explanation<ISudokuHighlighter> Explanation(IReadOnlyList<NumericChange> changes)
    {
        if (changes.Count != 1) return Explanation<ISudokuHighlighter>.Empty;

        var cell = new Cell(changes[0].Row, changes[0].Column);
        var ch = UnitMethods.Get(_unit).ToCoverHouse(cell);
        
        return new Explanation<ISudokuHighlighter>().Append(changes[0].Number + " is only present in ")
            .Append(cell).Append(" in ").Append(ch).Append(". It is therefore the solution for this cell.");
    }
    
    public Clue<ISudokuHighlighter> BuildClue(IReadOnlyList<NumericChange> changes, ISudokuSolvingState snapshot)
    {
        if(changes.Count == 0) return Clue<ISudokuHighlighter>.Default();

        var change = changes[0];
        var house = _unit switch
        {
            Unit.Row => new House(_unit, change.Row),
            Unit.Column => new House(_unit, change.Column),
            Unit.Box => new House(_unit, change.Row / 3 * 3 + change.Column / 3),
            _ => throw new ArgumentOutOfRangeException(nameof(_unit))
        };

        return new Clue<ISudokuHighlighter>(lighter =>
        {
            lighter.EncircleHouse(house, StepColor.Cause1);
        }, "Look at the possibilities in that house");
    }
}