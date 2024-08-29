using System;
using System.Collections.Generic;
using Model.Core;
using Model.Core.Changes;
using Model.Core.Highlighting;
using Model.Utility.BitSets;

namespace Model.Sudokus.Solver.Strategies.Symmetry;

public class GurthTheorem : SudokuStrategy
{
    public const string OfficialName = "Gurth's Theorem";
    private const InstanceHandling DefaultInstanceHandling = InstanceHandling.UnorderedAll;

    private readonly SudokuSymmetry[] _symmetries;
    private readonly int[]?[] _mappings;

    public GurthTheorem(SudokuSymmetry[] symmetries) : base(OfficialName, Difficulty.Medium, DefaultInstanceHandling)
    {
        UniquenessDependency = UniquenessDependency.FullyDependent;
        _symmetries = symmetries;
        _mappings = new int[]?[_symmetries.Length];
    }

    public override void Apply(ISudokuSolverData solverData)
    {
        InfiniteBitSet toApplyOnce = new();
        
        for (int i = 0; i < _symmetries.Length; i++)
        {
            if (_mappings[i] is not null) continue;
            
            var m = _symmetries[i].CheckFullSymmetry(solverData);
            if (m is null) continue;
            
            _mappings[i] = m;
            toApplyOnce.Add(i);
        }

        for (int i = 0; i < _symmetries.Length; i++)
        {
            var m = _mappings[i];
            if(m is null) continue;
            
            var s = _symmetries[i];
            if(toApplyOnce.Contains(i)) s.ApplyOnceFullSymmetry(solverData, m);
            s.ApplyEveryTimeFullSymmetry(solverData, m);

            if (!solverData.ChangeBuffer.NeedCommit()) continue;
            
            solverData.ChangeBuffer.Commit(DefaultNumericChangeReportBuilder<ISudokuSolvingState, ISudokuHighlighter>.Instance);
            if (StopOnFirstCommit) return;
        }
    }

    public override void OnNewSudoku(IReadOnlySudoku s)
    {
        base.OnNewSudoku(s);
        Array.Fill(_mappings, null);
    }
}