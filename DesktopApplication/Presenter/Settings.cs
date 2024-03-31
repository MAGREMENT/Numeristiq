using System.Collections.Generic;
using DesktopApplication.Presenter.Sudoku.Manage;
using Model;
using Model.Helpers.Settings;
using Model.Helpers.Settings.Types;
using Model.Utility;

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
            new EnumSetting<RotationDirection>("Rotation direction", new SpaceConverter(), RotationDirection.ClockWise)
        };
        _collections = new[]
        {
            new[] {new NamedListSpan<ISetting>("Themes", _settings, 0)},
            new[]
            {
                new NamedListSpan<ISetting>("Themes", _settings, 0),
                new NamedListSpan<ISetting>("Board", _settings, 1, 2),
                new NamedListSpan<ISetting>("Solver", _settings, 3)
            },
            new[]
            {
                new NamedListSpan<ISetting>("Themes", _settings, 0),
                new NamedListSpan<ISetting>("Highlighting", _settings, 4, 5)
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

    public void Set(int index, SettingValue value)
    {
        _settings[index].Set(value);
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
    SudokuPlayPage
}

public enum SpecificSettings
{
    Theme = 0,
    ShowSameCellLinks = 1,
    LinkOffsetSidePriority = 2,
    AllowUniqueness = 3
}

public delegate void OnSettingChange(SettingValue setting);