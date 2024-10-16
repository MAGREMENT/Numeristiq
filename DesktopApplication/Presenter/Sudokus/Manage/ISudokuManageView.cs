﻿using System.Collections.Generic;
using System.IO;
using Model.Core.Descriptions;
using Model.Sudokus.Solver;
using Model.Sudokus.Solver.Descriptions;

namespace DesktopApplication.Presenter.Sudokus.Manage;

public interface ISudokuManageView
{
    public void ClearSearchResults();
    public void AddSearchResult(string s);
    public void SetStrategyList(IReadOnlyList<SudokuStrategy> list);
    public void SetSelectedStrategyName(string name);
    public void SetManageableSettings(StrategySettingsPresenter presenter);
    public void SetNotFoundSettings();
    public void SetStrategyDescription(IDescription<ISudokuDescriptionDisplayer> description);
    public void ClearSelectedStrategy();
    Stream? GetUploadPresetStream();
    Stream? GetDownloadPresetStream();
}