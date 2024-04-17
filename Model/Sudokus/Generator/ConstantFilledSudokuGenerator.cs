using System.Collections.Generic;

namespace Model.Sudokus.Generator;

public class ConstantFilledSudokuGenerator : IFilledSudokuGenerator
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

    public List<int> GetRemovableCells()
    {
        var list = new List<int>(41);

        for (int i = 0; i < 81; i++)
        {
            if (Sudoku[i / 9, i % 9] != 0) list.Add(i);
        }

        return list;
    }
}