using System.Collections.Generic;
using Model.Core;
using Model.Core.Changes;
using Model.Core.Graphs;
using Model.Core.Highlighting;
using Model.Core.Settings;
using Model.Core.Settings.Types;
using Model.Sudokus.Solver.Utility;
using Model.Sudokus.Solver.Utility.Graphs;
using Model.Sudokus.Solver.Utility.Oddagons;

namespace Model.Sudokus.Solver.Strategies;

public class OddagonStrategy : SudokuStrategy
{
    public const string OfficialName = "Oddagon";
    private const InstanceHandling DefaultInstanceHandling = InstanceHandling.FirstOnly;

    private readonly IntSetting _maxNumberOfGuardians;
    private readonly IntSetting _maxLength;
    
    public OddagonStrategy(int maxLength, int maxNumberOfGuardians) : base(OfficialName, Difficulty.Extreme, DefaultInstanceHandling)
    {
        _maxLength = new IntSetting("Maximum length",
            "The maximum length of the oddagon",
            new SliderInteractionInterface(3, 15, 2), maxLength);
        _maxNumberOfGuardians = new IntSetting("Maximum number of guardians", 
            "The maximum amount of guardians an oddagon can have",
            new SliderInteractionInterface(1, 7, 1), maxNumberOfGuardians);
    }
    
    public override IEnumerable<ISetting> EnumerateSettings()
    {
        yield return _maxLength;
        yield return _maxNumberOfGuardians;
    }
    
    public override void Apply(ISudokuSolverData solverData)
    {
        foreach (var ao in solverData.PreComputer.AlmostOddagons(_maxLength.Value, _maxNumberOfGuardians.Value))
        {
            if (ao.Guardians.Length == 1) solverData.ChangeBuffer.ProposeSolutionAddition(ao.Guardians[0]);
            else
            {
                foreach (var cp in SudokuUtility.SharedSeenExistingPossibilities(solverData, ao.Guardians))
                {
                    solverData.ChangeBuffer.ProposePossibilityRemoval(cp);
                }
            }

            if (solverData.ChangeBuffer.NeedCommit())
            {
                solverData.ChangeBuffer.Commit(new OddagonReportBuilder(ao));
                if (StopOnFirstCommit) return;
            }
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