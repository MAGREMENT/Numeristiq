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
                " for the angle of links.", null, DesktopApplication.Presenter.LinkOffsetSidePriority.Any),
            new BooleanSetting("Unique solution", "The puzzle must have a unique solution. Allows additional strategies."
                , true),
            new IntSetting("Start angle", "Start angle for cells multi-color highlighting.",
                new SliderInteractionInterface(0, 360, 10), 0),
            new EnumSetting<RotationDirection>("Rotation direction", "Rotation direction for cells multi-color highlighting.",
                SpaceConverter.Instance, DesktopApplication.Presenter.RotationDirection.ClockWise),
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
                " convert cells with only one candidates to a solution for the grid format."),
            new EnumSetting<PossibilitiesLocation>("Main possibilities location",
                "Sets the main possibilities location, used for various operation like computing possibilities."
                ,SpaceConverter.Instance, PossibilitiesLocation.Middle),
            new BooleanSetting("Test solution count for clue", "When asking for a clue, verifies that the" +
                                                               " Sudoku has indeed a solution."),
            new BooleanSetting("Fast possibility display", "Display the Sudoku's possibilities in a" +
                                                           " faster but less elegant way."),
            new BooleanSetting("Are solution numbers", "Defines if the solutions should be represented as " +
                                                       "numbers")
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
            },
            new[] //BinairoSolveView
            {
                new NamedListSpan<ISetting>("Themes", _settings, 0),
                new NamedListSpan<ISetting>("Board", _settings, 15)
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

    public void TrySet(string name, SettingValue value, bool checkValidity = true, bool update = true)
    {
        foreach (var setting in _settings)
        {
            if (!setting.Name.Equals(name)) continue;
            
            setting.Set(value, checkValidity);
            if (update) _repository.UpdateSetting(setting);
            return;
        }
    }

    public IReadOnlyList<NamedListSpan<ISetting>> GetCollection(SettingCollections collection) =>
        _collections[(int)collection];

    public ISetting Theme => _settings[0];
    public ISetting ShowSameCellLinks => _settings[1];
    public ISetting LinkOffsetSidePriority => _settings[2];
    public ISetting AllowUniqueness => _settings[3];
    public ISetting StartAngle => _settings[4];
    public ISetting RotationDirection => _settings[5];
    public ISetting DefaultCopyFormat => _settings[6];
    public ISetting OpenCopyDialog => _settings[7];
    public ISetting DefaultPasteFormat => _settings[8];
    public ISetting OpenPasteDialog => _settings[9];
    public ISetting EmptyCellRepresentation => _settings[10];
    public ISetting SoloToGiven => _settings[11];
    public ISetting MainLocation => _settings[12];
    public ISetting TestSolutionCount => _settings[13];
    public ISetting FastPossibilityDisplay => _settings[14];
    public ISetting AreSolutionNumbers => _settings[15];
}

public enum SettingCollections
{
    WelcomeWindow,
    SudokuSolvePage,
    SudokuPlayPage,
    SudokuGeneratePage,
    BinairoSolvePage
}

public enum RotationDirection
{
    ClockWise = 1, CounterClockWise = -1
}

public enum LinkOffsetSidePriority
{
    Any, Left, Right
}