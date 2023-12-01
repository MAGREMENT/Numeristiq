using Global.Enums;
using Model;

namespace Presenter.Solver;

public class SolverSettings
{
    private bool _stepByStep = true;
    private StateShown _showState = StateShown.Before;
    private SudokuTranslationType _translationType = SudokuTranslationType.Shortcuts;
    private int _delayBeforeTransition = 350;
    private int _delayAfterTransition = 350;
    private bool _uniquenessAllowed = true;
    private ChangeType _actionOnCellChange = ChangeType.Solution;
    private bool _transformSoloPossibilityIntoGiven = true;
    private CellColor _givenColor = CellColor.Black;
    private CellColor _solvingColor = CellColor.Black;
    private LinkOffsetSidePriority _sidePriority = LinkOffsetSidePriority.Right;

    public event OnSettingChange? AnySettingChanged;
    public event OnSettingChange? ShownStateChanged;
    public event OnSettingChange? TranslationTypeChanged;
    public event OnSettingChange? UniquenessAllowedChanged;
    public event OnSettingChange? GivensNeedUpdate;
    
    public bool StepByStep
    {
        get => _stepByStep;
        set
        {
            _stepByStep = value;
            AnySettingChanged?.Invoke();
        } 
    }
    public StateShown StateShown
    {
        get => _showState;
        set
        {
            _showState = value;
            ShownStateChanged?.Invoke();
            AnySettingChanged?.Invoke();
        }
    }
    public SudokuTranslationType TranslationType
    {
        get => _translationType;
        set
        {
            _translationType = value;
            TranslationTypeChanged?.Invoke();
            AnySettingChanged?.Invoke();
        }
    }
    public int DelayBeforeTransition
    {
        get => _delayBeforeTransition;
        set
        {
            _delayBeforeTransition = value;
            AnySettingChanged?.Invoke();
        } 
    }
    public int DelayAfterTransition
    {
        get => _delayAfterTransition;
        set
        {
            _delayAfterTransition = value;
            AnySettingChanged?.Invoke();
        }
    }
    public bool UniquenessAllowed
    {
        get => _uniquenessAllowed;
        set
        {
            _uniquenessAllowed = value;
            UniquenessAllowedChanged?.Invoke();
            AnySettingChanged?.Invoke();
        }
    }
    public ChangeType ActionOnCellChange
    {
        get => _actionOnCellChange;
        set
        {
            _actionOnCellChange = value;
            AnySettingChanged?.Invoke();
        }
    }
    public bool TransformSoloPossibilityIntoGiven
    {
        get => _transformSoloPossibilityIntoGiven;
        set
        {
            _transformSoloPossibilityIntoGiven = value;
            AnySettingChanged?.Invoke();
        } 
    }
    public CellColor GivenColor
    {
        get => _givenColor;
        set
        {
            _givenColor = value;
            GivensNeedUpdate?.Invoke();
            AnySettingChanged?.Invoke();
        }
    }
    public CellColor SolvingColor
    {
        get => _solvingColor;
        set
        {
            _solvingColor = value;
            GivensNeedUpdate?.Invoke();
            AnySettingChanged?.Invoke();
        }
    }
    public LinkOffsetSidePriority SidePriority
    {
        get => _sidePriority;
        set
        {
            _sidePriority = value;
            AnySettingChanged?.Invoke();
        } 
    }

    
    private readonly IRepository<SettingsDAO> _repository;

    public SolverSettings(IRepository<SettingsDAO> repository)
    {
        _repository = repository;
    }
    
    public void Bind()
    {
        try
        {
            _repository.Initialize();
            var download = _repository.Download();
            if(download is not null) Load(download);
        }
        catch (RepositoryInitializationException)
        {
            _repository.New(ToDAO());
        }
        
        AnySettingChanged += () =>_repository.Upload(ToDAO());
    }
    
    private void Load(SettingsDAO DAO)
    {
        StepByStep = DAO.StepByStep;
        StateShown = DAO.StateShown;
        TranslationType = DAO.TranslationType;
        DelayBeforeTransition = DAO.DelayBeforeTransition;
        DelayAfterTransition = DAO.DelayAfterTransition;
        UniquenessAllowed = DAO.UniquenessAllowed;
        ActionOnCellChange = DAO.ActionOnCellChange;
        TransformSoloPossibilityIntoGiven = DAO.TransformSoloPossibilityIntoGiven;
        GivenColor = DAO.GivenColor;
        SolvingColor = DAO.SolvingColor;
        SidePriority = DAO.SidePriority;
    }

    private SettingsDAO ToDAO()
    {
        return new SettingsDAO(StepByStep, StateShown, TranslationType,
            DelayBeforeTransition, DelayAfterTransition, UniquenessAllowed,
            ActionOnCellChange, TransformSoloPossibilityIntoGiven,
            GivenColor, SolvingColor, SidePriority);
    }
}

public delegate void OnSettingChange();