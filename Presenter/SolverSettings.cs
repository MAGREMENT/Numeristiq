using Global.Enums;

namespace Presenter;

public class SolverSettings
{
    private StateShown _showState = StateShown.Before;
    private SudokuTranslationType _translationType = SudokuTranslationType.Shortcuts;
    private bool _uniquenessAllowed = true;

    public event OnShowStateChange? ShownStateChanged;
    public event OnTranslationTypeChange? TranslationTypeChanged;
    public event OnUniquenessAllowedChange? UniquenessAllowedChanged;
    public event OnGivensNeedUpdate? GivensNeedUpdate;
    
    public bool StepByStep { get; set; } = true;
    public StateShown StateShown
    {
        get => _showState;
        set
        {
            _showState = value;
            ShownStateChanged?.Invoke();
        }
    }
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
    public ChangeType ActionOnCellChange { get; set; } = ChangeType.Solution;
    public bool TransformSoloPossibilityIntoGiven { get; set; } = true;

    public void NotifyGivensNeedingUpdate()
    {
        GivensNeedUpdate?.Invoke();
    }
}

public delegate void OnShowStateChange();
public delegate void OnTranslationTypeChange();
public delegate void OnUniquenessAllowedChange();
public delegate void OnGivensNeedUpdate();