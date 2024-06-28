using Model.Core.Steps;

namespace DesktopApplication.Presenter.Nonograms.Solve;

public interface INonogramSolveView
{
    INonogramDrawer Drawer { get; }

    void ShowNonogramAsString(string s);
    void AddLog(IStep step, StateShown _shown);
    void ClearLogs();
    void OpenLog(int index);
    void CloseLogs();
    void SetLogsStateShown(StateShown stateShown);
    void SetCursorPosition(int index, string s);
}