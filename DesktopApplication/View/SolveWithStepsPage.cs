using System.Windows.Controls;
using DesktopApplication.Presenter;
using DesktopApplication.View.Controls;
using DesktopApplication.View.HelperWindows;
using Model.Core.Steps;

namespace DesktopApplication.View;

public abstract class SolveWithStepsPage : ManagedPage
{
    protected abstract StackPanel GetStepPanel();
    protected abstract ScrollViewer GetStepViewer();
    protected abstract IStepManagingPresenter GetStepsPresenter();
    
    public void AddStep(IStep step, StepState stepState)
    {
        GetStepPanel().Dispatcher.Invoke(() =>
        {
            var lc = new StepControl(step, stepState);
            GetStepPanel().Children.Add(lc);
            lc.OpenRequested += GetStepsPresenter().RequestStepOpening;
            lc.StateShownChanged += GetStepsPresenter().RequestStateShownChange;
            lc.PageSelector.PageChanged += GetStepsPresenter().RequestHighlightChange;
            lc.ExplanationAsked += () =>
            {
                var builder = GetStepsPresenter().RequestExplanation();
                if (builder is null) return;

                var window = new StepExplanationWindow(builder, GetExplanationDrawer());
                window.Show();
            };
        });
        GetStepViewer().Dispatcher.Invoke(() => GetStepViewer().ScrollToEnd());
    }

    public void ClearSteps()
    {
        GetStepPanel().Children.Clear();
    }

    public void OpenStep(int index)
    {
        if (index < 0 || index > GetStepPanel().Children.Count) return;
        if (GetStepPanel().Children[index] is not StepControl lc) return;
        
        lc.Open();
    }

    public void CloseStep(int index)
    {
        if (index < 0 || index > GetStepPanel().Children.Count) return;
        if (GetStepPanel().Children[index] is not StepControl lc) return;
        
        lc.Close();
    }

    public void SetStepsStateShown(StepState stepState)
    {
        foreach (var child in GetStepPanel().Children)
        {
            if (child is not StepControl lc) continue;

            lc.SetStateShown(stepState);
        }
    }
    
    protected abstract ISizeOptimizable GetExplanationDrawer();
}