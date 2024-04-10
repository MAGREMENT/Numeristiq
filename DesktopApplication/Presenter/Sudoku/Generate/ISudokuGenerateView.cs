using System.Collections.Generic;
using Model.Sudoku.Generator;

namespace DesktopApplication.Presenter.Sudoku.Generate;

public interface ISudokuGenerateView
{
    void UpdateNotEvaluatedList(IEnumerable<GeneratedSudokuPuzzle> sudokus);

    void UpdateEvaluatedList(IEnumerable<GeneratedSudokuPuzzle> sudokus);

    void UpdateCurrentlyEvaluated(GeneratedSudokuPuzzle? sudoku);

    void AllowGeneration(bool allowed);
}