using System.Collections.Generic;
using Model.Core;
using Model.Core.Changes;
using Model.Core.Highlighting;
using Model.Core.Settings;
using Model.Core.Settings.Types;
using Model.Sudokus.Solver.Utility;
using Model.Utility;

namespace Model.Sudokus.Solver.Strategies;

public class BUGStrategy : SudokuStrategy
{
    public const string OfficialName = "BUG";
    private const InstanceHandling DefaultInstanceHandling = InstanceHandling.FirstOnly;

    private readonly IntSetting _maxAdditionalCandidates;
    
    public BUGStrategy(int maxAdditionalCandidates) : base(OfficialName, StepDifficulty.Medium, DefaultInstanceHandling)
    {
        _maxAdditionalCandidates = new IntSetting("Max additional candidates", new SliderInteractionInterface(1, 5, 1), maxAdditionalCandidates);
        AddSetting(_maxAdditionalCandidates);
        
        UniquenessDependency = UniquenessDependency.FullyDependent;
    }
    
    public override void Apply(ISudokuSolverData solverData)
    {
        List<CellPossibility> additionalCandidates = new(_maxAdditionalCandidates.Value);
        for (int number = 1; number <= 9; number++)
        {
            var pos = solverData.PositionsFor(number);
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
                solverData.ChangeBuffer.ProposeSolutionAddition(additionalCandidates[0]);
                break;
            default:
                foreach (var cp in SudokuCellUtility.SharedSeenExistingPossibilities(solverData, additionalCandidates))
                {
                    solverData.ChangeBuffer.ProposePossibilityRemoval(cp);
                }

                break;
        }

        
        if(solverData.ChangeBuffer.NotEmpty()) solverData.ChangeBuffer.Commit(
            new BUGStrategyReportBuilder(additionalCandidates));
    }
}

public class BUGStrategyReportBuilder : IChangeReportBuilder<IUpdatableSudokuSolvingState, ISudokuHighlighter>
{
    private readonly List<CellPossibility> _additionalCandidates;

    public BUGStrategyReportBuilder(List<CellPossibility> additionalCandidates)
    {
        _additionalCandidates = additionalCandidates;
    }

    public ChangeReport<ISudokuHighlighter> BuildReport(IReadOnlyList<SolverProgress> changes, IUpdatableSudokuSolvingState snapshot)
    {
        return new ChangeReport<ISudokuHighlighter>( "", lighter =>
        {
            foreach (var cp in _additionalCandidates)
            {
                lighter.HighlightPossibility(cp, ChangeColoration.CauseOnOne);
            }

            ChangeReportHelper.HighlightChanges(lighter, changes);
        });
    }
    
    public Clue<ISudokuHighlighter> BuildClue(IReadOnlyList<SolverProgress> changes, IUpdatableSudokuSolvingState snapshot)
    {
        return Clue<ISudokuHighlighter>.Default();
    }
}