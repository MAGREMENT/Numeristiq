using System.Collections.Generic;
using Model;
using Model.Core.Settings;
using Model.Core.Settings.Types;
using Model.Sudokus;
using Model.Sudokus.Player;
using Model.Utility;
using Model.Utility.Collections;

namespace DesktopApplication.Presenter;

public class Settings
{
    private readonly ISetting[] _settings;
    private readonly NamedListSpan<ISetting>[][] _collections;
    private readonly IRepository<Dictionary<string, SettingValue>> _repository;
    private readonly Dictionary<int, List<OnSettingChange>> _events = new();

    public Settings(IReadOnlyList<Theme> themes, IRepository<Dictionary<string, SettingValue>> repository)
    {
        _settings = new ISetting[]
        {
            new IntSetting("Theme", new NameListInteractionInterface(themes), -1),
            new BooleanSetting("Show same cell links"),
            new EnumSetting<LinkOffsetSidePriority>("Link offset side priority",null, LinkOffsetSidePriority.Any),
            new BooleanSetting("Unique solution", true),
            new IntSetting("Start angle", new SliderInteractionInterface(0, 360, 10), 0),
            new EnumSetting<RotationDirection>("Rotation direction", SpaceConverter.Instance, RotationDirection.ClockWise),
            new EnumSetting<SudokuStringFormat>("Copy default format", SpaceConverter.Instance, SudokuStringFormat.Grid),
            new BooleanSetting("Open copy dialog"),
            new EnumSetting<SudokuStringFormat>("Paste default format", SpaceConverter.Instance, SudokuStringFormat.Base32),
            new BooleanSetting("Open paste dialog"),
            new EnumSetting<SudokuLineFormatEmptyCellRepresentation>("Line format empty cell representation", SpaceConverter.Instance, SudokuLineFormatEmptyCellRepresentation.Shortcuts),
            new BooleanSetting("Convert solo candidate to given for grid format"),
            new EnumSetting<PossibilitiesLocation>("Main possibilities location", SpaceConverter.Instance, PossibilitiesLocation.Middle),
            new BooleanSetting("Test solution count for clue")
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
                new NamedListSpan<ISetting>("Solver", _settings, 3),
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
    
    public void Update()
    {
        Dictionary<string, SettingValue> toUpload = new();

        foreach (var setting in _settings)
        {
            toUpload.Add(setting.Name, setting.Get());
        }
        
        _repository.Upload(toUpload);
    }

    public void Set(int index, SettingValue value, bool checkValidity = true)
    {
        _settings[index].Set(value, checkValidity);
        FirePossibleEvents(index);
    }

    public void TrySet(string name, SettingValue value)
    {
        for(int i = 0; i < _settings.Length; i++)
        {
            var setting = _settings[i];
            if (setting.Name.Equals(name))
            {
                setting.Set(value);
                FirePossibleEvents(i);
                return;
            }
        }
    }
    
    public void AddEvent(SpecificSettings specific, OnSettingChange del)
    {
        if (!_events.TryGetValue((int)specific, out var list))
        {
            list = new List<OnSettingChange>();
            _events[(int)specific] = list;
        }

        list.Add(del);
    }

    public IReadOnlyList<NamedListSpan<ISetting>> GetCollection(SettingCollections collection) =>
        _collections[(int)collection];

    public ISetting GetSetting(SpecificSettings specific) => _settings[(int)specific];
    
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
    
    #region Private

    private void FirePossibleEvents(int index)
    {
        if (_events.TryGetValue(index, out var list))
        {
            foreach (var e in list) e(_settings[index].Get());
        }
    }

    #endregion
}

public enum SettingCollections
{
    WelcomeWindow,
    SudokuSolvePage,
    SudokuPlayPage,
    SudokuGeneratePage
}

public enum SpecificSettings
{
    Theme = 0,
    ShowSameCellLinks = 1,
    LinkOffsetSidePriority = 2,
    AllowUniqueness = 3,
    StartAngle = 4,
    RotationDirection = 5,
    MainLocation = 12
}

public delegate void OnSettingChange(SettingValue setting);