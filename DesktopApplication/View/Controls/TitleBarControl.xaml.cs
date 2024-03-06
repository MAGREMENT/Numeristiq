using System.Windows;
using System.Windows.Media;

namespace DesktopApplication.View.Controls;

public partial class TitleBarControl
{
    public event TitleBarAction? Minimize;
    public event TitleBarAction? ChangeSize;
    public event TitleBarAction? Close;

    private bool _allowResize = true;

    public object InsideContent
    {
        set => ContentPresenter.Content = value;
        get => ContentPresenter.Content;
    }

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
        set
        {
            Icon.Source = value;
            Icon.Visibility = Visibility.Visible;
        }
    }

    public string AppTitle
    {
        set
        {
            Title.Text = value;
            Title.Visibility = Visibility.Visible;
        } 
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