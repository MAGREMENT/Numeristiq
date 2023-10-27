using System;
using System.Collections.Generic;
using Model.Solver.Helpers.Changes;
using Model.Solver.Possibilities;

namespace Model.Solver.Strategies;

public class GurthTheorem : AbstractStrategy //TODO : rotational symmetry + test
{
    public const string OfficialName = "Gurth's Theorem";
    private const OnCommitBehavior DefaultBehavior = OnCommitBehavior.WaitForAll;
    
    public override OnCommitBehavior DefaultOnCommitBehavior => DefaultBehavior;

    private readonly List<Symmetry> _symmetries;

    public GurthTheorem() : base(OfficialName, StrategyDifficulty.Medium, DefaultBehavior)
    {
        UniquenessDependency = UniquenessDependency.FullyDependent;
        _symmetries = new List<Symmetry>
        {
            new TopLeftToBottomRightCross(this), new TopRightToBottomLeftCross(this)
        };
    }

    public override void ApplyOnce(IStrategyManager strategyManager)
    {
        foreach (var symmetry in _symmetries)
        {
            symmetry.Run(strategyManager);
        }
    }

    public override void OnNewSudoku(Sudoku s)
    {
        base.OnNewSudoku(s);

        foreach (var symmetry in _symmetries)
        {
            symmetry.Reset();
        }
    }
}

public abstract class Symmetry
{
    private readonly IStrategy _strategy;
    protected int[] Mapping { get; private set; } = Array.Empty<int>();

    private bool _isSymmetric;
    private bool _appliedOnce;

    protected Symmetry(IStrategy strategy)
    {
        _strategy = strategy;
    }

    public void Run(IStrategyManager strategyManager)
    {
        if (!_isSymmetric)
        {
            if (Check(strategyManager, out var mapping))
            {
                _isSymmetric = true;
                Mapping = mapping;
            }
            else return;
        }

        if (!_appliedOnce)
        {
            ApplyOnce(strategyManager);
            _appliedOnce = true;
        }

        ApplyEveryTime(strategyManager);

        if (strategyManager.ChangeBuffer.NotEmpty())
            strategyManager.ChangeBuffer.Commit(_strategy, new GurthTheoremReportBuilder());
    }

    public void Reset()
    {
        _isSymmetric = false;
    }

    protected abstract void ApplyOnce(IStrategyManager strategyManager);
    
    protected abstract void ApplyEveryTime(IStrategyManager strategyManager);

    protected abstract bool Check(IStrategyManager strategyManager, out int[] mapping);
}

public class TopLeftToBottomRightCross : Symmetry
{
    public TopLeftToBottomRightCross(IStrategy strategy) : base(strategy)
    {
    }
    
    protected override void ApplyOnce(IStrategyManager strategyManager)
    {
        IPossibilities selfMap = IPossibilities.NewEmpty();
        for (int i = 0; i < Mapping.Length; i++)
        {
            if (Mapping[i] == i + 1) selfMap.Add(i + 1);
        }

        for (int unit = 0; unit < 9; unit++)
        {
            for (int possibility = 1; possibility <= 9; possibility++)
            {
                if (selfMap.Peek(possibility)) continue;
                
                strategyManager.ChangeBuffer.AddPossibilityToRemove(possibility, unit, unit);
            }
        }
    }

    protected override void ApplyEveryTime(IStrategyManager strategyManager)
    {
        for (int row = 0; row < 9; row++)
        {
            for (int col = 0; col < 9; col++)
            {
                var solved = strategyManager.Sudoku[row, col];
                if (solved == 0) continue;
                
                if(strategyManager.Sudoku[col, row] == 0)
                    strategyManager.ChangeBuffer.AddSolutionToAdd(Mapping[solved - 1], col, row);
            }
        }
    }

    protected override bool Check(IStrategyManager strategyManager, out int[] mapping)
    {
        mapping = new int[9];

        for (int row = 0; row < 9; row++)
        {
            for (int col = 0; col < 9; col++)
            {
                var solved = strategyManager.Sudoku[row, col];
                if (solved == 0) continue;

                var symmetry = strategyManager.Sudoku[col, row];
                if (symmetry == 0) return false;

                var definedSymmetry = mapping[solved - 1];
                if (definedSymmetry == 0) mapping[solved - 1] = symmetry;
                else if (definedSymmetry != symmetry)
                {
                    if (row != col) return false;
                }
            }
        }

        int count = 0;
        for (int i = 0; i < mapping.Length; i++)
        {
            var symmetry = mapping[i];
            if (symmetry == 0) return false;
            if (symmetry == i + 1) count++;
        }

        return count >= 3;
    }
}

public class TopRightToBottomLeftCross : Symmetry
{
    public TopRightToBottomLeftCross(IStrategy strategy) : base(strategy)
    {
    }
    
    protected override void ApplyOnce(IStrategyManager strategyManager)
    {
        IPossibilities selfMap = IPossibilities.NewEmpty();
        for (int i = 0; i < Mapping.Length; i++)
        {
            if (Mapping[i] == i + 1) selfMap.Add(i + 1);
        }

        for (int unit = 0; unit < 9; unit++)
        {
            for (int possibility = 1; possibility <= 9; possibility++)
            {
                if (selfMap.Peek(possibility)) continue;
                
                strategyManager.ChangeBuffer.AddPossibilityToRemove(possibility, unit, 8 - unit);
            }
        }
    }

    protected override void ApplyEveryTime(IStrategyManager strategyManager)
    {
        for (int row = 0; row < 9; row++)
        {
            for (int col = 0; col < 9; col++)
            {
                var solved = strategyManager.Sudoku[row, col];
                if (solved == 0) continue;
                
                if(strategyManager.Sudoku[8 - col, row] == 0)
                    strategyManager.ChangeBuffer.AddSolutionToAdd(Mapping[solved - 1], 8 - col, row);
            }
        }
    }

    protected override bool Check(IStrategyManager strategyManager, out int[] mapping)
    {
        mapping = new int[9];

        for (int row = 0; row < 9; row++)
        {
            for (int col = 0; col < 9; col++)
            {
                var solved = strategyManager.Sudoku[row, col];
                if (solved == 0) continue;

                var symmetry = strategyManager.Sudoku[8 - col, row];
                if (symmetry == 0) return false;

                var definedSymmetry = mapping[solved - 1];
                if (definedSymmetry == 0) mapping[solved - 1] = symmetry;
                else if (definedSymmetry != symmetry)
                {
                    if (row != 8 - col) return false;
                }
            }
        }
        
        int count = 0;
        for (int i = 0; i < mapping.Length; i++)
        {
            var symmetry = mapping[i];
            if (symmetry == 0) return false;
            if (symmetry == i + 1) count++;
        }

        return count >= 3;
    }
}

public class GurthTheoremReportBuilder : IChangeReportBuilder
{
    public ChangeReport Build(List<SolverChange> changes, IPossibilitiesHolder snapshot)
    {
        return ChangeReport.Default(changes);
    }
}