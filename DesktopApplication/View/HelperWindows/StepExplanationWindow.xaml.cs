using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using DesktopApplication.Presenter.Sudokus.Solve;
using DesktopApplication.Presenter.Sudokus.Solve.Explanation;
using DesktopApplication.View.Controls;
using Model.Core.Explanation;

namespace DesktopApplication.View.HelperWindows;

public partial class StepExplanationWindow : IStepExplanationView
{
    private readonly IStepExplanationPresenter _presenter;
    private readonly bool _initialized;
    
    public StepExplanationWindow(IStepExplanationPresenterBuilder builder, ISizeOptimizable optimizableContent)
    {
        InitializeComponent();
        Embedded.OptimizableContent = optimizableContent;
        
        _presenter = builder.Build(this);
        _presenter.LoadStep();
        _initialized = true;
        
        TitleBar.RefreshMaximizeRestoreButton(WindowState);
        StateChanged += (_, _) => TitleBar.RefreshMaximizeRestoreButton(WindowState);
    }
    
    private void Minimize()
    {
        WindowState = WindowState.Minimized;
    }

    private void ChangeSize()
    {
        WindowState = WindowState == WindowState.Maximized ? WindowState.Normal : WindowState.Maximized;
    }
    
    public T GetDrawer<T>() where T : IDrawer
    {
        return (T)Embedded.OptimizableContent!;
    }

    public IExplanationHighlighter ExplanationHighlighter => (IExplanationHighlighter)Embedded.OptimizableContent!;

    public void ShowExplanation(ExplanationElement? start)
    {
        var tb = new TextBlock
        {
            FontSize = 15,
            Padding = new Thickness(5),
            TextWrapping = TextWrapping.Wrap,
        };
        
        tb.SetResourceReference(ForegroundProperty, "Text");

        if (start is null) tb.Text = "No explanation available for this step";
        else
        {
            do
            {
                var run = new Run
                {
                    Text = start.ToString()
                };
                
                run.SetResourceReference(ForegroundProperty, ThemeInformation.ResourceNameFor(start.Color));
                
                if (start.ShouldBeBold) run.FontWeight = FontWeights.Bold;

                if (start.DoesShowSomething)
                {
                    var currentForEvent = start;
                    run.MouseEnter += (_, _) => _presenter.ShowExplanationElement(currentForEvent);
                    run.MouseLeave += (_, _) => _presenter.StopShowingExplanationElement();
                }

                tb.Inlines.Add(run);
                
                start = start.Next;
            } while (start is not null);
        }

        Viewer.Content = tb;
    }

    private void OnFinished(object sender, RoutedEventArgs e)
    {
        Close();
    }

    private void HighlightOn(object sender, RoutedEventArgs e)
    {
        if(_initialized) _presenter.TurnOnHighlight();
    }
    
    private void HighlightOff(object sender, RoutedEventArgs e)
    {
        if(_initialized) _presenter.TurnOffHighlight();
    }
}