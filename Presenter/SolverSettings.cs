using Global;
using Global.Enums;
using Model;
using Model.Solver;

namespace Presenter;

public class SolverSettings
{
    private SudokuTranslationType _translationType = SudokuTranslationType.Shortcuts;
    private bool _uniquenessAllowed = true;
    private OnInstanceFound _onInstanceFound = OnInstanceFound.Default;

    public event OnTranslationTypeChange? TranslationTypeChanged;
    public event OnUniquenessAllowedChange? UniquenessAllowedChanged;
    public event OnInstanceFoundChange? OnInstanceFoundChanged;
    public event OnGivensNeedUpdate? GivensNeedUpdate;
    
    public bool StepByStep { get; set; } = true;
    public StateShown StateShown { get; set; } = StateShown.Before;
    public SudokuTranslationType TranslationType
    {
        get => _translationType;
        set
        {
            _translationType = value;
            TranslationTypeChanged?.Invoke();
        }
    }
    public int DelayBeforeTransition { get; set; } = 350;
    public int DelayAfterTransition { get; set; } = 350;
    public bool UniquenessAllowed
    {
        get => _uniquenessAllowed;
        set
        {
            _uniquenessAllowed = value;
            UniquenessAllowedChanged?.Invoke();
        }
    }
    public OnInstanceFound OnInstanceFound
    {
        get => _onInstanceFound;
        set
        {
            _onInstanceFound = value;
            OnInstanceFoundChanged?.Invoke();
        }
    }
    public ChangeType ActionOnCellChange { get; set; } = ChangeType.Solution;

    public void NotifyGivensNeedingUpdate()
    {
        GivensNeedUpdate?.Invoke();
    }
}

public delegate void OnTranslationTypeChange();
public delegate void OnUniquenessAllowedChange();
public delegate void OnInstanceFoundChange();
public delegate void OnGivensNeedUpdate();