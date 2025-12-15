using System.Windows;
using System.Windows.Controls;
using DesktopApplication.Presenter;
using DesktopApplication.View.Controls;
using DesktopApplication.View.HelperWindows;
using Model.Core.Steps;

namespace DesktopApplication.View;

public interface IStepManagingView
{
    bool ScrollToBottomOnNewSteps { get; }
    StackPanel GetStepPanel();
    ScrollViewer GetStepViewer();
    IStepManagingPresenter GetStepsPresenter();
    ISizeOptimizable GetExplanationDrawer();
}

public static class IStepManagingViewHelper
{
    public static void AddStep(IStepManagingView view, int id, IStep step, StepState stepState)
    {
        view.GetStepPanel().Dispatcher.Invoke(() =>
        {
            var lc = new StepControl(id, step, stepState);
            view.GetStepPanel().Children.Add(lc);
            lc.OpenRequested += view.GetStepsPresenter().RequestStepOpening;
            lc.StateShownChanged += view.GetStepsPresenter().RequestStateShownChange;
            lc.PageSelector.PageChanged += view.GetStepsPresenter().RequestHighlightChange;
            lc.ExplanationAsked += () =>
            {
                var builder = view.GetStepsPresenter().RequestExplanation();
                if (builder is null) return;

                var window = new StepExplanationWindow(builder, view.GetExplanationDrawer());
                window.Show();
            };
        });
        
        if(view.ScrollToBottomOnNewSteps) view.GetStepViewer().Dispatcher.Invoke(() => view.GetStepViewer().ScrollToEnd());
    }

    public static void ClearSteps(IStepManagingView view)
    {
        view.GetStepPanel().Children.Clear();
    }

    public static void OpenStep(IStepManagingView view, int index)
    {
        if (index < 0 || index > view.GetStepPanel().Children.Count) return;
        if (view.GetStepPanel().Children[index] is not StepControl lc) return;
        
        lc.Open();
    }

    public static void CloseStep(IStepManagingView view, int index)
    {
        if (index < 0 || index > view.GetStepPanel().Children.Count) return;
        if (view.GetStepPanel().Children[index] is not StepControl lc) return;
        
        lc.Close();
    }

    public static void SetStepsStateShown(IStepManagingView view, StepState stepState)
    {
        foreach (var child in view.GetStepPanel().Children)
        {
            if (child is not StepControl lc) continue;

            lc.SetStateShown(stepState);
        }
    }
}

public abstract class StepManagingPage : ManagedPage, IStepManagingView
{
    public bool ScrollToBottomOnNewSteps => true;
    public abstract StackPanel GetStepPanel();
    public abstract ScrollViewer GetStepViewer();
    public abstract IStepManagingPresenter GetStepsPresenter();
    public abstract ISizeOptimizable GetExplanationDrawer();
    
    public void AddStep(int id, IStep step, StepState stepState)
    {
        IStepManagingViewHelper.AddStep(this, id, step, stepState);
    }

    public void ClearSteps()
    {
        IStepManagingViewHelper.ClearSteps(this);
    }

    public void OpenStep(int index)
    {
        IStepManagingViewHelper.OpenStep(this, index);
    }

    public void CloseStep(int index)
    {
        IStepManagingViewHelper.CloseStep(this, index);
    }

    public void SetStepsStateShown(StepState stepState)
    {
        IStepManagingViewHelper.SetStepsStateShown(this, stepState);
    }
}

public abstract class StepManagingWindow : Window, IStepManagingView
{
    public bool ScrollToBottomOnNewSteps => false;
    public abstract StackPanel GetStepPanel();
    public abstract ScrollViewer GetStepViewer();
    public abstract IStepManagingPresenter GetStepsPresenter();
    public abstract ISizeOptimizable GetExplanationDrawer();
    
    public void AddStep(int id, IStep step, StepState stepState)
    {
        IStepManagingViewHelper.AddStep(this, id, step, stepState);
    }

    public void ClearSteps()
    {
        IStepManagingViewHelper.ClearSteps(this);
    }

    public void OpenStep(int index)
    {
        IStepManagingViewHelper.OpenStep(this, index);
    }

    public void CloseStep(int index)
    {
        IStepManagingViewHelper.CloseStep(this, index);
    }

    public void SetStepsStateShown(StepState stepState)
    {
        IStepManagingViewHelper.SetStepsStateShown(this, stepState);
    }
}