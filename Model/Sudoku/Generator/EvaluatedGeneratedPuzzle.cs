using Model.Sudoku.Solver;

namespace Model.Sudoku.Generator;

public record EvaluatedGeneratedPuzzle(string Sudoku, double Rating, SudokuStrategy? HardestStrategy);