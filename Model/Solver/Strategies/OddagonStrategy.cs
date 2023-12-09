using System.Collections.Generic;
using Global.Enums;
using Model.Solver.Helpers.Changes;
using Model.Solver.StrategiesUtility.Graphs;
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
        strategyManager.GraphManager.ConstructSimple(ConstructRule.CellStrongLink, ConstructRule.CellWeakLink,
            ConstructRule.UnitStrongLink, ConstructRule.UnitWeakLink);
        var graph = strategyManager.GraphManager.SimpleLinkGraph;
        
        foreach (var ao in strategyManager.PreComputer.AlmostOddagons())
        {
            if (ao.Guardians.Length == 1) strategyManager.ChangeBuffer.ProposeSolutionAddition(ao.Guardians[0]);
            else
            {
                foreach (var link in graph.GetLinks(ao.Guardians[0]))
                {
                    bool ok = true;

                    for (int i = 0; i < ao.Guardians.Length; i++)
                    {
                        if (!graph.HasLinkTo(link, ao.Guardians[i]))
                        {
                            ok = false;
                            break;
                        }
                    }
                    
                    if (ok) strategyManager.ChangeBuffer.ProposePossibilityRemoval(link);
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
                lighter.HighlightPossibility(element, ChangeColoration.CauseOnOne);
            }
            
            _oddagon.Loop.ForEachLink((one, two) => lighter.CreateLink(one, two, LinkStrength.Strong));

            foreach (var cp in _oddagon.Guardians)
            {
                lighter.EncirclePossibility(cp);
            }

            IChangeReportBuilder.HighlightChanges(lighter, changes);
        });
    }
}