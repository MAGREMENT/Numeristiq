using System.Collections;
using System.Collections.Generic;
using Model;
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
        yield return (new BooleanSetting("Enabled", _strategy.Enabled), -1);
        yield return (new EnumSetting<InstanceHandling>("Instance handling",SpaceConverter.Instance, _strategy.InstanceHandling), -2);
        for (int i = 0; i < _strategy.Settings.Count; i++)
        {
            yield return (_strategy.Settings[i], i);
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

