using Global.Enums;
using Model;
using Presenter.Player;
using Presenter.Solver;

namespace Presenter;

public class Settings : ISolverSettings, IPlayerSettings
{
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
    private bool _showSameCellLinks;

    public event OnSettingChange? AnySettingChanged;
    public event OnSettingChange? ShownStateChanged;
    public event OnSettingChange? TranslationTypeChanged;
    public event OnSettingChange? UniquenessAllowedChanged;
    public event OnSettingChange? RedrawNeeded;

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
            RedrawNeeded?.Invoke();
            AnySettingChanged?.Invoke();
        }
    }
    public CellColor SolvingColor
    {
        get => _solvingColor;
        set
        {
            _solvingColor = value;
            RedrawNeeded?.Invoke();
            AnySettingChanged?.Invoke();
        }
    }
    public LinkOffsetSidePriority SidePriority
    {
        get => _sidePriority;
        set
        {
            _sidePriority = value;
            RedrawNeeded?.Invoke();
            AnySettingChanged?.Invoke();
        } 
    }
    public bool ShowSameCellLinks
    {
        get => _showSameCellLinks;
        set
        {
            _showSameCellLinks = value;
            RedrawNeeded?.Invoke();
            AnySettingChanged?.Invoke();
        }
    }

    public void Bind(IRepository<SettingsDAO> repository)
    {
        var download = repository.Download();
        if(download is not null) Load(download);
        
        AnySettingChanged += () => repository.Upload(ToDAO());
    }
    
    private void Load(SettingsDAO DAO)
    {
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
        ShowSameCellLinks = DAO.ShowSameCellLinks;
    }

    public SettingsDAO ToDAO()
    {
        return new SettingsDAO(StateShown, TranslationType,
            DelayBeforeTransition, DelayAfterTransition, UniquenessAllowed,
            ActionOnCellChange, TransformSoloPossibilityIntoGiven,
            GivenColor, SolvingColor, SidePriority, ShowSameCellLinks);
    }
}

public delegate void OnSettingChange();