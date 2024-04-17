using System.Collections.Generic;
using Model.Sudokus;
using Model.Sudokus.Generator;

namespace DesktopApplication.Presenter.Sudokus.Generate;

public interface ISudokuGenerateView
{
    void ActivateFilledSudokuGenerator(bool activated);
    void ActivateRandomDigitRemover(bool activated);
    void ActivatePuzzleEvaluator(bool activated);
    void ShowTransition(TransitionPlace place);
    void UpdateEvaluatedList(IEnumerable<GeneratedSudokuPuzzle> sudokus);
    void AllowGeneration(bool allowed);
    void AllowCancel(bool allowed);
    void SetCriteriaList(IReadOnlyList<EvaluationCriteria> criteriaList);
    void ShowSudoku(Sudoku sudoku);
}

public enum TransitionPlace
{
    ToRDR, ToEvaluator, ToFinalList, ToBin
}