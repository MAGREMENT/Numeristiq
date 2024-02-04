using System;
using System.Collections.Generic;
using Model.Sudoku.Solver.BitSets;
using Model.Sudoku.Solver.Helpers.Changes;
using Model.Sudoku.Solver.Possibility;
using Model.Utility;

namespace Model.Sudoku.Solver.Strategies;

public class GurthTheorem : AbstractStrategy
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
            new TopLeftToBottomRightCross(this), new TopRightToBottomLeftCross(this),
            new Rotational90(this), new Rotational180(this), new Rotational270(this)
        };
    }

    public override void Apply(IStrategyUser strategyUser)
    {
        foreach (var symmetry in _symmetries)
        {
            symmetry.Run(strategyUser);
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

    private int[] _mapping = Array.Empty<int>();
    private bool _isSymmetric;
    private bool _appliedOnce;

    protected Symmetry(IStrategy strategy)
    {
        _strategy = strategy;
    }

    public void Run(IStrategyUser strategyUser)
    {
        if (!_isSymmetric)
        {
            if (Check(strategyUser))
            {
                _isSymmetric = true;
            }
            else return;
        }

        if (!_appliedOnce)
        {
            ApplyOnce(strategyUser);
            _appliedOnce = true;
        }

        ApplyEveryTime(strategyUser);

        if (strategyUser.ChangeBuffer.NotEmpty())
            strategyUser.ChangeBuffer.Commit(_strategy, new GurthTheoremReportBuilder());
    }

    public void Reset()
    {
        _isSymmetric = false;
    }

    protected abstract int MinimumSelfMapCount { get; }
    protected abstract IEnumerable<Cell> CenterCells();
    protected abstract Cell GetSymmetricalCell(int row, int col);

    private void ApplyOnce(IStrategyUser strategyUser)
    {
        var selfMap = SelfMap();

        foreach (var cell in CenterCells())
        {
            foreach (var possibility in strategyUser.PossibilitiesAt(cell).EnumeratePossibilities())
            {
                if (!selfMap.Contains(possibility))
                {
                    strategyUser.ChangeBuffer.ProposePossibilityRemoval(possibility, cell.Row, cell.Column);
                }
            }
        }
    }

    private void ApplyEveryTime(IStrategyUser strategyUser)
    {
        for (int row = 0; row < 9; row++)
        {
            for (int col = 0; col < 9; col++)
            {
                var symmetry = GetSymmetricalCell(row, col);
                var solved = strategyUser.Sudoku[row, col];

                if (solved != 0) strategyUser.ChangeBuffer.ProposeSolutionAddition(_mapping[solved - 1],
                    symmetry.Row, symmetry.Column);
                else
                {
                    var symmetricPossibilities = strategyUser.PossibilitiesAt(symmetry);
                    var mappedPossibilities = GetMappedPossibilities(strategyUser.PossibilitiesAt(row, col));

                    foreach (var possibility in symmetricPossibilities.EnumeratePossibilities())
                    {
                        if(!mappedPossibilities.Contains(possibility)) strategyUser.ChangeBuffer
                            .ProposePossibilityRemoval(possibility, symmetry.Row, symmetry.Column);
                    }
                }
            }
        }
    }

    private bool Check(IStrategyUser strategyUser)
    {
        _mapping = new int[9];
        HashSet<Cell> centerCells = new HashSet<Cell>(CenterCells());

        for (int row = 0; row < 9; row++)
        {
            for (int col = 0; col < 9; col++)
            {
                if (centerCells.Contains(new Cell(row, col))) continue;

                var solved = strategyUser.Sudoku[row, col];
                if (solved == 0) continue;

                var symmetricCell = GetSymmetricalCell(row, col);

                var symmetry = strategyUser.Sudoku[symmetricCell.Row, symmetricCell.Column];
                if (symmetry == 0) return false;

                var definedSymmetry = _mapping[solved - 1];
                if (definedSymmetry == 0) _mapping[solved - 1] = symmetry;
                else if (definedSymmetry != symmetry) return false;
            }
        }

        int count = 0;
        for (int i = 0; i < _mapping.Length; i++)
        {
            var symmetry = _mapping[i];
            if (symmetry == 0) return false;
            if (symmetry == i + 1) count++;
        }

        return count >= MinimumSelfMapCount;
    }

    private ReadOnlyBitSet16 SelfMap()
    {
        var selfMap = new ReadOnlyBitSet16();
        for (int i = 0; i < _mapping.Length; i++)
        {
            if (_mapping[i] == i + 1) selfMap += i + 1;
        }

        return selfMap;
    }

    private ReadOnlyBitSet16 GetMappedPossibilities(ReadOnlyBitSet16 possibilities)
    {
        var result = new ReadOnlyBitSet16();
        foreach (var possibility in possibilities.EnumeratePossibilities())
        {
            result += _mapping[possibility - 1];
        }

        return result;
    }

}

public class TopLeftToBottomRightCross : Symmetry
{
    public TopLeftToBottomRightCross(IStrategy strategy) : base(strategy)
    {
    }

    protected override int MinimumSelfMapCount => 3;

    protected override IEnumerable<Cell> CenterCells()
    {
        for (int unit = 0; unit < 9; unit++)
        {
            yield return new Cell(unit, unit);
        }
    }

    protected override Cell GetSymmetricalCell(int row, int col)
    {
        return new Cell(col, row);
    }
}

public class TopRightToBottomLeftCross : Symmetry
{
    public TopRightToBottomLeftCross(IStrategy strategy) : base(strategy)
    {
    }

    protected override int MinimumSelfMapCount => 3;

    protected override IEnumerable<Cell> CenterCells()
    {
        for (int unit = 0; unit < 9; unit++)
        {
            yield return new Cell(unit, 8 - unit);
        }
    }
    
    protected override Cell GetSymmetricalCell(int row, int col)
    {
        return new Cell(8 - col, 8 - row);
    }
}

public class Rotational90 : Symmetry
{
    public Rotational90(IStrategy strategy) : base(strategy)
    {
    }

    protected override int MinimumSelfMapCount => 1;
    protected override IEnumerable<Cell> CenterCells()
    {
        yield return new Cell(4, 4);
    }

    protected override Cell GetSymmetricalCell(int row, int col)
    {
        return new Cell(col, 8 - row);
    }
}

public class Rotational180 : Symmetry
{
    public Rotational180(IStrategy strategy) : base(strategy)
    {
    }

    protected override int MinimumSelfMapCount => 1;
    protected override IEnumerable<Cell> CenterCells()
    {
        yield return new Cell(4, 4);
    }

    protected override Cell GetSymmetricalCell(int row, int col)
    {
        return new Cell(8 - row, 8 - col);
    }
}

public class Rotational270 : Symmetry
{
    public Rotational270(IStrategy strategy) : base(strategy)
    {
    }

    protected override int MinimumSelfMapCount => 1;
    protected override IEnumerable<Cell> CenterCells()
    {
        yield return new Cell(4, 4);
    }

    protected override Cell GetSymmetricalCell(int row, int col)
    {
        return new Cell(8 - col, row);
    }
}

public class GurthTheoremReportBuilder : IChangeReportBuilder
{
    public ChangeReport Build(IReadOnlyList<SolverChange> changes, IPossibilitiesHolder snapshot)
    {
        return ChangeReport.Default(changes);
    }
}