using System.Collections.Generic;
using Model.Helpers.Descriptions;
using Model.Sudokus.Solver;

namespace DesktopApplication.Presenter.Sudokus.Manage;

public interface ISudokuManageView
{
    public void ClearSearchResults();
    public void AddSearchResult(string s);
    public void SetStrategyList(IReadOnlyList<SudokuStrategy> list);
    public void SetSelectedStrategyName(string name);
    public void SetManageableSettings(StrategySettingsPresenter presenter);
    public void SetStrategyDescription(IDescription description);
    public void ClearSelectedStrategy();
}