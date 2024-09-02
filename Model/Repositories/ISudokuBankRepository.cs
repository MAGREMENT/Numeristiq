using System.Collections.Generic;
using Model.Core;
using Model.Sudokus;

namespace Model.Repositories;

public interface ISudokuBankRepository
{
    void Initialize();
    Sudoku? FindRandom(Difficulty difficulty);
    void Clear();
    void Add(Sudoku sudoku, Difficulty difficulty);
    int Add(IEnumerable<(Sudoku, Difficulty)> entries);
    bool IsAvailable();
}