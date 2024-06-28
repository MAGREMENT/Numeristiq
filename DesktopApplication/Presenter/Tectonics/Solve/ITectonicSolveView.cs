using Model.Core.Highlighting;
using Model.Core.Steps;

namespace DesktopApplication.Presenter.Tectonics.Solve;

public interface ITectonicSolveView
{
    public ITectonicDrawer Drawer { get; }

    void SetTectonicString(string s);
    void AddLog(IStep step, StateShown _shown);
    void ClearLogs();
    void OpenLog(int index);
    void CloseLogs();
    void SetLogsStateShown(StateShown stateShown);
    void SetCursorPosition(int index, string s);
}