using System.Collections.Generic;
using Model.Core;
using Model.Core.Changes;
using Model.Core.Explanation;
using Model.Core.Highlighting;
using Model.Utility;
using Model.Utility.BitSets;

namespace Model.Sudokus.Solver.Strategies;

public class NakedSingleStrategy : SudokuStrategy
{
    public const string OfficialName = "Naked Single";
    private const InstanceHandling DefaultInstanceHandling = InstanceHandling.UnorderedAll;

    public NakedSingleStrategy() : base(OfficialName, Difficulty.Basic, DefaultInstanceHandling) {}
    
    public override void Apply(ISudokuSolverData solverData)
    {
        for (int row = 0; row < 9; row++)
        {
            for (int col = 0; col < 9; col++)
            {
                if (solverData.PossibilitiesAt(row, col).Count != 1) continue;
                
                solverData.ChangeBuffer.ProposeSolutionAddition(solverData.PossibilitiesAt(row, col).FirstPossibility(), row, col);
                if (solverData.ChangeBuffer.NeedCommit())
                {
                    solverData.ChangeBuffer.Commit( new NakedSingleReportBuilder());
                    if (StopOnFirstCommit) return;
                }
            }
        }
    }
}

public class NakedSingleReportBuilder : IChangeReportBuilder<NumericChange, ISudokuSolvingState, ISudokuHighlighter>
{
    public ChangeReport<ISudokuHighlighter> BuildReport(IReadOnlyList<NumericChange> changes, ISudokuSolvingState snapshot)
    {
        return new ChangeReport<ISudokuHighlighter>(Description(changes),
            lighter => ChangeReportHelper.HighlightChanges(lighter, changes), Explanation(changes));
    }

    private static string Description(IReadOnlyList<NumericChange> changes)
    {
        return changes.Count != 1 ? "" : $"Naked Single in r{changes[0].Row + 1}c{changes[0].Column + 1}";
    }

    private static ExplanationElement? Explanation(IReadOnlyList<NumericChange> changes)
    {
        if (changes.Count != 1) return null;

        var change = changes[0];
        var start = new StringExplanationElement($"{change.Number} is the only possibility for ");
        _ = start + new Cell(change.Row, change.Column) + ". It is therefore the solution for that cell.";

        return start;
    }
    
    public Clue<ISudokuHighlighter> BuildClue(IReadOnlyList<NumericChange> changes, ISudokuSolvingState snapshot)
    {
        if(changes.Count == 0) return Clue<ISudokuHighlighter>.Default();
        return new Clue<ISudokuHighlighter>(lighter =>
            {
                lighter.EncircleCell(changes[0].Row, changes[0].Column);
            }, "Look at the possibilities for that cell");
    }
}