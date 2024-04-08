using System.Collections.Generic;
using Model.Sudoku.Generator;

namespace DesktopApplication.Presenter.Sudoku.Generate;

public interface ISudokuGenerateView
{
    void UpdateNotEvaluatedList(IEnumerable<Model.Sudoku.Sudoku> sudokus);

    void UpdateEvaluatedList(IEnumerable<EvaluatedGeneratedPuzzle> sudokus);

    void UpdateCurrentlyEvaluated(Model.Sudoku.Sudoku? sudoku);

    void AllowGeneration(bool allowed);
}