using System.Collections.Generic;
using Global.Enums;
using Model.Solver.Helpers.Changes;
using Model.Solver.StrategiesUtility;
using Model.Solver.StrategiesUtility.Oddagons;

namespace Model.Solver.Strategies;

public class OddagonStrategy : AbstractStrategy
{
    public const string OfficialName = "Oddagon";
    private const OnCommitBehavior DefaultBehavior = OnCommitBehavior.Return;

    public override OnCommitBehavior DefaultOnCommitBehavior => DefaultBehavior;
    
    public OddagonStrategy() : base(OfficialName, StrategyDifficulty.Extreme, DefaultBehavior)
    {
    }
    
    public override void Apply(IStrategyManager strategyManager)
    {
        foreach (var ao in strategyManager.PreComputer.AlmostOddagons())
        {
            if (ao.Guardians.Length == 1) strategyManager.ChangeBuffer.ProposeSolutionAddition(ao.Guardians[0]);
            else
            {
                foreach (var cp in Cells.SharedSeenExistingPossibilities(strategyManager, ao.Guardians))
                {
                    strategyManager.ChangeBuffer.ProposePossibilityRemoval(cp);
                }
            }

            if (strategyManager.ChangeBuffer.NotEmpty() && strategyManager.ChangeBuffer.Commit(this,
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

    public ChangeReport Build(List<SolverChange> changes, IPossibilitiesHolder snapshot)
    {
        return new ChangeReport(IChangeReportBuilder.ChangesToString(changes), "", lighter =>
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