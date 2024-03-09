using System.Collections.Generic;
using Model.Helpers;
using Model.Helpers.Changes;
using Model.Helpers.Settings;
using Model.Helpers.Settings.Types;
using Model.Sudoku.Solver.StrategiesUtility;

namespace Model.Sudoku.Solver.Strategies;

public class BUGStrategy : SudokuStrategy
{
    public const string OfficialName = "BUG";
    private const OnCommitBehavior DefaultBehavior = OnCommitBehavior.Return;
    
    public override OnCommitBehavior DefaultOnCommitBehavior => DefaultBehavior;

    private readonly IntSetting _maxAdditionalCandidates;
    
    public BUGStrategy(int maxAdditionalCandidates) : base(OfficialName, StrategyDifficulty.Medium, DefaultBehavior)
    {
        _maxAdditionalCandidates = new IntSetting("Max additional candidates", new SliderInteractionInterface(1, 5, 1), maxAdditionalCandidates);
        AddSetting(_maxAdditionalCandidates);
        
        UniquenessDependency = UniquenessDependency.FullyDependent;
    }
    
    public override void Apply(IStrategyUser strategyUser)
    {
        List<CellPossibility> additionalCandidates = new(_maxAdditionalCandidates.Value);
        for (int number = 1; number <= 9; number++)
        {
            var pos = strategyUser.PositionsFor(number);
            if (pos.Count == 0) continue;

            var copy = pos.Copy();
            for (int i = 0; i < 9; i++)
            {
                foreach (var method in UnitMethods.All)
                {
                    if (method.Count(pos, i) == 2) method.Void(copy, i);
                }
            }

            if (copy.Count + additionalCandidates.Count > _maxAdditionalCandidates.Value) return;
            foreach (var cell in copy)
            {
                additionalCandidates.Add(new CellPossibility(cell, number));
            }
        }

        switch (additionalCandidates.Count)
        {
            case 0 : return;
            case 1 : 
                strategyUser.ChangeBuffer.ProposeSolutionAddition(additionalCandidates[0]);
                break;
            default:
                foreach (var cp in Cells.SharedSeenExistingPossibilities(strategyUser, additionalCandidates))
                {
                    strategyUser.ChangeBuffer.ProposePossibilityRemoval(cp);
                }

                break;
        }

        
        if(strategyUser.ChangeBuffer.NotEmpty()) strategyUser.ChangeBuffer.Commit(
            new BUGStrategyReportBuilder(additionalCandidates));
    }
}

public class BUGStrategyReportBuilder : IChangeReportBuilder<IUpdatableSudokuSolvingState>
{
    private readonly List<CellPossibility> _additionalCandidates;

    public BUGStrategyReportBuilder(List<CellPossibility> additionalCandidates)
    {
        _additionalCandidates = additionalCandidates;
    }

    public ChangeReport Build(IReadOnlyList<SolverProgress> changes, IUpdatableSudokuSolvingState snapshot)
    {
        return new ChangeReport( "", lighter =>
        {
            foreach (var cp in _additionalCandidates)
            {
                lighter.HighlightPossibility(cp, ChangeColoration.CauseOnOne);
            }

            ChangeReportHelper.HighlightChanges(lighter, changes);
        });
    }
}