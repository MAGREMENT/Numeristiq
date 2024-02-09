using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace NewView;

public partial class TitleBarControl : UserControl
{
    public event TitleBarAction? Minimize;
    public event TitleBarAction? Maximize;
    public event TitleBarAction? Close;

    public bool AllowResize
    {
        set
        {
            MaximizeButton.IsEnabled = value;
            RestoreButton.IsEnabled = value;
        }
    }

    public ImageSource AppIcon
    {
        set => Icon.Source = value;
    }

    public string AppTitle
    {
        set => Title.Text = value;
    }
    
    public TitleBarControl()
    {
        InitializeComponent();
        
        RenderOptions.SetBitmapScalingMode(Icon, BitmapScalingMode.Fant);
    }

    private void OnMinimizeButtonClick(object sender, RoutedEventArgs e)
    {
        Minimize?.Invoke();
    }

    private void OnMaximizeRestoreButtonClick(object sender, RoutedEventArgs e)
    {
        Maximize?.Invoke();
    }

    private void OnCloseButtonClick(object sender, RoutedEventArgs e)
    {
        Close?.Invoke();     
    }
    
    public void RefreshMaximizeRestoreButton(WindowState state)
    {
        if (state == WindowState.Maximized)
        {
            MaximizeButton.Visibility = Visibility.Collapsed;
            RestoreButton.Visibility = Visibility.Visible;
        }
        else
        {
            MaximizeButton.Visibility = Visibility.Visible;
            RestoreButton.Visibility = Visibility.Collapsed;
        }
    }
}

public delegate void TitleBarAction();