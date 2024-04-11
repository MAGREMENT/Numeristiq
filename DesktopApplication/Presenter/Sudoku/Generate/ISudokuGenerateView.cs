using System.Collections.Generic;
using Model.Sudoku.Generator;

namespace DesktopApplication.Presenter.Sudoku.Generate;

public interface ISudokuGenerateView
{
    void UpdateEvaluatedList(IEnumerable<GeneratedSudokuPuzzle> sudokus);

    void AllowGeneration(bool allowed);
}