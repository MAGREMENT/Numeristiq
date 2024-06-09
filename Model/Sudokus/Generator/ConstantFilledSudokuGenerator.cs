using System.Collections.Generic;
using Model.Core.Generators;
using Model.Utility;

namespace Model.Sudokus.Generator;

public class ConstantFilledSudokuGenerator : IFilledPuzzleGenerator<Sudoku>
{
    public Sudoku Sudoku { get; set; }
    
    public ConstantFilledSudokuGenerator(Sudoku sudoku)
    {
        Sudoku = sudoku;
    }
    
    public Sudoku Generate()
    {
        return Sudoku.Copy();
    }

    public Sudoku Generate(out List<Cell> removableCells)
    {
        removableCells = GetRemovableCells();
        return Generate();
    }

    private List<Cell> GetRemovableCells()
    {
        var list = new List<Cell>(41);
        for (int row = 0; row < 9; row++)
        {
            for (int col = 0; col < 9; col++)
            {
                if (Sudoku[row, col] != 0) list.Add(new Cell(row, col));
            }
        }

        return list;
    }
}