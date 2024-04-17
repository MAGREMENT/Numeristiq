using Model.Helpers.Highlighting;
using Model.Helpers.Logs;

namespace DesktopApplication.Presenter.Tectonics.Solve;

public interface ITectonicSolveView
{
    public ITectonicDrawer Drawer { get; }

    void SetTectonicString(string s);
    void AddLog(ISolverLog<ITectonicHighlighter> log, StateShown _shown);
    void ClearLogs();
    void OpenLog(int index);
    void CloseLogs();
    void SetLogsStateShown(StateShown stateShown);
    void SetCursorPosition(int index, string s);
}