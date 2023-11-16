using System.Windows;
using System.Windows.Media;
using Global.Enums;
using Presenter.Translator;
using View.Utils;

namespace View.Pages.Solver.UserControls;

public partial class LogUserControl
{ 
    public delegate void OnStateShowChanged(StateShown ss);
    public event OnStateShowChanged? ShownTypeChanged;

    private bool _invokeStateShowEvent = true;
    
    public LogUserControl()
    {
        InitializeComponent();

        Main.MouseEnter += (_, _) =>
        {
            Main.Background = new SolidColorBrush(Colors.White);
        };
        Main.MouseLeave += (_, _) =>
        {
            Main.Background = new SolidColorBrush(Colors.WhiteSmoke);
        };
    }

    public void InitLog(ViewLog log)
    {
        Number.Text = "#" + log.Id;
        Title.Foreground = new SolidColorBrush(ColorManager.ToColor(log.Intensity));
        Title.Text = log.Title;
        Text.Text = log.Changes;

        HighlightsNumber.Text = log.HighlightCursor;
        if (log.HighlightCount == 1)
        {
            LeftArrow.IsEnabled = false;
            RightArrow.IsEnabled = false;
        }
    }

    public void CurrentlyShowed()
    {
        Highlights.Visibility = Visibility.Visible;
    }

    public void NotShowedAnymore()
    {
        Highlights.Visibility = Visibility.Hidden;
    }

    private void ShiftLeft(object sender, RoutedEventArgs e)
    {
        //TODO
    }

    private void ShiftRight(object sender, RoutedEventArgs e)
    {
        //TODO
    }

    private void TypeBefore_OnChecked(object sender, RoutedEventArgs e)
    {
        if(_invokeStateShowEvent) ShownTypeChanged?.Invoke(StateShown.Before);
    }

    private void TypeAfter_OnChecked(object sender, RoutedEventArgs e)
    {
        if(_invokeStateShowEvent) ShownTypeChanged?.Invoke(StateShown.After);
    }

    public void SetShownType(StateShown type)
    {
        _invokeStateShowEvent = false;
        if (type == StateShown.After) TypeAfter.IsChecked = true;
        else TypeBefore.IsChecked = true;
        _invokeStateShowEvent = true;
    }
}