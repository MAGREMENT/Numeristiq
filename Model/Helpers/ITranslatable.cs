using Model.Sudoku.Solver.BitSets;

namespace Model.Helpers;

public interface ITranslatable
{
    int this[int row, int col] { get; }
    ReadOnlyBitSet16 PossibilitiesAt(int row, int col);
}