using System.Collections.Generic;
using Model.Core.Settings;
using Model.Sudokus.Generator;

namespace DesktopApplication.Presenter.Sudokus.Generate;

public interface IManageCriteriaView
{
    public void AddSearchResult(string s);
    public void SetSelectedCriteria(EvaluationCriteria criteria);
    public void SetButtonAction(bool toAdd);
    public void SetCriteriaList(IReadOnlyList<EvaluationCriteria> criteriaList);
    public void UpdateCriteriaSettings(int index, IReadOnlyList<IReadOnlySetting> settings);
}