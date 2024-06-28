using System.Windows;
using System.Windows.Threading;
using DesktopApplication.Presenter.Nonograms;
using DesktopApplication.Presenter.Nonograms.Solve;
using DesktopApplication.View.Controls;
using Model.Core.Steps;

namespace DesktopApplication.View.Nonograms.Pages;

public partial class SolvePage : INonogramSolveView
{
    private readonly NonogramSolvePresenter _presenter;
    
    private int _logOpen;

    public INonogramDrawer Drawer => (INonogramDrawer)EmbeddedDrawer.OptimizableContent!;

    public SolvePage(NonogramApplicationPresenter presenter)
    {
        InitializeComponent();

        _presenter = presenter.Initialize(this);
    }
    
    public void AddLog(IStep step, StateShown stateShown)
    {
        LogPanel.Dispatcher.Invoke(() =>
        {
            var lc = new StepControl(step, stateShown);
            LogPanel.Children.Add(lc);
            /*lc.OpenRequested += _presenter.RequestLogOpening;
            lc.StateShownChanged += _presenter.RequestStateShownChange;
            lc.HighlightShifted += _presenter.RequestHighlightShift;*/
        });
        LogViewer.Dispatcher.Invoke(() => LogViewer.ScrollToEnd());
    }

    public void ClearLogs()
    {
        LogPanel.Children.Clear();
    }

    public void OpenLog(int index)
    {
        if (index < 0 || index > LogPanel.Children.Count) return;
        if (LogPanel.Children[index] is not StepControl lc) return;

        _logOpen = index;
        lc.Open();
    }

    public void CloseLogs()
    {
        if (_logOpen < 0 || _logOpen >= LogPanel.Children.Count) return;
        if (LogPanel.Children[_logOpen] is not StepControl lc) return;

        _logOpen = -1;
        lc.Close();
    }

    public void SetLogsStateShown(StateShown stateShown)
    {
        foreach (var child in LogPanel.Children)
        {
            if (child is not StepControl lc) continue;

            lc.SetStateShown(stateShown);
        }
    }

    public void SetCursorPosition(int index, string s)
    {
        if (index < 0 || index > LogPanel.Children.Count) return;
        if (LogPanel.Children[index] is not StepControl lc) return;

        lc.SetCursorPosition(s);
    }
    
    public void ShowNonogramAsString(string s)
    {
        TextBox.SetText(s);
    }

    private void Solve(object sender, RoutedEventArgs e)
    {
        _presenter.Solve(false);
    }

    private void Advance(object sender, RoutedEventArgs e)
    {
        _presenter.Solve(true);
    }

    private void CreateNewNonogram(string s)
    {
        _presenter.SetNewNonogram(s);
    }

    private void OnHideableTextboxShowed()
    {
        _presenter.ShowNonogramAsString();
    }
}