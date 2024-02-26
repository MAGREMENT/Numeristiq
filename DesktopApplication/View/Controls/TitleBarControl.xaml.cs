using System.Windows;
using System.Windows.Media;

namespace DesktopApplication.View.Controls;

public partial class TitleBarControl
{
    public event TitleBarAction? Minimize;
    public event TitleBarAction? ChangeSize;
    public event TitleBarAction? Close;

    private bool _allowResize = true;

    public bool AllowResize
    {
        set
        {
            _allowResize = value;
            if (value == false)
            {
                MaximizeButton.Visibility = Visibility.Collapsed;
                RestoreButton.Visibility = Visibility.Collapsed;
            }
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
        if (_allowResize) ChangeSize?.Invoke();
    }

    private void OnCloseButtonClick(object sender, RoutedEventArgs e)
    {
        Close?.Invoke();     
    }
    
    public void RefreshMaximizeRestoreButton(WindowState state)
    {
        if (!_allowResize) return;
        
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