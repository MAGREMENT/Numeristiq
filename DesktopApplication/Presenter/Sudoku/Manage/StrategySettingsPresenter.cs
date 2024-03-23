using System.Collections;
using System.Collections.Generic;
using Model.Helpers.Settings;
using Model.Helpers.Settings.Types;
using Model.Sudoku.Solver;

namespace DesktopApplication.Presenter.Sudoku.Manage;

public class StrategySettingsPresenter : IEnumerable<(ISetting, int)>, ISettingCollection
{
    private readonly SudokuStrategy _strategy;
    private readonly IStrategyRepositoryUpdater _updater;

    public StrategySettingsPresenter(SudokuStrategy strategy, IStrategyRepositoryUpdater updater)
    {
        _strategy = strategy;
        _updater = updater;
    }

    public IEnumerator<(ISetting, int)> GetEnumerator()
    {
        yield return (new BooleanSetting("Enabled", _strategy.Enabled), -1);
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
        if (index == -1) _strategy.Enabled = value.ToBool();
        else _strategy.Set(index, value);
        _updater.Update();
    }
}