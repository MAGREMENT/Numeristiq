using System.Collections;
using System.Collections.Generic;
using System.Text;
using Model.Helpers.Settings;
using Model.Helpers.Settings.Types;
using Model.Sudoku.Solver;
using Model.Utility;

namespace DesktopApplication.Presenter.Sudoku.Manage;

public class StrategySettingsPresenter : IEnumerable<(ISetting, int)>, ISettingCollection
{
    private readonly SudokuStrategy _strategy;
    private readonly IStrategyRepositoryUpdater _updater;
    private readonly SpaceConverter _converter = new();

    public StrategySettingsPresenter(SudokuStrategy strategy, IStrategyRepositoryUpdater updater)
    {
        _strategy = strategy;
        _updater = updater;
    }

    public IEnumerator<(ISetting, int)> GetEnumerator()
    {
        yield return (new BooleanSetting("Enabled", _strategy.Enabled), -1);
        yield return (new EnumSetting<InstanceHandling>("Instance handling", _converter, _strategy.InstanceHandling), -2);
        for (int i = 0; i < _strategy.Settings.Count; i++)
        {
            yield return (_strategy.Settings[i], i);
        }
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    public void Set(int index, SettingValue value)
    {
        switch (index)
        {
            case -1:
                _strategy.Enabled = value.ToBool();
                break;
            case -2:
                _strategy.InstanceHandling = (InstanceHandling)value.ToInt();
                break;
            default:
                _strategy.Set(index, value);
                break;
        }

        _updater.Update();
    }
}

public class SpaceConverter : IStringConverter
{
    private readonly StringBuilder _builder = new();
    
    public string Convert(string s)
    {
        if (s.Length is 0 or 1) return s;
        
        _builder.Clear();
        _builder.Append(s[0]);

        for (int i = 1; i < s.Length; i++)
        {
            var c = s[i];
            if (char.IsUpper(c))
            {
                _builder.Append(' ');
                _builder.Append(char.ToLower(c));
            }
            else _builder.Append(c);
        }

        return _builder.ToString();
    }
}