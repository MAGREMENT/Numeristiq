using System.Collections;
using System.Collections.Generic;
using Model.Core;
using Model.Core.Settings;
using Model.Core.Settings.Types;
using Model.Repositories;
using Model.Sudokus.Solver;
using Model.Utility;

namespace DesktopApplication.Presenter.Sudokus.Manage;

public class StrategySettingsPresenter : IEnumerable<(ISetting, int)>, ISettingCollection
{
    private readonly SudokuStrategy _strategy;
    private readonly IStrategyRepository<SudokuStrategy> _repo;

    public StrategySettingsPresenter(SudokuStrategy strategy, IStrategyRepository<SudokuStrategy> repo)
    {
        _strategy = strategy;
        _repo = repo;
    }

    public IEnumerator<(ISetting, int)> GetEnumerator()
    {
        yield return (new BooleanSetting("Enabled", "Is the strategy enabled", _strategy.Enabled), -1);
        yield return (new EnumSetting<InstanceHandling>("Instance handling", "Defines the way different instances" +
                                                                             "of the same strategy during the same search are handled",
            SpaceConverter.Instance, _strategy.InstanceHandling), -2);
        int i = 0;
        foreach (var setting in _strategy.EnumerateSettings())
        {
            yield return (setting, i++);
        }
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    public void Set(int index, SettingValue value, bool checkValidity)
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
                _strategy.Set(index, value, checkValidity);
                break;
        }

        _repo.UpdateStrategy(_strategy);
    }
}

