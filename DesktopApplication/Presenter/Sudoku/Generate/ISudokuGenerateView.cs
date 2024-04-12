using System.Collections.Generic;
using Model.Sudoku.Generator;

namespace DesktopApplication.Presenter.Sudoku.Generate;

public interface ISudokuGenerateView
{
    void ActivateFilledSudokuGenerator(bool activated);
    void ActivateRandomDigitRemover(bool activated);
    void ActivatePuzzleEvaluator(bool activated);
    void ShowTransition(TransitionPlace place);
    void UpdateEvaluatedList(IEnumerable<GeneratedSudokuPuzzle> sudokus);
    void AllowGeneration(bool allowed);
    void AllowCancel(bool allowed);
}

public enum TransitionPlace
{
    ToRDR, ToEvaluator, ToFinalList
}