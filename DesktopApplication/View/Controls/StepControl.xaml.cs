using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using DesktopApplication.Presenter;
using Model.Core.Changes;
using Model.Core.Highlighting;
using Model.Sudokus.Solver.Highlighting;
using Model.Core.Steps;

namespace DesktopApplication.View.Controls;

public partial class StepControl
{
    private readonly int _id;
    private bool _shouldCallStateShownEvent = true;

    public event OnOpenRequest? OpenRequested;
    public event OnStateShownChange? StateShownChanged;
    public event OnExplanationAsked? ExplanationAsked;
    
    public StepControl(int id, IStep step, StepState stepState)
    {
        InitializeComponent();

        _id = id;
        Number.Text = id.ToString();
        Title.Text = step.Title;
        Title.SetResourceReference(ForegroundProperty, ThemeInformation.ResourceNameFor(step.Difficulty));
        PageSelector.Max = step.HighlightCount();
        SetStateShown(stepState);
        TextOutput.Text = step.Description;
    }

    public void Open()
    {
        BottomPart.Visibility = Visibility.Visible;
    }

    public void Close()
    {
        BottomPart.Visibility = Visibility.Collapsed;
    }

    public void SetStateShown(StepState stepState)
    {
        _shouldCallStateShownEvent = false;
        if (stepState == StepState.From) BeforeButton.IsChecked = true;
        else AfterButton.IsChecked = true;
        _shouldCallStateShownEvent = true;
    }

    private void OnClick(object sender, MouseButtonEventArgs e)
    {
        OpenRequested?.Invoke(_id);
    }

    private void BeforeChecked(object sender, RoutedEventArgs e)
    {
        if (_shouldCallStateShownEvent) StateShownChanged?.Invoke(StepState.From);
    }
    
    private void AfterChecked(object sender, RoutedEventArgs e)
    {
        if (_shouldCallStateShownEvent) StateShownChanged?.Invoke(StepState.To);
    }

    private void OnExplanationAsked(object sender, RoutedEventArgs e)
    {
        ExplanationAsked?.Invoke();
    }
}

public delegate void OnOpenRequest(int id);
public delegate void OnStateShownChange(StepState stepState);
public delegate void OnExplanationAsked();