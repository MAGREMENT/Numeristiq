using System;
using System.Collections.Generic;
using Model.Core;
using Model.Core.Changes;
using Model.Core.Highlighting;
using Model.Utility;
using Model.Utility.BitSets;

namespace Model.Sudokus.Solver.Strategies;

public class GurthTheorem : SudokuStrategy
{
    public const string OfficialName = "Gurth's Theorem";
    private const InstanceHandling DefaultInstanceHandling = InstanceHandling.UnorderedAll;

    private readonly List<Symmetry> _symmetries;

    public GurthTheorem() : base(OfficialName, StepDifficulty.Medium, DefaultInstanceHandling)
    {
        UniquenessDependency = UniquenessDependency.FullyDependent;
        _symmetries = new List<Symmetry>
        {
            new TopLeftToBottomRightCross(), new TopRightToBottomLeftCross(),
            new Rotational90(), new Rotational180(), new Rotational270()
        };
    }

    public override void Apply(ISudokuSolverData solverData)
    {
        foreach (var symmetry in _symmetries)
        {
            symmetry.Run(solverData);
        }
    }

    public override void OnNewSudoku(IReadOnlySudoku s)
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
    private int[] _mapping = Array.Empty<int>();
    private bool _isSymmetric;
    private bool _appliedOnce;

    public void Run(ISudokuSolverData solverData)
    {
        if (!_isSymmetric)
        {
            if (Check(solverData))
            {
                _isSymmetric = true;
            }
            else return;
        }

        if (!_appliedOnce)
        {
            ApplyOnce(solverData);
            _appliedOnce = true;
        }

        ApplyEveryTime(solverData);

        if (solverData.ChangeBuffer.NotEmpty())
            solverData.ChangeBuffer.Commit(DefaultChangeReportBuilder<ISudokuSolvingState, ISudokuHighlighter>.Instance);
    }

    public void Reset()
    {
        _isSymmetric = false;
    }

    protected abstract int MinimumSelfMapCount { get; }
    protected abstract IEnumerable<Cell> CenterCells();
    protected abstract Cell GetSymmetricalCell(int row, int col);

    private void ApplyOnce(ISudokuSolverData solverData)
    {
        var selfMap = SelfMap();

        foreach (var cell in CenterCells())
        {
            foreach (var possibility in solverData.PossibilitiesAt(cell).EnumeratePossibilities())
            {
                if (!selfMap.Contains(possibility))
                {
                    solverData.ChangeBuffer.ProposePossibilityRemoval(possibility, cell.Row, cell.Column);
                }
            }
        }
    }

    private void ApplyEveryTime(ISudokuSolverData solverData)
    {
        for (int row = 0; row < 9; row++)
        {
            for (int col = 0; col < 9; col++)
            {
                var symmetry = GetSymmetricalCell(row, col);
                var solved = solverData.Sudoku[row, col];

                if (solved != 0) solverData.ChangeBuffer.ProposeSolutionAddition(_mapping[solved - 1],
                    symmetry.Row, symmetry.Column);
                else
                {
                    var symmetricPossibilities = solverData.PossibilitiesAt(symmetry);
                    var mappedPossibilities = GetMappedPossibilities(solverData.PossibilitiesAt(row, col));

                    foreach (var possibility in symmetricPossibilities.EnumeratePossibilities())
                    {
                        if(!mappedPossibilities.Contains(possibility)) solverData.ChangeBuffer
                            .ProposePossibilityRemoval(possibility, symmetry.Row, symmetry.Column);
                    }
                }
            }
        }
    }

    private bool Check(ISudokuSolverData solverData)
    {
        _mapping = new int[9];
        HashSet<Cell> centerCells = new HashSet<Cell>(CenterCells());

        for (int row = 0; row < 9; row++)
        {
            for (int col = 0; col < 9; col++)
            {
                if (centerCells.Contains(new Cell(row, col))) continue;

                var solved = solverData.Sudoku[row, col];
                if (solved == 0) continue;

                var symmetricCell = GetSymmetricalCell(row, col);

                var symmetry = solverData.Sudoku[symmetricCell.Row, symmetricCell.Column];
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