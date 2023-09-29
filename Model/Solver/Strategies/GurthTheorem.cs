using System.Collections.Generic;
using Model.Solver.Helpers.Changes;
using Model.Solver.Possibilities;

namespace Model.Solver.Strategies;

public class GurthTheorem : AbstractStrategy //TODO : rotational symmetry
{
    public const string OfficialName = "Gurth's Theorem";

    private bool _hasSymmetry;
    private readonly List<SymmetryEliminations> _eliminations = new();

    public GurthTheorem() : base(OfficialName, StrategyDifficulty.Medium)
    {
        UniquenessDependency = UniquenessDependency.FullyDependent;
    }

    public override void ApplyOnce(IStrategyManager strategyManager)
    {
        if (!_hasSymmetry) CheckForSymmetry(strategyManager);
        if (!_hasSymmetry) return;

        foreach (var elimination in _eliminations)
        {
            elimination.Apply(strategyManager);
        }
    }

    public override void OnNewSudoku(Sudoku s)
    {
        base.OnNewSudoku(s);

        _hasSymmetry = false;
        _eliminations.Clear();
    }

    private void CheckForSymmetry(IStrategyManager strategyManager)
    {
        CheckForTopLeftToBottomRight(strategyManager);
        CheckForTopRightToBottomLeft(strategyManager);
    }

    private void CheckForTopLeftToBottomRight(IStrategyManager strategyManager)
    {
        int[] potentialMapping = new int[9];

        for (int row = 0; row < 9; row++)
        {
            for (int col = 0; col < 9; col++)
            {
                var solved = strategyManager.Sudoku[row, col];
                if (solved == 0) continue;

                var symmetry = strategyManager.Sudoku[col, row];
                if (symmetry == 0) return;

                var definedSymmetry = potentialMapping[solved - 1];
                if (definedSymmetry == 0) potentialMapping[solved - 1] = symmetry;
                else if (definedSymmetry != symmetry)
                {
                    if (row != col) return;
                }
            }
        }

        int count = 0;
        for (int i = 0; i < potentialMapping.Length; i++)
        {
            var symmetry = potentialMapping[i];
            if (symmetry == 0) return;
            if (symmetry == i + 1) count++;
        }

        if (count < 3) return;

        _hasSymmetry = true;
        _eliminations.Add(new TopLeftToBottomRightCrossEliminations(this, potentialMapping));
    }

    private void CheckForTopRightToBottomLeft(IStrategyManager strategyManager)
    {
        int[] potentialMapping = new int[9];

        for (int row = 0; row < 9; row++)
        {
            for (int col = 0; col < 9; col++)
            {
                var solved = strategyManager.Sudoku[row, col];
                if (solved == 0) continue;

                var symmetry = strategyManager.Sudoku[8 - col, row];
                if (symmetry == 0) return;

                var definedSymmetry = potentialMapping[solved - 1];
                if (definedSymmetry == 0) potentialMapping[solved - 1] = symmetry;
                else if (definedSymmetry != symmetry)
                {
                    if (row != 8 - col) return;
                }
            }
        }
        
        int count = 0;
        for (int i = 0; i < potentialMapping.Length; i++)
        {
            var symmetry = potentialMapping[i];
            if (symmetry == 0) return;
            if (symmetry == i + 1) count++;
        }

        if (count < 3) return;

        _hasSymmetry = true;
        _eliminations.Add(new TopRightToBottomLeftCrossEliminations(this, potentialMapping));
    }
}

public abstract class SymmetryEliminations
{
    protected int[] Mapping { get; }

    private readonly IStrategy _strategy;
    private bool _appliedOnce;

    protected SymmetryEliminations(IStrategy strategy, int[] mapping)
    {
        _strategy = strategy;
        Mapping = mapping;
    }

    public void Apply(IStrategyManager strategyManager)
    {
        if (!_appliedOnce)
        {
            ApplyOnce(strategyManager);
            _appliedOnce = true;
        }

        ApplyEveryTime(strategyManager);

        if (strategyManager.ChangeBuffer.NotEmpty())
            strategyManager.ChangeBuffer.Push(_strategy, new GurthTheoremReportBuilder());
    }

    public abstract void ApplyOnce(IStrategyManager strategyManager);
    
    public abstract void ApplyEveryTime(IStrategyManager strategyManager);
}

public class TopLeftToBottomRightCrossEliminations : SymmetryEliminations
{
    public override void ApplyOnce(IStrategyManager strategyManager)
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

    public override void ApplyEveryTime(IStrategyManager strategyManager)
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

    public TopLeftToBottomRightCrossEliminations(IStrategy strategy, int[] mapping) : base(strategy, mapping)
    {
    }
}

public class TopRightToBottomLeftCrossEliminations : SymmetryEliminations
{
    public override void ApplyOnce(IStrategyManager strategyManager)
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

    public override void ApplyEveryTime(IStrategyManager strategyManager)
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

    public TopRightToBottomLeftCrossEliminations(IStrategy strategy, int[] mapping) : base(strategy, mapping)
    {
    }
}

public class GurthTheoremReportBuilder : IChangeReportBuilder
{
    public ChangeReport Build(List<SolverChange> changes, IPossibilitiesHolder snapshot)
    {
        return ChangeReport.Default(changes);
    }
}