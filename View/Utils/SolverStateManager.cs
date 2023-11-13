using Model;
using Model.Solver;
using Model.Solver.Helpers.Logs;

namespace View.Utils;

public class SolverStateManager
{
    private readonly IGraphicsManager _gm;
    private readonly ISolverGraphics _sg;
    private readonly ILogListGraphics _llg;

    private SolverState? _shownState;

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
        _gm.TranslationTypeChanged += TranslationTypeChanged;
    }

    private void LogShowed(ISolverLog log)
    {
        _shownState = log.StateAfter;
            
        _llg.FocusLog(log);
        _gm.ShowSudokuAsString(SudokuTranslator.Translate(log.StateAfter, _sg.TranslationType));
        _gm.ShowExplanation(log.Explanation);
    }

    private void CurrentStateShowed()
    {
        _shownState = _sg.CurrentState;
        
        _llg.UnFocusLog();
        _gm.ShowSudokuAsString(SudokuTranslator.Translate(_sg.CurrentState, _sg.TranslationType));
    }

    private void ShowLogAsked(ISolverLog log, StateShownType type)
    {
        _shownState = type == StateShownType.Before ? log.StateBefore : log.StateAfter;
        
        _sg.ShowState(_shownState);
        _sg.HighlightLog(log);
        _gm.ShowSudokuAsString(SudokuTranslator.Translate(_shownState, _sg.TranslationType));
        _gm.ShowExplanation(log.Explanation);
    }

    private void ShowStartAsked()
    {
        _shownState = _sg.StartState;
        
        _sg.ShowState(_sg.StartState);
        _gm.ShowSudokuAsString(SudokuTranslator.Translate(_sg.StartState, _sg.TranslationType));
    }
    
    private void ShowCurrentAsked()
    {
        _shownState = _sg.CurrentState;
            
        _sg.ShowCurrent();
        _gm.ShowSudokuAsString(SudokuTranslator.Translate(_sg.CurrentState, _sg.TranslationType));
    }

    private void TranslationTypeChanged()
    {
        _gm.ShowSudokuAsString(SudokuTranslator.Translate(_shownState ?? _sg.CurrentState, _sg.TranslationType));
    }
}

public interface IGraphicsManager
{
    void ShowSudokuAsString(string asString);
    void ShowExplanation(string explanation);

    public event OnTranslationTypeChanged? TranslationTypeChanged;
}

public delegate void OnTranslationTypeChanged();

public interface ISolverGraphics
{
    SudokuTranslationType TranslationType { get; }
    SolverState StartState { get; }
    SolverState CurrentState { get; }

    public event OnLogShowed? LogShowed;
    public event OnCurrentStateShowed? CurrentStateShowed;

    void HighlightLog(ISolverLog log);
    void ShowState(SolverState state);
    void ShowCurrent();
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

public delegate void OnShowLogAsked(ISolverLog log, StateShownType type);
public delegate void OnShowCurrentAsked();
public delegate void OnShowStartAsked();

public enum StateShownType
{
    Before, After
}