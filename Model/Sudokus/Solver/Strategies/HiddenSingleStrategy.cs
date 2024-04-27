using System;
using System.Collections.Generic;
using Model.Helpers;
using Model.Helpers.Changes;
using Model.Helpers.Highlighting;
using Model.Sudokus.Solver.Explanation;
using Model.Sudokus.Solver.Utility;
using Model.Utility;

namespace Model.Sudokus.Solver.Strategies;

public class HiddenSingleStrategy : SudokuStrategy
{
    public const string OfficialName = "Hidden Single";
    private const InstanceHandling DefaultInstanceHandling = InstanceHandling.UnorderedAll;

    public HiddenSingleStrategy() : base(OfficialName, StrategyDifficulty.Basic, DefaultInstanceHandling){}
    
    public override void Apply(ISudokuStrategyUser strategyUser)
    {
        for (int number = 1; number <= 9; number++)
        {
            for (int i = 0; i < 3; i++)
            {
                for (int j = 0; j < 3; j++)
                {
                    var u = i * 3 + j;
                    
                    var rp = strategyUser.RowPositionsAt(u, number);
                    if (rp.Count == 1)
                    {
                        strategyUser.ChangeBuffer.ProposeSolutionAddition(number, u, rp.First());
                        strategyUser.ChangeBuffer.Commit(new HiddenSingleReportBuilder(Unit.Row));
                        if (StopOnFirstPush) return;
                    }
                    
                    var cp = strategyUser.ColumnPositionsAt(u, number);
                    if (cp.Count == 1)
                    {
                        strategyUser.ChangeBuffer.ProposeSolutionAddition(number, cp.First(), u);
                        strategyUser.ChangeBuffer.Commit(new HiddenSingleReportBuilder(Unit.Column));
                        if (StopOnFirstPush) return;
                    }
                    
                    var mp = strategyUser.MiniGridPositionsAt(i, j, number);
                    if (mp.Count != 1) continue;
                    
                    var pos = mp.First();
                    strategyUser.ChangeBuffer.ProposeSolutionAddition(number, pos.Row, pos.Column);
                    strategyUser.ChangeBuffer.Commit(new HiddenSingleReportBuilder(Unit.MiniGrid));
                    if (StopOnFirstPush) return;
                }
            }
        }
    }
}

public class HiddenSingleReportBuilder : IChangeReportBuilder<IUpdatableSudokuSolvingState, ISudokuHighlighter>
{
    private readonly Unit _unit;

    public HiddenSingleReportBuilder(Unit unit)
    {
        _unit = unit;
    }

    public ChangeReport<ISudokuHighlighter> BuildReport(IReadOnlyList<SolverProgress> changes, IUpdatableSudokuSolvingState snapshot)
    {
        return new ChangeReport<ISudokuHighlighter>( Description(changes),
            lighter => ChangeReportHelper.HighlightChanges(lighter, changes), Explanation(changes));
    }

    private static string Description(IReadOnlyList<SolverProgress> changes)
    {
        if (changes.Count != 1) return "";

        return $"Hidden Single in r{changes[0].Row + 1}c{changes[0].Column + 1}";
    }

    private ExplanationElement? Explanation(IReadOnlyList<SolverProgress> changes)
    {
        if (changes.Count != 1) return null;

        var cell = new Cell(changes[0].Row, changes[0].Column);
        var ch = UnitMethods.Get(_unit).ToCoverHouse(cell);
        
        var start = new StringExplanationElement(changes[0].Number + " is only present in ");
        _ = start + cell + " in " + ch + ". It is therefore the solution for this cell.";

        return start;
    }
    
    public Clue<ISudokuHighlighter> BuildClue(IReadOnlyList<SolverProgress> changes, IUpdatableSudokuSolvingState snapshot)
    {
        if(changes.Count == 0) return Clue<ISudokuHighlighter>.Default();

        var change = changes[0];
        var house = _unit switch
        {
            Unit.Row => new House(_unit, change.Row),
            Unit.Column => new House(_unit, change.Column),
            Unit.MiniGrid => new House(_unit, change.Row * 3 + change.Column),
            _ => throw new ArgumentOutOfRangeException(nameof(_unit))
        };

        return new Clue<ISudokuHighlighter>(lighter =>
        {
            lighter.EncircleHouse(house, ChangeColoration.CauseOffOne);
        }, "Look at the possibilities in that house");
    }
}