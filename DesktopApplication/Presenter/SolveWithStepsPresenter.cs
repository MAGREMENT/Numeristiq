using System.Collections.Generic;
using DesktopApplication.Presenter.Sudokus.Solve;
using Model.Core.Highlighting;
using Model.Core.Steps;

namespace DesktopApplication.Presenter;

public abstract class SolveWithStepsPresenter<THighlight, TStep, TState> : ISolveWithStepsPresenter
    where TStep : IStep<THighlight, TState>
{
    protected readonly IHighlighterTranslator<THighlight> _translator;
    protected int _currentlyOpenedStep = -1;
    private int _stepCount;
    private StateShown _stateShown = StateShown.Before;
    
    protected abstract IReadOnlyList<TStep> Steps { get; }
    protected abstract ISolveWithStepsView View { get; }

    protected SolveWithStepsPresenter(IHighlighterTranslator<THighlight> translator)
    {
        _translator = translator;
    }
    
    public void UpdateSteps()
    {
        if (Steps.Count < _stepCount)
        {
            ClearSteps();
            return;
        }

        for (;_stepCount < Steps.Count; _stepCount++)
        {
            View.AddStep(Steps[_stepCount], _stateShown);
        }
    }

    public void RequestStepOpening(int id)
    {
        var index = id - 1;
        if (index < 0 || index >= Steps.Count) return;
        
        if(_currentlyOpenedStep != -1) View.CloseStep(_currentlyOpenedStep);

        if (_currentlyOpenedStep == index)
        {
            _currentlyOpenedStep = -1;
            SetShownState(GetCurrentState(), false, true);
        }
        else
        {
            View.OpenStep(index);
            _currentlyOpenedStep = index;

            var log = Steps[index];
            SetShownState(_stateShown == StateShown.Before ? log.From : log.To, false, true); 
            _translator.Translate(log.HighlightCollection, false); 
        }
    }

    public void RequestStateShownChange(StateShown ss)
    {
        _stateShown = ss;
        View.SetStepsStateShown(ss);
        if (_currentlyOpenedStep < 0 || _currentlyOpenedStep >= Steps.Count) return;
        
        var log = Steps[_currentlyOpenedStep];
        SetShownState(_stateShown == StateShown.Before ? log.From : log.To, false, true); 
        _translator.Translate(log.HighlightCollection, false);
    }

    public void RequestHighlightChange(int newHighlight)
    {
        if (_currentlyOpenedStep < 0 || _currentlyOpenedStep >= Steps.Count) return;
        
        var log = Steps[_currentlyOpenedStep];
        log.HighlightCollection.GoTo(newHighlight - 1);
        
        _translator.Translate(log.HighlightCollection, true);
    }

    public abstract IStepExplanationPresenterBuilder? RequestExplanation();
    protected abstract void SetShownState(TState numericSolvingState, bool solutionAsClues, bool showPossibilities);
    protected abstract TState GetCurrentState();
    
    protected void ClearSteps()
    {
        View.ClearSteps();
        _stepCount = 0;
    }
}

public interface ISolveWithStepsPresenter
{
    public void RequestStepOpening(int id);
    public void RequestStateShownChange(StateShown ss);
    public void RequestHighlightChange(int newHighlight);
    public IStepExplanationPresenterBuilder? RequestExplanation();
}

public interface IHighlighterTranslator<out T>
{
    void Translate(IHighlightable<T> highlightable, bool clear);
}

public interface ISolveWithStepsView
{
    void AddStep(IStep step, StateShown _shown);
    void ClearSteps();
    void OpenStep(int index);
    void CloseStep(int index);
    void SetStepsStateShown(StateShown stateShown);
}

public enum StateShown
{
    Before, After
}