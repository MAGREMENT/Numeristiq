using Model.Core.Settings;
using Model.Sudokus.Generator;

namespace DesktopApplication.Presenter.Sudokus.Generate;

public class ManageCriteriaPresenter
{
    private readonly IManageCriteriaView _view;
    private readonly SudokuEvaluator _evaluator;

    private EvaluationCriteria? _currentlySelectedCriteria;
    private bool _isCurrentlySelectedCriteriaInEvaluator;
    
    public ManageCriteriaPresenter(IManageCriteriaView view, SudokuEvaluator evaluator)
    {
        _view = view;
        _evaluator = evaluator;
        _evaluator.TakeSnapShot();

        foreach (var criteria in CriteriaPool.EnumerateCriterias())
        {
            _view.AddSearchResult(criteria);
        }

        _view.SetCriteriaList(_evaluator.Criterias);
    }

    public void SelectCriteriaFromSearch(string s)
    {
        var criteria = CriteriaPool.CreateFrom(s, _evaluator.GetUsedStrategiesName());
        if (criteria is null) return;

        if (_currentlySelectedCriteria is not null)
            _currentlySelectedCriteria.SettingUpdated -= UpdateSettingOfCurrentCriteria;
        
        _currentlySelectedCriteria = criteria;
        _currentlySelectedCriteria.SettingUpdated += UpdateSettingOfCurrentCriteria;
        
        UpdateCriteriaAction(_evaluator.Criterias.Contains(criteria));
        
        _view.SetSelectedCriteria(criteria);
    }

    public void SelectCriteriaFromList(int index)
    {
        if (index < 0 || index >= _evaluator.Criterias.Count) return;
        var criteria = _evaluator.Criterias[index];
        
        if (_currentlySelectedCriteria is not null)
            _currentlySelectedCriteria.SettingUpdated -= UpdateSettingOfCurrentCriteria;
        
        _currentlySelectedCriteria = criteria;
        _currentlySelectedCriteria.SettingUpdated += UpdateSettingOfCurrentCriteria;
        
        UpdateCriteriaAction(_evaluator.Criterias.Contains(criteria));

        _view.SetSelectedCriteria(criteria);
    }

    public void DoCriteriaAction()
    {
        if (_currentlySelectedCriteria is null) return;

        if (_isCurrentlySelectedCriteriaInEvaluator)
        {
            _evaluator.Criterias.Remove(_currentlySelectedCriteria);
        }
        else
        {
            _evaluator.Criterias.Add(_currentlySelectedCriteria);
        }

        UpdateCriteriaAction(!_isCurrentlySelectedCriteriaInEvaluator);
        
        _view.SetCriteriaList(_evaluator.Criterias);
    }

    public void CancelChanges()
    {
        _evaluator.RestoreSnapShot();
    }

    private void UpdateSettingOfCurrentCriteria(IReadOnlySetting setting)
    {
        if (_currentlySelectedCriteria is null) return;

        UpdateCriteriaAction(_evaluator.Criterias.Contains(_currentlySelectedCriteria));
        var index = _evaluator.Criterias.IndexOf(_currentlySelectedCriteria);
        if (index == -1) return;
        
        _view.UpdateCriteriaSettings(index, _currentlySelectedCriteria.Settings);
    }

    private void UpdateCriteriaAction(bool isInEvaluator)
    {
        _isCurrentlySelectedCriteriaInEvaluator = isInEvaluator;
        _view.SetButtonAction(!_isCurrentlySelectedCriteriaInEvaluator);
    }
}