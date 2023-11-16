using Global;
using Presenter.Translator;

namespace Presenter;

public interface ISolverView
{
    void SetCellTo(int row, int col, int number);
    void SetCellTo(int row, int col, int[] possibilities);
    void UpdateGivens(HashSet<Cell> givens);
    void SetTranslation(string translation);
    void FocusLog(int number);
    void UnFocusLog();
    void ShowExplanation(string explanation);
    void SetLogs(IReadOnlyList<ViewLog> logs);
    void ClearLogs();
    void InitializeStrategies(IReadOnlyList<ViewStrategy> strategies);
    void UpdateStrategies(IReadOnlyList<ViewStrategy> strategies);
    void LightUpStrategy(int number);
}