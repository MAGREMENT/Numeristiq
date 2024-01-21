using Model.Solver.Helpers.Logs;
using Presenter.Translators;

namespace Presenter.Solver;

public interface ISolverView : ISolverDrawer
{
    void DisableActions();
    void EnableActions();
    void SetTranslation(string translation);
    void FocusLog(int number);
    void UnFocusLog();
    void SetLogs(IReadOnlyList<ViewLog> logs);
    void ShowFocusedLog(ViewLog log);
    void InitializeStrategies(IReadOnlyList<ViewStrategy> strategies);
    void UpdateStrategies(IReadOnlyList<ViewStrategy> strategies);
    void LightUpStrategy(int number);
    void ToClipboard(string s);
    void ShowPossibleSteps(StepChooserPresenterBuilder builder);
    void ShowExplanation(ISolverLog log);
}