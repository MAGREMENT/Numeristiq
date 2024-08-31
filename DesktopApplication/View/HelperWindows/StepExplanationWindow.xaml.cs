using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using DesktopApplication.Presenter;
using DesktopApplication.View.Controls;
using Model.Core.Explanations;

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

   public void ShowExplanation<T>(Explanation<T> e)
    {
        var tb = new TextBlock
        {
            FontSize = 15,
            Padding = new Thickness(5),
            TextWrapping = TextWrapping.Wrap,
        };
        
        tb.SetResourceReference(ForegroundProperty, "Text");

        if (e.Count == 0) tb.Text = "No explanation available for this step";
        else
        {
            foreach (var element in e)
            {
                var run = new Run
                {
                    Text = e.ToString()
                };
                
                run.SetResourceReference(ForegroundProperty, ThemeInformation.ResourceNameFor(element.Color));
                
                if (element.ShouldBeBold) run.FontWeight = FontWeights.Bold;

                if (element.DoesShowSomething)
                {
                    var currentForEvent = e;
                    run.MouseEnter += (_, _) => _presenter.ShowExplanationElement(currentForEvent);
                    run.MouseLeave += (_, _) => _presenter.StopShowingExplanationElement();
                }

                tb.Inlines.Add(run);
            }
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