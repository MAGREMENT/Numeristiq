using Model.Core;
using Model.Core.Changes;
using Model.Core.Highlighting;

namespace Model.Sudokus.Solver.Strategies.Symmetry;

public class AntiGurthTheorem : SudokuStrategy
{
    public const string OfficialName = "Anti Gurth's Theorem";
    private const InstanceHandling DefaultInstanceHandling = InstanceHandling.UnorderedAll;

    private readonly SudokuSymmetry[] _symmetries;

    public AntiGurthTheorem(SudokuSymmetry[] symmetries) : base(OfficialName, Difficulty.Medium, DefaultInstanceHandling)
    {
        UniquenessDependency = UniquenessDependency.FullyDependent;
        _symmetries = symmetries;
    }

    public override void Apply(ISudokuSolverData data)
    {
        foreach (var s in _symmetries)
        {
            var result = s.CheckAlmostSymmetry(data);
            if (result is null || !WouldSymmetryBeWrong(data, s, result)) continue;

            var cell = s.GetSymmetricalCell(result.Exception);
            data.ChangeBuffer.ProposePossibilityRemoval(
                result.Mapping[data[result.Exception.Row, result.Exception.Column] - 1], cell);
            if (!data.ChangeBuffer.NeedCommit()) continue;
            
            data.ChangeBuffer.Commit(DefaultNumericChangeReportBuilder<ISudokuSolvingState, 
                ISudokuHighlighter>.Instance);
            if (StopOnFirstCommit) return;
        }
    }

    private static bool WouldSymmetryBeWrong(ISudokuSolverData data, SudokuSymmetry symmetry, AlmostSymmetryResult result)
    {
        if (result.SelfMapCount > symmetry.MaximumSelfMapCount) return true;

        var p = SudokuSymmetry.SelfMapped(result.Mapping);
        foreach (var cell in symmetry.CenterCells())
        {
            var poss = data.PossibilitiesAt(cell);
            if(poss.Count == 0) continue;

            if (!poss.ContainsAny(p)) return true;
        }
        
        return false;
    }
}