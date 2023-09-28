using Model;
using Model.Solver;
using Model.Solver.Helpers.Logs;

namespace SudokuSolver.Utils;

public class SolverStateManager
{
    private readonly IGraphicsManager _gm;
    private readonly ISolverGraphics _sg;
    private readonly ILogListGraphics _llg;

    public SolverStateManager(IGraphicsManager gm, ISolverGraphics sg, ILogListGraphics llg)
    {
        _gm = gm;
        _sg = sg;
        _llg = llg;

        _sg.LogShowed += LogShowed;
        _sg.CurrentStateShowed += CurrentStateShowed;
        _llg.ShowLogAsked += ShowLogAsked;
        _llg.ShowStartAsked += ShowStartAsked;
        _llg.ShowCurrentAsked += ShowCurrentAsked;
    }

    private void LogShowed(ISolverLog log)
    {
        _llg.FocusLog(log);
        _gm.ShowSudokuAsString(Solver.StateToSudokuString(log.SolverState, _sg.TranslationType));
    }

    private void CurrentStateShowed()
    {
        _llg.UnFocusLog();
        _gm.ShowSudokuAsString(Solver.StateToSudokuString(_sg.CurrentState, _sg.TranslationType));
    }

    private void ShowLogAsked(ISolverLog log)
    {
        _sg.ShowState(log.SolverState);
        _sg.HighLightLog(log);
        _gm.ShowSudokuAsString(Solver.StateToSudokuString(log.SolverState, _sg.TranslationType));
    }

    private void ShowStartAsked()
    {
        _sg.ShowState(_sg.StartState);
        _gm.ShowSudokuAsString(Solver.StateToSudokuString(_sg.StartState, _sg.TranslationType));
    }
    
    private void ShowCurrentAsked()
    {
        _sg.ShowState(_sg.CurrentState);
        _gm.ShowSudokuAsString(Solver.StateToSudokuString(_sg.CurrentState, _sg.TranslationType));
    }
}

public interface IGraphicsManager
{
    void ShowSudokuAsString(string asString);
}

public interface ISolverGraphics
{
    SudokuTranslationType TranslationType { get; }
    string StartState { get; }
    string CurrentState { get; }

    public event OnLogShowed? LogShowed;
    public event OnCurrentStateShowed? CurrentStateShowed;

    void HighLightLog(ISolverLog log);
    void ShowState(string state);
}

public delegate void OnLogShowed(ISolverLog log);

public delegate void OnCurrentStateShowed();

public interface ILogListGraphics
{
    public event OnShowLogAsked? ShowLogAsked;
    public event OnShowCurrentAsked? ShowCurrentAsked;
    public event OnShowStartAsked? ShowStartAsked;
    
    void FocusLog(ISolverLog log);
    void UnFocusLog();
}

public delegate void OnShowLogAsked(ISolverLog log);
public delegate void OnShowCurrentAsked();
public delegate void OnShowStartAsked();