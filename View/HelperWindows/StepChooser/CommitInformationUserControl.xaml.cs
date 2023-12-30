using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Presenter.Translators;
using View.Utility;

namespace View.HelperWindows.StepChooser;

public partial class CommitInformationUserControl : UserControl
{
    public event OnHighlightShift? HighlightShifted;
    
    public CommitInformationUserControl()
    {
        InitializeComponent();
    }

    public void Show(ViewCommitInformation commit)
    {
        StrategyName.Text = commit.StrategyName;
        StrategyName.Foreground = new SolidColorBrush(ColorUtility.ToColor(commit.StrategyIntensity));
        StrategyChanges.Text = commit.Changes;
        
        HighlightsNumber.Text = commit.HighlightCursor;
        var shouldEnableArrows = commit.HighlightCount > 1;
        LeftArrow.IsEnabled = shouldEnableArrows;
        RightArrow.IsEnabled = shouldEnableArrows;

        Main.Visibility = Visibility.Visible;
    }

    public void StopShow()
    {
        Main.Visibility = Visibility.Hidden;
    }

    private void ShiftLeft(object sender, RoutedEventArgs e)
    {
        HighlightShifted?.Invoke(-1);
    }

    private void ShiftRight(object sender, RoutedEventArgs e)
    {
        HighlightShifted?.Invoke(1);
    }
}

public delegate void OnHighlightShift(int num);