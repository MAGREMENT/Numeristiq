using System.Collections.Generic;
using Global;
using Global.Enums;
using Model.Solver.Helpers.Changes;
using Model.Solver.Position;
using Model.Solver.StrategiesUtility;

namespace Model.Solver.Strategies;

public class BUGStrategy : AbstractStrategy
{
    public const string OfficialName = "BUG";
    private const OnCommitBehavior DefaultBehavior = OnCommitBehavior.Return;
    
    public override OnCommitBehavior DefaultOnCommitBehavior => DefaultBehavior;

    private int _maxAdditionalCandidates;
    
    public BUGStrategy(int maxAdditionalCandidates) : base(OfficialName, StrategyDifficulty.Medium, DefaultBehavior)
    {
        _maxAdditionalCandidates = maxAdditionalCandidates;
        UniquenessDependency = UniquenessDependency.FullyDependent;
        ArgumentsList.Add(new IntStrategyArgument("Max additional candidates", () => _maxAdditionalCandidates,
            i => _maxAdditionalCandidates = i, new SliderViewInterface(1, 5, 1)));
    }
    
    public override void Apply(IStrategyManager strategyManager)
    {
        List<CellPossibility> additionalCandidates = new(_maxAdditionalCandidates);
        for (int number = 1; number <= 9; number++)
        {
            var pos = strategyManager.PositionsFor(number);
            if (pos.Count == 0) continue;

            var copy = pos.Copy();
            for (int i = 0; i < 9; i++)
            {
                foreach (var method in UnitMethods.All)
                {
                    if (method.Count(pos, i) == 2) method.Void(copy, i);
                }
            }

            if (copy.Count + additionalCandidates.Count > _maxAdditionalCandidates) return;
            foreach (var cell in copy)
            {
                additionalCandidates.Add(new CellPossibility(cell, number));
            }
        }

        switch (additionalCandidates.Count)
        {
            case 0 : return;
            case 1 : 
                strategyManager.ChangeBuffer.ProposeSolutionAddition(additionalCandidates[0]);
                break;
            default:
                foreach (var cp in Cells.SharedSeenExistingPossibilities(strategyManager, additionalCandidates))
                {
                    strategyManager.ChangeBuffer.ProposePossibilityRemoval(cp);
                }

                break;
        }

        
        if(strategyManager.ChangeBuffer.NotEmpty()) strategyManager.ChangeBuffer.Commit(this,
            new BUGStrategyReportBuilder(additionalCandidates));
    }
}

public class BUGStrategyReportBuilder : IChangeReportBuilder
{
    private readonly List<CellPossibility> _additionalCandidates;

    public BUGStrategyReportBuilder(List<CellPossibility> additionalCandidates)
    {
        _additionalCandidates = additionalCandidates;
    }

    public ChangeReport Build(List<SolverChange> changes, IPossibilitiesHolder snapshot)
    {
        return new ChangeReport(IChangeReportBuilder.ChangesToString(changes), "", lighter =>
        {
            foreach (var cp in _additionalCandidates)
            {
                lighter.HighlightPossibility(cp, ChangeColoration.CauseOnOne);
            }

            IChangeReportBuilder.HighlightChanges(lighter, changes);
        });
    }
}