using Global;
using Presenter.Translator;

namespace Presenter;

public interface ISolverView : ISudokuDrawer
{
    void DisableActions();
    void EnableActions();
    void SetCellTo(int row, int col, int number);
    void SetCellTo(int row, int col, int[] possibilities);
    void UpdateGivens(HashSet<Cell> givens);
    void SetTranslation(string translation);
    void FocusLog(int number);
    void UnFocusLog();
    void ShowExplanation(string explanation);
    void SetLogs(IReadOnlyList<ViewLog> logs);
    void UpdateFocusedLog(ViewLog log);
    void InitializeStrategies(IReadOnlyList<ViewStrategy> strategies);
    void UpdateStrategies(IReadOnlyList<ViewStrategy> strategies);
    void LightUpStrategy(int number);
    void PutCursorOn(Cell cell);
    void ClearCursor();
    void UpdateBackground();
    void ToClipboard(string s);
    void ShowFullScan(string s);
    void ShowAllStrategies(string s);
}