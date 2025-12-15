using System.Collections.Generic;
using Model.Core.Highlighting;
using Model.Core.Steps;

namespace DesktopApplication.Presenter;

public abstract class StepManagingPresenter<THighlight, TStep, TState> : IStepManagingPresenter
    where TStep : IStep<THighlight, TState>
{
    protected readonly IHighlighterTranslator<THighlight> _translator;
    protected int _currentlyOpenedStep = -1;
    protected StepState _stepState = StepState.From;
    protected int _stepCount;
    
    protected abstract IReadOnlyList<TStep> Steps { get; }
    protected abstract IStepManagingView View { get; }

    protected StepManagingPresenter(IHighlighterTranslator<THighlight> translator)
    {
        _translator = translator;
    }

    public virtual int GetViewIndex(int ind)
    {
        return ind;
    }
    
    public virtual void UpdateSteps()
    {
        if (Steps.Count < _stepCount)
        {
            ClearSteps();
            return;
        }

        for (;_stepCount < Steps.Count; _stepCount++)
        {
            View.AddStep(_stepCount + 1, Steps[_stepCount], _stepState);
        }
    }

    public virtual void RequestStepOpening(int id)
    {
        var index = id - 1;
        if (index < 0 || index >= Steps.Count) return;
        
        if(_currentlyOpenedStep != -1) View.CloseStep(GetViewIndex(_currentlyOpenedStep));

        if (_currentlyOpenedStep == index)
        {
            _currentlyOpenedStep = -1;
            SetShownState(GetCurrentState(), false, true);
        }
        else
        {
            _currentlyOpenedStep = index;
            View.OpenStep(GetViewIndex(_currentlyOpenedStep));

            var log = Steps[_currentlyOpenedStep];
            SetShownState(_stepState == StepState.From ? log.From : log.To, false, true); 
            _translator.Translate(log.HighlightCollection, false); 
        }
    }

    public void RequestStateShownChange(StepState ss)
    {
        _stepState = ss;
        View.SetStepsStateShown(ss);
        if (_currentlyOpenedStep < 0 || _currentlyOpenedStep >= Steps.Count) return;
        
        var log = Steps[_currentlyOpenedStep];
        SetShownState(_stepState == StepState.From ? log.From : log.To, false, true); 
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

public interface IStepManagingPresenter
{
    public void RequestStepOpening(int id);
    public void RequestStateShownChange(StepState ss);
    public void RequestHighlightChange(int newHighlight);
    public IStepExplanationPresenterBuilder? RequestExplanation();
}

public interface IHighlighterTranslator<out T>
{
    void Translate(IHighlightable<T> highlightable, bool clear);
}

public interface IStepManagingView
{
    void AddStep(int id, IStep step, StepState shown);
    void ClearSteps();
    void OpenStep(int index);
    void CloseStep(int index);
    void SetStepsStateShown(StepState stepState);
}