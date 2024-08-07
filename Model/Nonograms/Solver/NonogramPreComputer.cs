using System;
using Model.Utility.BitSets;

namespace Model.Nonograms.Solver;

public class NonogramPreComputer
{
    private readonly INonogramSolverData _data;
    
    private MultiValueSpace[] _horizontalMainSpace = Array.Empty<MultiValueSpace>();
    private MultiValueSpace[] _verticalMainSpace = Array.Empty<MultiValueSpace>();
    private readonly InfiniteBitSet _hrvHits = new();
    private readonly InfiniteBitSet _vrvHits = new();

    private ValueSpaceCollection?[] _horizontalValueSpaces = Array.Empty<ValueSpaceCollection>();
    private ValueSpaceCollection?[] _verticalValueSpaces = Array.Empty<ValueSpaceCollection>();

    public NonogramPreComputer(INonogramSolverData data)
    {
        _data = data;
    }

    public void AdaptToNewSize(int rowCount, int colCount)
    {
        _horizontalMainSpace = new MultiValueSpace[rowCount];
        _horizontalValueSpaces = new ValueSpaceCollection?[rowCount];
        
        _verticalMainSpace = new MultiValueSpace[colCount];
        _verticalValueSpaces = new ValueSpaceCollection?[colCount];
    }

    public MultiValueSpace HorizontalRemainingValuesSpace(int row)
    {
        if (!_hrvHits.Contains(row)) _horizontalMainSpace[row] = NonogramUtility.HorizontalRemainingValuesSpace(
                _data.Nonogram, _data, row);
        return _horizontalMainSpace[row];
    }

    public MultiValueSpace VerticalRemainingValuesSpace(int col)
    {
        if (!_vrvHits.Contains(col)) _verticalMainSpace[col] = NonogramUtility.VerticalRemainingValuesSpace(
                _data.Nonogram, _data, col);
        return _verticalMainSpace[col];
    }

    public IReadOnlyValueSpaceCollection HorizontalValueSpaces(int row)
    {
        _horizontalValueSpaces[row] ??= NonogramUtility.HorizontalValueSpaces(_data.Nonogram, _data,
            HorizontalRemainingValuesSpace(row), row);
        return _horizontalValueSpaces[row]!;
    }

    public IReadOnlyValueSpaceCollection VerticalValueSpaces(int col)
    {
        _verticalValueSpaces[col] ??= NonogramUtility.VerticalValueSpaces(_data.Nonogram, _data,
            VerticalRemainingValuesSpace(col), col);
        return _verticalValueSpaces[col]!;
    }

    public void Reset()
    {
        _hrvHits.Clear();
        _vrvHits.Clear();
        Array.Fill(_horizontalValueSpaces, null);
        Array.Fill(_verticalValueSpaces, null);
    }
}

