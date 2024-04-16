using System.Collections.Generic;

namespace Model.Sudoku.Generator;

public interface IFilledSudokuGenerator
{
    public Sudoku Generate();

    public List<int> GetRemovableCells();
}