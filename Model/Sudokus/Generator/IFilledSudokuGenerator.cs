using System.Collections.Generic;

namespace Model.Sudokus.Generator;

public interface IFilledSudokuGenerator
{
    public Sudoku Generate();

    public List<int> GetRemovableCells();
}