using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using DesktopApplication.Presenter.Sudoku.Solve;
using DesktopApplication.Presenter.Sudoku.Solve.Explanation;
using DesktopApplication.View.Utility;
using Model.Sudoku.Solver.Explanation;

namespace DesktopApplication.View.HelperWindows;

public partial class StepExplanationWindow : IStepExplanationView
{ 
    public StepExplanationWindow(StepExplanationPresenterBuilder builder)
    {
        InitializeComponent();
        var presenter = builder.Build(this);
        presenter.Initialize();
        
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

    public ISudokuSolverDrawer Drawer => Board;
    
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
                    Foreground = ColorUtility.ToBrush(start.Color),
                    Text = start.ToString()
                };
                if (start.ShouldBeBold) run.FontWeight = FontWeights.Bold;

                var currentForEvent = start;
                run.MouseEnter += (_, _) =>
                {
                    currentForEvent.Show(Board);
                    Board.Refresh();
                };
                run.MouseLeave += (_, _) =>
                {
                    Board.ClearHighlights();
                    Board.Refresh();
                };

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
}