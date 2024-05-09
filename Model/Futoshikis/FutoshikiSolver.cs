using System;
using Model.Utility.BitSets;

namespace Model.Futoshikis;

public class FutoshikiSolver
{
    private Futoshiki _futoshiki = new();
    private ReadOnlyBitSet16[,] _possibilities = new ReadOnlyBitSet16[0, 0];
    private ReadOnlyBitSet16[,] _rowPositions = new ReadOnlyBitSet16[0, 0];
    private ReadOnlyBitSet16[,] _columnPositions = new ReadOnlyBitSet16[0, 0];

    public IReadOnlyFutoshiki Futoshiki => _futoshiki;

    public void SetFutoshiki(Futoshiki futoshiki)
    {
        _futoshiki = futoshiki;
        _possibilities = new ReadOnlyBitSet16[futoshiki.Length, futoshiki.Length];
        _rowPositions = new ReadOnlyBitSet16[futoshiki.Length, futoshiki.Length];
        _columnPositions = new ReadOnlyBitSet16[futoshiki.Length, futoshiki.Length];
    }

    #region Private

    private void InitPossibilities()
    {
        for (int i = 0; i < _futoshiki.Length; i++)
        {
            for (int j = 0; j < _futoshiki.Length; j++)
            {
                _possibilities[i, j] = ReadOnlyBitSet16.Filled(1, _futoshiki.Length);
                _rowPositions[i, j] = ReadOnlyBitSet16.Filled(0, _futoshiki.Length - 1);
                _columnPositions[i, j] = ReadOnlyBitSet16.Filled(0, _futoshiki.Length - 1);
            }
        }
        
        for (int i = 0; i < _futoshiki.Length; i++)
        {
            for (int j = 0; j < _futoshiki.Length; j++)
            {
                var n = _futoshiki[i, j];
                if (n != 0) UpdatePossibilitiesAfterSolutionAdded(n, i, j);
            }
        }
    }

    private void UpdatePossibilitiesAfterSolutionAdded(int n, int row, int col)
    {
        _possibilities[row, col] = new ReadOnlyBitSet16();
        _rowPositions[row, n - 1] = new ReadOnlyBitSet16();
        _columnPositions[col, n - 1] = new ReadOnlyBitSet16();
        
        for (int i = 0; i < _futoshiki.Length; i++)
        {
            _possibilities[row, i] -= n;
            _possibilities[i, col] -= n;
            _rowPositions[row, i] -= col;
            _columnPositions[col, i] -= row;
        }
    }

    #endregion
}