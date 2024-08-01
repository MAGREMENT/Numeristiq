using System.Collections.Generic;
using Model.Core;
using Model.Core.Changes;
using Model.Core.Highlighting;
using Model.Sudokus.Solver.Utility;
using Model.Sudokus.Solver.Utility.Graphs;
using Model.Sudokus.Solver.Utility.Oddagons;

namespace Model.Sudokus.Solver.Strategies;

public class OddagonStrategy : SudokuStrategy
{
    public const string OfficialName = "Oddagon";
    private const InstanceHandling DefaultInstanceHandling = InstanceHandling.FirstOnly;

    public OddagonStrategy() : base(OfficialName, Difficulty.Extreme, DefaultInstanceHandling)
    {
    }
    
    public override void Apply(ISudokuSolverData solverData)
    {
        foreach (var ao in solverData.PreComputer.AlmostOddagons())
        {
            if (ao.Guardians.Length == 1) solverData.ChangeBuffer.ProposeSolutionAddition(ao.Guardians[0]);
            else
            {
                foreach (var cp in SudokuCellUtility.SharedSeenExistingPossibilities(solverData, ao.Guardians))
                {
                    solverData.ChangeBuffer.ProposePossibilityRemoval(cp);
                }
            }

            if (solverData.ChangeBuffer.NotEmpty() && solverData.ChangeBuffer.Commit(
                    new OddagonReportBuilder(ao)) && StopOnFirstPush) return;
        }
    }
}

public class OddagonReportBuilder : IChangeReportBuilder<NumericChange, ISudokuSolvingState, ISudokuHighlighter>
{
    private readonly AlmostOddagon _oddagon;

    public OddagonReportBuilder(AlmostOddagon oddagon)
    {
        _oddagon = oddagon;
    }

    public ChangeReport<ISudokuHighlighter> BuildReport(IReadOnlyList<NumericChange> changes, ISudokuSolvingState snapshot)
    {
        return new ChangeReport<ISudokuHighlighter>( "", lighter =>
        {
            foreach (var element in _oddagon.Loop.Elements)
            {
                lighter.HighlightPossibility(element, StepColor.Cause2);
            }
            
            _oddagon.Loop.ForEachLink((one, two)
                => lighter.CreateLink(one, two, LinkStrength.Strong), LinkStrength.Strong);
            _oddagon.Loop.ForEachLink((one, two)
                => lighter.CreateLink(one, two, LinkStrength.Weak), LinkStrength.Weak);

            foreach (var cp in _oddagon.Guardians)
            {
                lighter.EncirclePossibility(cp);
                lighter.HighlightPossibility(cp, StepColor.On);
            }

            ChangeReportHelper.HighlightChanges(lighter, changes);
        });
    }
    
    public Clue<ISudokuHighlighter> BuildClue(IReadOnlyList<NumericChange> changes, ISudokuSolvingState snapshot)
    {
        return Clue<ISudokuHighlighter>.Default();
    }
}