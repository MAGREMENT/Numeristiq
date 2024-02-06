using System.Collections.Generic;
using Model.Helpers.Changes;
using Model.Sudoku.Solver.StrategiesUtility;
using Model.Sudoku.Solver.StrategiesUtility.Graphs;
using Model.Sudoku.Solver.StrategiesUtility.Oddagons;

namespace Model.Sudoku.Solver.Strategies;

public class OddagonStrategy : AbstractSudokuStrategy
{
    public const string OfficialName = "Oddagon";
    private const OnCommitBehavior DefaultBehavior = OnCommitBehavior.Return;

    public override OnCommitBehavior DefaultOnCommitBehavior => DefaultBehavior;
    
    public OddagonStrategy() : base(OfficialName, StrategyDifficulty.Extreme, DefaultBehavior)
    {
    }
    
    public override void Apply(IStrategyUser strategyUser)
    {
        foreach (var ao in strategyUser.PreComputer.AlmostOddagons())
        {
            if (ao.Guardians.Length == 1) strategyUser.ChangeBuffer.ProposeSolutionAddition(ao.Guardians[0]);
            else
            {
                foreach (var cp in Cells.SharedSeenExistingPossibilities(strategyUser, ao.Guardians))
                {
                    strategyUser.ChangeBuffer.ProposePossibilityRemoval(cp);
                }
            }

            if (strategyUser.ChangeBuffer.NotEmpty() && strategyUser.ChangeBuffer.Commit(
                    new OddagonReportBuilder(ao)) && OnCommitBehavior == OnCommitBehavior.Return) return;
        }
    }
}

public class OddagonReportBuilder : IChangeReportBuilder
{
    private readonly AlmostOddagon _oddagon;

    public OddagonReportBuilder(AlmostOddagon oddagon)
    {
        _oddagon = oddagon;
    }

    public ChangeReport Build(IReadOnlyList<SolverChange> changes, IPossibilitiesHolder snapshot)
    {
        return new ChangeReport( "", lighter =>
        {
            foreach (var element in _oddagon.Loop.Elements)
            {
                lighter.HighlightPossibility(element, ChangeColoration.CauseOffTwo);
            }
            
            _oddagon.Loop.ForEachLink((one, two)
                => lighter.CreateLink(one, two, LinkStrength.Strong), LinkStrength.Strong);
            _oddagon.Loop.ForEachLink((one, two)
                => lighter.CreateLink(one, two, LinkStrength.Weak), LinkStrength.Weak);

            foreach (var cp in _oddagon.Guardians)
            {
                lighter.EncirclePossibility(cp);
                lighter.HighlightPossibility(cp, ChangeColoration.CauseOnOne);
            }

            IChangeReportBuilder.HighlightChanges(lighter, changes);
        });
    }
}