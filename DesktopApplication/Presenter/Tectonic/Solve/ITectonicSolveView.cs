using Model;
using Model.Helpers.Highlighting;
using Model.Helpers.Logs;

namespace DesktopApplication.Presenter.Tectonic.Solve;

public interface ITectonicSolveView
{
    public ITectonicDrawer Drawer { get; }
    
    void AddLog(ISolverLog<ITectonicHighlighter> log, StateShown _shown);
    void ClearLogs();
    void OpenLog(int index);
    void CloseLogs();
    void SetLogsStateShown(StateShown stateShown);
    void SetCursorPosition(int index, string s);
}