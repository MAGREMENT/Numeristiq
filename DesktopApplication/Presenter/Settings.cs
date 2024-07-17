using System.Collections.Generic;
using Model.Core.Settings;
using Model.Core.Settings.Types;
using Model.Repositories;
using Model.Sudokus;
using Model.Sudokus.Player;
using Model.Utility;
using Model.Utility.Collections;

namespace DesktopApplication.Presenter;

public class Settings
{
    private readonly ISetting[] _settings;
    private readonly NamedListSpan<ISetting>[][] _collections;
    private readonly ISettingRepository _repository;

    public Settings(IReadOnlyList<Theme> themes, ISettingRepository repository)
    {
        _settings = new ISetting[]
        {
            new IntSetting("Theme", "The theme of the application.", new NameListInteractionInterface(themes), -1),
            new BooleanSetting("Show same cell links", "Allows links between candidates of the same cell to be shown."),
            new EnumSetting<LinkOffsetSidePriority>("Link offset side priority", "Defines which side will be prioritized" +
                " for the angle of links.", null, LinkOffsetSidePriority.Any),
            new BooleanSetting("Unique solution", "The puzzle must have a unique solution. Allows additional strategies."
                , true),
            new IntSetting("Start angle", "Start angle for cells multi-color highlighting.",
                new SliderInteractionInterface(0, 360, 10), 0),
            new EnumSetting<RotationDirection>("Rotation direction", "Rotation direction for cells multi-color highlighting.",
                SpaceConverter.Instance, RotationDirection.ClockWise),
            new EnumSetting<SudokuStringFormat>("Copy default format", "The default format used for copying.",
                SpaceConverter.Instance, SudokuStringFormat.Grid),
            new BooleanSetting("Open copy dialog", "Opens a dialog window with options when a copy is asked."),
            new EnumSetting<SudokuStringFormat>("Paste default format", "The default format used for pasting.",
                SpaceConverter.Instance, SudokuStringFormat.Base32),
            new BooleanSetting("Open paste dialog", "Opens a dialog window with options when a paste is asked."),
            new EnumSetting<SudokuLineFormatEmptyCellRepresentation>("Line format empty cell representation",
                "Defines how empty cells are represented when translating a Sudoku to text",
                SpaceConverter.Instance, SudokuLineFormatEmptyCellRepresentation.Shortcuts),
            new BooleanSetting("Convert solo candidate to given for grid format", "Tells the parser to" +
                " convert cells with only one candidates to be converted to a solution with the grid format."),
            new EnumSetting<PossibilitiesLocation>("Main possibilities location",
                "Sets the main possibilities location, used for various operation like computing possibilities."
                ,SpaceConverter.Instance, PossibilitiesLocation.Middle),
            new BooleanSetting("Test solution count for clue", "When asking for a clue, verifies that the" +
                                                               " Sudoku has indeed a solution."),
            new BooleanSetting("Fast possibility display", "Display the Sudoku's possibilities in a" +
                                                           " faster but less elegant way.")
        };
        _collections = new[]
        {
            new[] //WelcomeView
            {
                new NamedListSpan<ISetting>("Themes", _settings, 0)
            },
            new[] //SudokuSolverView
            {
                new NamedListSpan<ISetting>("Themes", _settings, 0),
                new NamedListSpan<ISetting>("Board", _settings, 1, 2),
                new NamedListSpan<ISetting>("Solver", _settings, 3, 14),
                new NamedListSpan<ISetting>("Editing", _settings, 6, 7, 8, 9, 10, 11)
            },
            new[] //SudokuPlayerView
            {
                new NamedListSpan<ISetting>("Themes", _settings, 0),
                new NamedListSpan<ISetting>("Highlighting", _settings, 4, 5),
                new NamedListSpan<ISetting>("Player", _settings, 12, 13),
                new NamedListSpan<ISetting>("Editing", _settings, 8, 9, 11)
            },
            new [] //SudokuGenerateView
            {
                new NamedListSpan<ISetting>("Themes", _settings, 0),
            }
        };
        _repository = repository;
    }
    
    public void Update(IEnumerable<ISetting> settings)
    {
        _repository.UpdateSettings(settings);
    }

    public void Set(int index, SettingValue value, bool checkValidity = true, bool update = true)
    {
        var setting = _settings[index];
        
        _settings[index].Set(value, checkValidity);
        if (update) _repository.UpdateSetting(setting);
    }

    public void TrySet(string name, SettingValue value)
    {
        foreach (var setting in _settings)
        {
            if (setting.Name.Equals(name))
            {
                setting.Set(value);
                return;
            }
        }
    }

    public IReadOnlyList<NamedListSpan<ISetting>> GetCollection(SettingCollections collection) =>
        _collections[(int)collection];

    public ISetting ThemeSetting => _settings[0];
    public ISetting ShowSameCellsLinksSetting => _settings[1];
    public ISetting LinkOffsetSidePrioritySetting => _settings[2];
    public ISetting AllowUniquenessSetting => _settings[3];
    public ISetting StartAngleSetting => _settings[4];
    public ISetting RotationDirectionSetting => _settings[5];
    public ISetting MainLocationSetting => _settings[12];
    public ISetting FastPossibilityDisplaySetting => _settings[14];
    
    //TODO to ISetting
    public int Theme => _settings[0].Get().ToInt();
    public bool ShowSameCellLinks => _settings[1].Get().ToBool();
    public LinkOffsetSidePriority LinkOffsetSidePriority => ((EnumSetting<LinkOffsetSidePriority>)_settings[2]).Value;
    public int StartAngle => _settings[4].Get().ToInt();
    public RotationDirection RotationDirection => ((EnumSetting<RotationDirection>)_settings[5]).Value;
    public SudokuStringFormat DefaultCopyFormat => ((EnumSetting<SudokuStringFormat>)_settings[6]).Value;
    public bool OpenCopyDialog => _settings[7].Get().ToBool();
    public SudokuStringFormat DefaultPasteFormat => ((EnumSetting<SudokuStringFormat>)_settings[8]).Value;
    public bool OpenPasteDialog => _settings[9].Get().ToBool();
    public SudokuLineFormatEmptyCellRepresentation EmptyCellRepresentation =>
        ((EnumSetting<SudokuLineFormatEmptyCellRepresentation>)_settings[10]).Value;
    public bool SoloToGiven => _settings[11].Get().ToBool();
    public PossibilitiesLocation MainLocation => ((EnumSetting<PossibilitiesLocation>)_settings[12]).Value;
    public bool TestSolutionCount => _settings[13].Get().ToBool();
    public bool FastPossibilityDisplay => _settings[14].Get().ToBool();
}

public enum SettingCollections
{
    WelcomeWindow,
    SudokuSolvePage,
    SudokuPlayPage,
    SudokuGeneratePage
}