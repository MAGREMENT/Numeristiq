using System;
using System.Collections.Generic;
using Global;
using Model.SudokuSolving.Solver.Helpers.Changes;
using Model.SudokuSolving.Solver.Possibility;

namespace Model.SudokuSolving.Solver.Strategies;

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

    public override void Apply(IStrategyManager strategyManager)
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

    private int[] _mapping = Array.Empty<int>();
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
            if (Check(strategyManager))
            {
                _isSymmetric = true;
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

    protected abstract int MinimumSelfMapCount { get; }
    protected abstract IEnumerable<Cell> CenterCells();
    protected abstract Cell GetSymmetricalCell(int row, int col);

    private void ApplyOnce(IStrategyManager strategyManager)
    {
        var selfMap = SelfMap();

        foreach (var cell in CenterCells())
        {
            foreach (var possibility in strategyManager.PossibilitiesAt(cell))
            {
                if (!selfMap.Peek(possibility))
                {
                    strategyManager.ChangeBuffer.ProposePossibilityRemoval(possibility, cell.Row, cell.Column);
                }
            }
        }
    }

    private void ApplyEveryTime(IStrategyManager strategyManager)
    {
        for (int row = 0; row < 9; row++)
        {
            for (int col = 0; col < 9; col++)
            {
                var symmetry = GetSymmetricalCell(row, col);
                var solved = strategyManager.Sudoku[row, col];

                if (solved != 0) strategyManager.ChangeBuffer.ProposeSolutionAddition(_mapping[solved - 1],
                    symmetry.Row, symmetry.Column);
                else
                {
                    var symmetricPossibilities = strategyManager.PossibilitiesAt(symmetry);
                    var mappedPossibilities = GetMappedPossibilities(strategyManager.PossibilitiesAt(row, col));

                    foreach (var possibility in symmetricPossibilities)
                    {
                        if(!mappedPossibilities.Peek(possibility)) strategyManager.ChangeBuffer
                            .ProposePossibilityRemoval(possibility, symmetry.Row, symmetry.Column);
                    }
                }
            }
        }
    }

    private bool Check(IStrategyManager strategyManager)
    {
        _mapping = new int[9];
        HashSet<Cell> centerCells = new HashSet<Cell>(CenterCells());

        for (int row = 0; row < 9; row++)
        {
            for (int col = 0; col < 9; col++)
            {
                if (centerCells.Contains(new Cell(row, col))) continue;

                var solved = strategyManager.Sudoku[row, col];
                if (solved == 0) continue;

                var symmetricCell = GetSymmetricalCell(row, col);

                var symmetry = strategyManager.Sudoku[symmetricCell.Row, symmetricCell.Column];
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

    private Possibilities SelfMap()
    {
        Possibilities selfMap = Possibilities.NewEmpty();
        for (int i = 0; i < _mapping.Length; i++)
        {
            if (_mapping[i] == i + 1) selfMap.Add(i + 1);
        }

        return selfMap;
    }

    private Possibilities GetMappedPossibilities(IReadOnlyPossibilities possibilities)
    {
        Possibilities result = Possibilities.NewEmpty();
        foreach (var possibility in possibilities)
        {
            result.Add(_mapping[possibility - 1]);
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