using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using DesktopApplication.Presenter;
using Model.Core.Changes;
using Model.Core.Highlighting;
using Model.Core.Steps;

namespace DesktopApplication.View.Controls;

public partial class StepControl
{
    private readonly int _id;
    private bool _shouldCallStateShownEvent = true;

    public event OnOpenRequest? OpenRequested;
    public event OnStateShownChange? StateShownChanged;
    public event OnExplanationAsked? ExplanationAsked;
    
    public StepControl(IStep step, StateShown stateShown)
    {
        InitializeComponent();

        _id = step.Id;
        Number.Text = step.Id.ToString();
        Title.Text = step.Title;
        Title.SetResourceReference(ForegroundProperty, ThemeInformation.ResourceNameFor(step.Difficulty));
        PageSelector.Max = step.HighlightCount();
        SetStateShown(stateShown);
        TextOutput.Text = step.Description;
    }

    public StepControl(int id, BuiltChangeCommit<NumericChange, ISudokuHighlighter> commit) //TODO use
    {
        InitializeComponent();
        
        _id = id;
        Number.Text = id.ToString();
        Title.Text = commit.Maker.Name;
        Title.SetResourceReference(ForegroundProperty, ThemeInformation.ResourceNameFor(commit.Maker.Difficulty));
        PageSelector.Max = commit.Report.HighlightCollection.Count;
        SetStateShown(StateShown.Before);
        TextOutput.Text = commit.Report.Description;
        ExplanationButton.Visibility = Visibility.Collapsed;
    }

    private void OnMouseEnter(object sender, MouseEventArgs e)
    {
        if (sender is not Border b) return;

        b.SetResourceReference(BorderBrushProperty, "Primary");
    }

    private void OnMouseLeave(object sender, MouseEventArgs e)
    {
        if (sender is not Border b) return;

        b.SetResourceReference(BorderBrushProperty, "Background1");
    }

    public void Open()
    {
        BottomPart.Visibility = Visibility.Visible;
    }

    public void Close()
    {
        BottomPart.Visibility = Visibility.Collapsed;
    }

    public void SetStateShown(StateShown stateShown)
    {
        _shouldCallStateShownEvent = false;
        if (stateShown == StateShown.Before) BeforeButton.IsChecked = true;
        else AfterButton.IsChecked = true;
        _shouldCallStateShownEvent = true;
    }

    private void OnClick(object sender, MouseButtonEventArgs e)
    {
        OpenRequested?.Invoke(_id);
    }

    private void BeforeChecked(object sender, RoutedEventArgs e)
    {
        if (_shouldCallStateShownEvent) StateShownChanged?.Invoke(StateShown.Before);
    }
    
    private void AfterChecked(object sender, RoutedEventArgs e)
    {
        if (_shouldCallStateShownEvent) StateShownChanged?.Invoke(StateShown.After);
    }

    private void OnExplanationAsked(object sender, RoutedEventArgs e)
    {
        ExplanationAsked?.Invoke();
    }
}

public delegate void OnOpenRequest(int id);
public delegate void OnStateShownChange(StateShown stateShown);
public delegate void OnExplanationAsked();