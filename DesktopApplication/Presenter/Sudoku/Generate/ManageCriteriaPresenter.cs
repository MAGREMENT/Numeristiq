using Model.Helpers.Settings;
using Model.Sudoku.Generator;
using Model.Utility.Collections;

namespace DesktopApplication.Presenter.Sudoku.Generate;

public class ManageCriteriaPresenter
{
    private readonly IManageCriteriaView _view;
    private readonly UniqueList<EvaluationCriteria> _criterias;
    private readonly IManageCriteriaCallback _callback;

    private EvaluationCriteria? _currentlySelectedCriteria;
    private bool _isCurrentlySelectedCriteriaInEvaluator;
    
    public ManageCriteriaPresenter(IManageCriteriaView view, UniqueList<EvaluationCriteria> criterias,
        IManageCriteriaCallback callback)
    {
        _view = view;
        _criterias = criterias;
        _callback = callback;

        foreach (var criteria in CriteriaPool.EnumerateCriterias())
        {
            _view.AddSearchResult(criteria);
        }

        _view.SetCriteriaList(_criterias);
    }

    public void SelectCriteriaFromSearch(string s)
    {
        var criteria = CriteriaPool.CreateFrom(s);
        if (criteria is null) return;

        if (_currentlySelectedCriteria is not null)
            _currentlySelectedCriteria.SettingUpdated -= UpdateSettingOfCurrentCriteria;
        
        _currentlySelectedCriteria = criteria;
        _currentlySelectedCriteria.SettingUpdated += UpdateSettingOfCurrentCriteria;
        _isCurrentlySelectedCriteriaInEvaluator = _criterias.Contains(criteria);

        _view.SetSelectedCriteria(criteria);
        _view.SetButtonAction(!_isCurrentlySelectedCriteriaInEvaluator);
    }

    public void SelectCriteriaFromList(int index)
    {
        if (index < 0 || index >= _criterias.Count) return;
        var criteria = _criterias[index];
        
        if (_currentlySelectedCriteria is not null)
            _currentlySelectedCriteria.SettingUpdated -= UpdateSettingOfCurrentCriteria;
        
        _currentlySelectedCriteria = criteria;
        _currentlySelectedCriteria.SettingUpdated += UpdateSettingOfCurrentCriteria;
        _isCurrentlySelectedCriteriaInEvaluator = true;

        _view.SetSelectedCriteria(criteria);
        _view.SetButtonAction(!_isCurrentlySelectedCriteriaInEvaluator);
    }

    public void DoCriteriaAction()
    {
        if (_currentlySelectedCriteria is null) return;

        if (_isCurrentlySelectedCriteriaInEvaluator)
        {
            _criterias.Remove(_currentlySelectedCriteria);
        }
        else
        {
            _criterias.Add(_currentlySelectedCriteria);
        }

        _isCurrentlySelectedCriteriaInEvaluator = !_isCurrentlySelectedCriteriaInEvaluator;
        
        _view.SetButtonAction(!_isCurrentlySelectedCriteriaInEvaluator);
        _view.SetCriteriaList(_criterias);
    }

    public void Save()
    {
        _callback.SetCriterias(_criterias);
    }

    private void UpdateSettingOfCurrentCriteria(IReadOnlySetting setting)
    {
        if (_currentlySelectedCriteria is null) return;

        var index = _criterias.IndexOf(_currentlySelectedCriteria);
        if (index == -1) return;
        
        _view.UpdateCriteriaSettings(index, _currentlySelectedCriteria.Settings);
    }
}